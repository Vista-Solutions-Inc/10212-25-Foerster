using DevicesLibrary;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Wpf.Ui.Controls;
using System.Windows.Forms.Design;
using Foerster.Models.Managers;
using HalconDotNet;
using Wpf.Ui;

namespace Foerster.ViewModels
{
    public abstract partial class DeviceVisual : ObservableObject
    {
        private readonly ISnackbarService _snackbarService;
        public ISnackbarService SnackbarService { get { return _snackbarService; } }

        // Properties 
        [ObservableProperty] private IDevice _device;
        [ObservableProperty] private string _deviceName;
        [ObservableProperty] private string _deviceStatus;
        [ObservableProperty] private string _settingsFilePath;
        [ObservableProperty] string _exposureTime;

        [ObservableProperty] private InfoBadgeSeverity _badgeSeverity = InfoBadgeSeverity.Informational;
        [ObservableProperty] private ImageSource _deviceImageSource;
        [ObservableProperty] private SolidColorBrush _tileBackground = new SolidColorBrush();
        [ObservableProperty] private DeviceTypes _deviceType;

        // Constructors 
        public DeviceVisual(IDevice device, ISnackbarService snackbarService)
        {
            _snackbarService = snackbarService;
            Device = device;
            DeviceName = device.DeviceName;
            DeviceStatus = device.DeviceStatus.ToString();
            // set badge color by device status
            UpdateBadgeStatus(device.DeviceStatus);
            // set default background brush 
            TileBackground = new SolidColorBrush(Color.FromArgb(120, 224, 224, 224));
        }
        // Methods
        public abstract void OnLoosingFocus();
        public abstract void OnGainingFocus();
        public void UpdateBadgeStatus(DeviceStatus deviceStatus)
        {
            // set device badge color by device status 
            switch (deviceStatus)
            {
                case DevicesLibrary.DeviceStatus.Unknown:
                    BadgeSeverity = InfoBadgeSeverity.Attention;
                    break;
                case DevicesLibrary.DeviceStatus.Disconnected:
                    BadgeSeverity = InfoBadgeSeverity.Informational;
                    break;
                case DevicesLibrary.DeviceStatus.Initialized:
                    BadgeSeverity = InfoBadgeSeverity.Informational;
                    break;
                case DevicesLibrary.DeviceStatus.Started:
                    BadgeSeverity = InfoBadgeSeverity.Success;
                    break;
                case DevicesLibrary.DeviceStatus.Error:
                    BadgeSeverity = InfoBadgeSeverity.Critical;
                    break;
                default:
                    BadgeSeverity = InfoBadgeSeverity.Informational;
                    break;
            }
        }

        [RelayCommand] private async void OnReconnectDevice(object content)
        {
            if (!Device.IsEmulated && Device.DeviceStatus != DevicesLibrary.DeviceStatus.Started)
            {
                if (Device.DeviceStatus == DevicesLibrary.DeviceStatus.Error)
                {
                    await Device.DisconnectAsync();
                }
                await Device.ConnectAsync();
            }        }
    }
    public class Errors
    {
        private string _message;
        private string _time;

        public string Message { get { return _message; } set { _message = value; } }
        public string Time { get { return _time; } set { _time = value; } }
        public Errors(string message, string time)
        {
            this.Message = message;
            this.Time = time;
        }
    }

}
