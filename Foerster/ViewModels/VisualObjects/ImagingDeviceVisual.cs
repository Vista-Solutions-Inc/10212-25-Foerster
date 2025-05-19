using DevicesLibrary;
using HalconDotNet;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using VistaControls.Display;
using System.Windows.Forms;
using DevicesLibrary.Devices.Imaging;

namespace Foerster.ViewModels.VisualObjects
{
    public partial class ImagingDeviceVisual : DeviceVisual
    {
        private bool _isFirstError = true;
        private bool _setLive = false;
        private int _snackBarDuration = 4;
        [ObservableProperty] HImage _currentImage;
        Dictionary<string, HImage> _multiImage;
        [ObservableProperty] Visibility _isMultyImageCamera = Visibility.Collapsed;
        [ObservableProperty] List<string> _imagesTypesList;
        [ObservableProperty] string _selectedImagesType;
        [ObservableProperty] private string _calibrationStatus;
        [ObservableProperty] LutDisplayTypes _lutDisplayType = LutDisplayTypes.Default;
        [ObservableProperty] SolidColorBrush _LiveButtonBrush = new SolidColorBrush(Colors.Transparent);
        [ObservableProperty] string _exposureTime;
        [ObservableProperty] ObservableCollection<Errors> _errorCodes = new ObservableCollection<Errors>();
        [ObservableProperty] Visibility _calibrateButtonVisibility = Visibility.Visible;
        [ObservableProperty] string _emulationFolderPath = "";
        public bool SetLive { get { return _setLive; } set { _setLive = value; } }

        public ImagingDeviceVisual(IDevice device, ISnackbarService snackbarService) : base(device, snackbarService)
        {
            this.DeviceType = DeviceTypes.ImagingDevice;
            ImagingDevice imagingDevice = (ImagingDevice)device;

            IsMultyImageCamera = (imagingDevice.ImagingDeviceCaptureType == ImagingDeviceCaptureTypes.MultiImageCapture) ? Visibility.Visible : Visibility.Collapsed;
            _imagesTypesList = imagingDevice.ImagesTypesList;
            _selectedImagesType = (_imagesTypesList.Count > 0) ? _imagesTypesList[0] : "";
            CalibrationStatus = imagingDevice.IsCalibrated ? "Completed" : "Missing";
            SettingsFilePath = imagingDevice.CameraSettingsPath;
            EmulationFolderPath = imagingDevice.GetFrameGrabberSettingsParameter(this, new FrameGrabberParameterUpdatedEventArgs ( false,"CameraType") );


            _exposureTime = imagingDevice.ExposureTime.ToString();
            imagingDevice.DeviceConnectedEvent += DeviceConnected;
            imagingDevice.DeviceDisconnectedEvent += DeviceDisconnected;
            imagingDevice.DeviceEmulatedEvent += DeviceEmulated;
            imagingDevice.ErrorDeviceEvent += DeviceError;
            // set the background brush by imaging device type
            TileBackground = new SolidColorBrush(Color.FromArgb(120, 204, 229, 255));
            _errorCodes.Add(new Errors("                    ", "                    "));
            Type imagingDeviceType = device.GetType();
            string typeString = imagingDeviceType.Name;
            switch (typeString)
            {
                case "Generic2DCamera":
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/GenericIndustrialCamS.png"));
                    break;
                case "Photoneo3DScanner":
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/PhotoneoS.png"));
                    break;
                case "SickRulerCamera":
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/SickRulerS.png"));
                    break;
                case "CA_H500CX":
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/CA_H500CX.png"));
                    break;
                default:
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/GenericIndustrialCamS.png"));
                    break;
            }
        }
        // Methods
        private void DeviceConnected(object sender, EventArgs e)
        {
            DeviceStatus = DevicesLibrary.DeviceStatus.Started.ToString();
            BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Success;
            ImagingDevice imagingDevice = Device as ImagingDevice;
            ExposureTime = imagingDevice.GetDeviceParameter(this, new ParameterUpdatedEventArgs("ExposureTime")).ToString();
        }
        private void DeviceDisconnected(object sender, EventArgs e)
        {
            DeviceStatus = DevicesLibrary.DeviceStatus.Disconnected.ToString();
            BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Informational;
        }
        private void DeviceEmulated(object sender, EventArgs e)
        {
            DeviceStatus = DevicesLibrary.DeviceStatus.Emulated.ToString();
            BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Informational;
        }
        private void DeviceError(object sender, EventArgs e)
        {
            DeviceStatus = DevicesLibrary.DeviceStatus.Error.ToString();
            BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Critical;
            DeviceErrorEventArgs args = (DeviceErrorEventArgs)e;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                if (_isFirstError)
                {
                    ErrorCodes.Clear();
                    _isFirstError = false;
                }
                if ((Device as ImagingDevice).IsLive)
                {
                    (Device as ImagingDevice).StopCaptureImageLiveAsync(this, new EventArgs());
                    LiveButtonBrush = new SolidColorBrush(Colors.Transparent);
                    _setLive = false;
                    // Unregister from event 
                    (Device as ImagingDevice).ImageCapturedEvent -= UpdateImageDisplay;
                }
                ErrorCodes.Add(new Errors(args.ErrorMessage, DateTime.Now.ToString()));
            }));
        }
        [RelayCommand] private void OnSnapImage(object content)
        {
            ImagingDevice imagingDevice = (ImagingDevice)Device;
            // Register to image available event
            imagingDevice.ImageCapturedEvent += UpdateImageDisplay;
            // Capture Image
            imagingDevice.CaptureImage(this, new EventArgs());  
            // Unregister from event 
            imagingDevice.ImageCapturedEvent -= UpdateImageDisplay;
        }
        [RelayCommand] private void OnLiveCapture(object content)
        {
            AlternateLiveCaptureState();
        }
        [RelayCommand] private void OnUpdateParameters(object content) {
            ImagingDevice imagingDevice = (ImagingDevice)Device;
            bool tempIsLive = imagingDevice.IsLive;
            if (tempIsLive)
            {
                AlternateLiveCaptureState();
            }
            try
            {
                Int32.TryParse(ExposureTime, out int Result);
                if (Result <= 0 || Result > 100000)
                {
                    this.SnackbarService.Show("Exposure Time", "The exposure time modification failed.", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(_snackBarDuration));
                }
                else
                {
                    if (imagingDevice.IsEmulated)
                    {
                        this.SnackbarService.Show("Exposure Time", "The Exposure Time cannot be changed if the camera is emulated.", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(_snackBarDuration));
                        return;
                    }
                    imagingDevice.ExposureTime = Result;
                    this.SnackbarService.Show("Exposure Time", "Exposure Time changed successfully.", ControlAppearance.Success, new SymbolIcon(SymbolRegular.CheckmarkCircle24), TimeSpan.FromSeconds(_snackBarDuration));
                }
            }
            catch
            {
                this.SnackbarService.Show("Exposure Time", "The exposure time modification failed.", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(_snackBarDuration));
            }
            if (tempIsLive)
            {
                AlternateLiveCaptureState();
            }
        }
        [RelayCommand] private void OnAdvancedParameters(object content)
        {

        }
        [RelayCommand] private async void OnCalibrateDevice(object content)
        {
            throw new NotImplementedException();
        }

        [RelayCommand]
        private async void OnSetNewEmulationFolderPath(object content)
        {
            ImagingDevice imagingDevice = (ImagingDevice)Device;
            using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog ())
            {
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string folderName = folderBrowserDialog1.SelectedPath;
                    
                    imagingDevice.SetFrameGrabberSettingsParameter(this, new FrameGrabberParameterUpdatedEventArgs(false, "CameraType", folderName));
                }
            }
            EmulationFolderPath = imagingDevice.GetFrameGrabberSettingsParameter(this, new FrameGrabberParameterUpdatedEventArgs(false, "CameraType"));


        }
        partial void OnLutDisplayTypeChanged(LutDisplayTypes value)
        {
            var temp = CurrentImage;
            CurrentImage = new HImage();
            CurrentImage = temp;
        }
        private void AlternateLiveCaptureState()
        {
            ImagingDevice imagingDevice = (ImagingDevice)Device;
            if (!imagingDevice.IsLive && (imagingDevice.IsConnected || imagingDevice.IsEmulated))
            {
                // Register to image available event
                imagingDevice.ImageCapturedEvent += UpdateImageDisplay;
                imagingDevice.StartCaptureImageLiveAsync(this, new EventArgs());
                LiveButtonBrush = new SolidColorBrush(Color.FromArgb(200, 222, 49, 99));
                _setLive = true;
            }
            else
            {
                imagingDevice.StopCaptureImageLiveAsync(this, new EventArgs());
                LiveButtonBrush = new SolidColorBrush(Colors.Transparent);
                _setLive = false;
                // Unregister from event 
                imagingDevice.ImageCapturedEvent -= UpdateImageDisplay;
            }
        }
        public void UpdateImageDisplay(object sender, EventArgs e)
        {
            if (e.GetType() == typeof(ImageCaptureEventArgs))
            {
                CurrentImage = ((ImageCaptureEventArgs)e).Image;
            }
            else if (e.GetType() == typeof(MultiImageCaptureEventArgs))
            {
                _multiImage = ((MultiImageCaptureEventArgs)e).Image;
                if (_multiImage == null || string.IsNullOrEmpty(SelectedImagesType))
                {
                    CurrentImage = new HImage();
                    return;
                }
                CurrentImage = _multiImage[SelectedImagesType];

            }
        }
        partial void OnSelectedImagesTypeChanged(string value)
        {
            if (_multiImage == null || string.IsNullOrEmpty(SelectedImagesType))
            {
                CurrentImage = new HImage();
                return;
            }

            CurrentImage = _multiImage[SelectedImagesType];
        }
        public override void OnLoosingFocus()
        {
            if (_setLive)
            {
                AlternateLiveCaptureState();
                // Reset Display
                CurrentImage = new HImage();
            }
        }
        public override void OnGainingFocus()
        {
            ImagingDevice imagingDevice = (ImagingDevice)Device;
            // Update status 
            DeviceStatus = Device.DeviceStatus.ToString();
            // set badge color by device status
            UpdateBadgeStatus(Device.DeviceStatus);
            // Update  parameters
            ExposureTime = imagingDevice.ExposureTime.ToString();
        }

    }
}
