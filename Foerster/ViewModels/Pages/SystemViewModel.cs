
using DevicesLibrary;
using DevicesLibrary.Devices.Imaging;
using Foerster.Models.Managers;
using System.Collections.ObjectModel;
using Wpf.Ui;
using Foerster.ViewModels.VisualObjects;
using Foerster.Models.System;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;
using VistaControls.UsersManagement;


namespace Foerster.ViewModels.Pages
{
    public partial class SystemViewModel : ObservableObject, INavigationAware
    {
        private readonly ISnackbarService _snackbarService;
        private SystemManager _systemManager;
        private SystemConfiguration _systemConfiguration;
        private UserManager _userManager;

        public ISnackbarService SnackbarService { get { return _snackbarService; } }
        public int SnackbarDuration = 4;

        [ObservableProperty] ObservableCollection<DeviceVisual> _deviceList = new ObservableCollection<DeviceVisual>();
        [ObservableProperty] DeviceVisual _selectedDevice;

        [ObservableProperty] Visibility _maintenanceVisibility;
        [ObservableProperty] bool _maintenanceEnabled;
        // Constructors
        public SystemViewModel(ISnackbarService snackbarService) 
        {
            // get system manager instance
            _systemManager = App.GetService<SystemManager>();
            _systemConfiguration = SystemConfiguration.Instance;
            _systemConfiguration.RunModeUpdateCompletedEvent += UpdateRunMode;
            _snackbarService = snackbarService;

            // load device list
            UpdateDeviceList();


            // subscribe to user change event and update accordingly
            _userManager = App.GetService<UserManager>();
            _userManager.UserLevelUpdatedEvent += OnAccessLevelChanged;
            UpdateVisibilityByAccessLevel(_userManager.CurrentUserRole.ToString());
        }

        // Methods
        // --local methods--
        private void UpdateDeviceList()
        {
            DeviceList.Clear();
            foreach (KeyValuePair<string, IDevice> entry in _systemManager.SystemDevices)
            {
                IDevice device = entry.Value;
                DeviceVisual visualDevice;
                switch (device.DeviceType)
                {
                    case (DeviceTypes.ImagingDevice):
                        visualDevice = new ImagingDeviceVisual(device, _snackbarService);
                        DeviceList.Add(visualDevice);
                        break;
                    case (DeviceTypes.ScanningDevice):
                        visualDevice = new ScanningDeviceVisual(device , _snackbarService);
                        DeviceList.Add(visualDevice);
                        break;
                    case (DeviceTypes.RobotDeviceFullControl):
                        visualDevice = new RobotDeviceFullControlVisual(device, _snackbarService);
                        DeviceList.Add(visualDevice);
                        break;
                    case (DeviceTypes.RobotDeviceLimitedControl):
                        visualDevice = new RobotDeviceLimitedControlVisual(device, _snackbarService);
                        DeviceList.Add(visualDevice);
                        break;
                }
            }
        }

        private void UpdateRunMode(object? sender, EventArgs e) {
            RunMode runMode = _systemConfiguration.RunMode;
            switch (runMode) {
                case RunMode.online: {
                        foreach (DeviceVisual deviceVisual in DeviceList) {
                            if (deviceVisual.DeviceType == DeviceTypes.ImagingDevice) {
                                ImagingDeviceVisual imagingDeviceVisual = deviceVisual as ImagingDeviceVisual;
                                ImagingDevice imagingDevice = imagingDeviceVisual.Device as ImagingDevice;
                                try {
                                    imagingDevice.DeviceConnection.GoOnline();
                                    imagingDeviceVisual.BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Success;
                                    imagingDeviceVisual.DeviceStatus = "Started";
                                }
                                catch {
                                    imagingDeviceVisual.BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Critical;
                                    imagingDeviceVisual.DeviceStatus = "Error";
                                }
                            }
                        }
                        break;
                    }
                case RunMode.offline: {
                        foreach (DeviceVisual deviceVisual in DeviceList) {
                            if (deviceVisual.DeviceType == DeviceTypes.ImagingDevice) {
                                ImagingDeviceVisual imagingDeviceVisual = deviceVisual as ImagingDeviceVisual;
                                ImagingDevice imagingDevice = imagingDeviceVisual.Device as ImagingDevice;
                                try {
                                    imagingDevice.DeviceConnection.GoOffline();
                                    imagingDeviceVisual.BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Informational;
                                    imagingDeviceVisual.DeviceStatus = "Emulated";
                                }
                                catch {
                                    imagingDeviceVisual.BadgeSeverity = Wpf.Ui.Controls.InfoBadgeSeverity.Critical;
                                    imagingDeviceVisual.DeviceStatus = "Error";
                                }
                            }
                        }
                        break;
                    }
            }
        }
        // --On change event methods--
        partial void OnSelectedDeviceChanged(DeviceVisual? oldValue, DeviceVisual newValue)
        {
            oldValue?.OnLoosingFocus();
            newValue.OnGainingFocus();
        }

        #region Navigation aware
        public async Task OnNavigatedToAsync()
        {
            if (SelectedDevice != null)
            {
                SelectedDevice.OnGainingFocus();
            }
        }

        public async Task OnNavigatedFromAsync()
        {
            for (int i = 0; i < DeviceList.Count; i++)
            {
                if (DeviceList[i].DeviceType == DevicesLibrary.DeviceTypes.ImagingDevice)
                {
                    ImagingDevice imagingDevice = DeviceList[i].Device as ImagingDevice;
                    if (imagingDevice.IsLive)
                    {
                        imagingDevice.ImageCapturedEvent -= (DeviceList[i] as ImagingDeviceVisual).UpdateImageDisplay;
                        imagingDevice.StopCaptureImageLiveAsync(this, new EventArgs());
                        (DeviceList[i] as ImagingDeviceVisual).SetLive = false;
                        (DeviceList[i] as ImagingDeviceVisual).LiveButtonBrush = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
        }
        #endregion


        #region User Control
        private void OnAccessLevelChanged(object? sender, EventArgs e)
        {
            UserLevelEventArgs? args = e as UserLevelEventArgs;
            if (args == null) { return; }
            UpdateVisibilityByAccessLevel(args.UserLevel);
        }
        private void UpdateVisibilityByAccessLevel(string userLevel)
        {
            switch (userLevel)
            {
                case "Operator":
                    MaintenanceVisibility = Visibility.Collapsed;
                    MaintenanceEnabled = false;
                    break;
                case "Supervisor":
                    MaintenanceVisibility = Visibility.Visible;
                    MaintenanceEnabled = true;
                    break;
                case "Administrator":
                    MaintenanceVisibility = Visibility.Visible;
                    MaintenanceEnabled = true;
                    break;
            }
        }
        #endregion
    }

}


