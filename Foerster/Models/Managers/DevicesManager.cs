using DevicesLibrary;
using DevicesLibrary.Devices.Imaging;
using MessageBusLibrary;
using MessageBusLibrary.Interfaces;
using System.Xml.Linq;
using VistaHelpers.FileManagement;
using Foerster.Models.System;


namespace Foerster.Models.Managers
{
    /// <summary>
    /// The Device Manager class is our main connection to all system's devices.
    /// This alleviates the load of the system manager and allows us to develop specialized device handling methods 
    /// Eventually, we should be able to use this Manager to go further and discover devices in selected interfaces &
    /// present them to the System manager to be paired with its internal functionalities. 
    /// </summary>
    /// 
    public class DevicesManagerErrorInfo
    {
        DevicesManagerErrorLevels _errorLevel;
        string _details;

        public DevicesManagerErrorLevels ErrorLevel { get { return _errorLevel; } }
        public string Details { get { return _details; } }

        public DevicesManagerErrorInfo(DevicesManagerErrorLevels errorLevel, string details)
        {
            _errorLevel = errorLevel;
            _details = details;
        }
    }
    public enum DevicesManagerErrorLevels
    {
        Fatal,
        Warning,
        Info,
    }

    public enum DevicesManagerStatusTypes
    {
        Ok,
        Error,
    }
    public class DevicesManager : IGenericMessageBusConnector
    {
        // Events 
        EventHandler RegisterDeviceErrorEvent;
        public event EventHandler? DevicesRunModeUpdatedEvent;

        // Fields
        DevicesManagerStatusTypes _status;
        DevicesManagerErrorInfo _errorInfo;

        // --General--
        private Dictionary<string, IDevice> _deviceRegistry = new Dictionary<string, IDevice>();
        private string _devicesManagerSettings;
        private List<string> _listDevicesTypes;
        private List<string> _listDevicesSettings;
        bool _emulateDevices;

        // --Bus Connection--
        private string _connectorName = nameof(DevicesLibrary.DeviceHub);
        private DeviceHub _deviceHub;
        private MessageProcessor _messageProcessor;


        // Properties
        public DeviceHub DeviceHub { get { return _deviceHub; } }
        public Dictionary<string, IDevice> DeviceRegistry { get { return _deviceRegistry; } }

        // -- For Bus Connection --
        public string ConnectorName { get { return _connectorName; } }
        public MessageProcessor MessageProcessor { get { return _messageProcessor; } }
        public DevicesManagerStatusTypes DevicesManagerStatus { get { return _status; } }

        // Constructor
        public DevicesManager()
        {
            _status = DevicesManagerStatusTypes.Ok;
            _deviceHub = new DeviceHub();
            ConnectModuleToBus();
        }

        // Methods
        public void ConnectModuleToBus()
        {
            _messageProcessor = new MessageProcessor(MessageBus.Instance, this, _connectorName);
            _messageProcessor.PlugAnnouncementInMethod(this, AnnouncementType.DevicesOperationModeChangeRequested, SwitchDevicesRunMode);
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DevicesModeUpdated, handler => DevicesRunModeUpdatedEvent += handler);
            ConnectHubToProcessor();
        }
        private void ConnectHubToProcessor()
        {
            _messageProcessor.SubscribeToBus(_deviceHub, "DeviceHub");

            //add basic hub connections for the Bus (all IDevice generic methods and events) 
            // Plug Events 
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceConnected, handler => _deviceHub.DeviceConnectedEvent += handler);
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceDisconnected, handler => _deviceHub.DeviceDisconnectedEvent += handler);
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceReferenceSent, handler => _deviceHub.SendRequestedDeviceEvent += handler);

            _messageProcessor.PlugEventInError(ErrorLevel.Error, StandardErrors.DeviceError, handler => _deviceHub.DeviceErrorEvent += handler);
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RequestedDeviceInfo, handler => _deviceHub.SendRequestedDeviceInfoEvent += handler);
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.ReconnectAllDevicesCompleted, handler => _deviceHub.ReconnectAllDevicesCompletedEvent += handler);
            // Plug Commands (Methods)
            _messageProcessor.PlugCommandInMethod(_deviceHub, CommandType.ReconnectAllDevices, _deviceHub.ConnectAllDevices);
            _messageProcessor.PlugCommandInMethod(_deviceHub, CommandType.GetDeviceInfo, _deviceHub.GetDeviceInfo);
            _messageProcessor.PlugCommandInMethod(DeviceHub, CommandType.GetDevice, _deviceHub.GetDevice);

        }
        public async Task InitializeDevices(RunMode runMode, string currentPath)
        {
            _status = DevicesManagerStatusTypes.Ok;
            _emulateDevices = false;
            ReadSettings(currentPath + "/Settings/");
            //// Devices Initialization
            if (runMode == RunMode.offline) _emulateDevices = true;
            for (int i = 0; i < _listDevicesTypes.Count; i++)
            {

                var newDevice = GetInstance("DevicesLibrary." + _listDevicesTypes[i], currentPath + "/Settings/" + _listDevicesSettings[i]);
                if (newDevice != null)
                {
                    RegisterDevice((IDevice)newDevice, ((IDevice)newDevice).DeviceName);
                }
                else
                {
                    string Message = "Could not load the device of type( DevicesLibrary." + _listDevicesTypes[i] + ") + with the following settings file : " + currentPath + "/Settings/" + _listDevicesSettings[i];
                    _status = DevicesManagerStatusTypes.Error;
                    _errorInfo = new DevicesManagerErrorInfo(DevicesManagerErrorLevels.Warning, Message);
                    OnRegisterDeviceError(new RegisterDeviceErrorEventArgs("DevicesLibrary." + _listDevicesTypes[i], currentPath + "/Settings/" + _listDevicesSettings[i]));
                }
            }
            // Start devices
            await DeviceHub.ConnectAllDevices();
        }

        protected virtual void OnRegisterDeviceError(RegisterDeviceErrorEventArgs e)
        {
            // Safely raise the capture event for all suscribers
            RegisterDeviceErrorEvent?.Invoke(this, e);
        }
        private bool RegisterDevice(IDevice device, string deviceAlias)
        {
            // Match device and alias in the local registry
            if (_deviceRegistry.ContainsKey(deviceAlias))
            {
                return false;
            }
            else
            {
                // Add it to local Registry
                _deviceRegistry.Add(deviceAlias, device);
                // Add it to the hub 
                _deviceHub.AddDevice(device);
                // Subscribe it to the bus
                _messageProcessor.SubscribeToBus(device, deviceAlias);
                // Connect it to the bus through this Manager based on its characteristics  
                DeviceTypes deviceType = device.DeviceType;
                switch (deviceType)
                {
                    case DeviceTypes.ImagingDevice:

                        ImagingDevice imagingDevice = (ImagingDevice)device;
                        // -- Plug Events --
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceDisconnected, handler => imagingDevice.DeviceDisconnectedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceConnected, handler => imagingDevice.DeviceConnectedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.ImageCaptured, handler => imagingDevice.ImageCapturedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.CameraCalibrationUpdated, handler => imagingDevice.CalibrationUpdatedEvent += handler);
                        // -- Plug Methods --
                        // Device
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ConnectDevice, imagingDevice.Connect);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ConnectDeviceAsync, imagingDevice.ConnectAsync);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.DisconnectDevice, imagingDevice.Disconnect);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ConnectDeviceAsync, imagingDevice.DisconnectAsync);
                        //Camera
                        _messageProcessor.PlugCommandInMethod(device, CommandType.CaptureImage, imagingDevice.CaptureImage);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.CaptureImageAsync, imagingDevice.CaptureImageAsync);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.StartLiveCapture, imagingDevice.StartCaptureImageLive);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.StopLiveCapture, imagingDevice.StopCaptureImageLive);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ResetDevice, imagingDevice.ResetDevice);
                        //Error
                        _messageProcessor.PlugEventInError(ErrorLevel.Error, StandardErrors.DeviceError, handler => imagingDevice.ErrorDeviceEvent += handler);

                        break;
                    case DeviceTypes.ScanningDevice:

                        break;
                    case DeviceTypes.RobotDeviceFullControl:
                        RobotDeviceFullControl robotDevice = (RobotDeviceFullControl)device;
                        // -- Plug Events --
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RobotStatusUpdated, handler => robotDevice.RobotStatusUpdatedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RobotPositionUpdated, handler => robotDevice.RobotPositionUpdatedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RobotMotionStarted, handler => robotDevice.RobotMotionStartedEvent += handler);
                        _messageProcessor.PlugEventInError(ErrorLevel.Error, StandardErrors.DeviceCommsError, handler => robotDevice.RobotCommunicationErrorEvent += handler);
                        // -- Plug Methods --
                        _messageProcessor.PlugCommandInMethod(device, CommandType.GetRobotStatus, robotDevice.GetRobotStatus);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.GetRobotPose, robotDevice.GetRobotPose);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.MoveRobotToPosition, robotDevice.MoveRobotToPosition);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.RunRobotRoutine, robotDevice.RunRoutine);
                        break;
                    case DeviceTypes.RobotDeviceLimitedControl:
                        RobotDeviceLimitedControl robotDeviceLimitedControl = (RobotDeviceLimitedControl)device;
                        // -- Plug Events --
                        //Device
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceSettingsReloaded, handler => robotDeviceLimitedControl.SettingsReloadedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceConnected, handler => robotDeviceLimitedControl.DeviceConnectedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DeviceDisconnected, handler => robotDeviceLimitedControl.DeviceDisconnectedEvent += handler);
                        //Robot
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RobotSetPoseComplete, handler => robotDeviceLimitedControl.SetRobotPoseCompletedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RobotPoseConversionComplete, handler => robotDeviceLimitedControl.ConvertPoseToRobotFormatCompletedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RobotPositionUpdated, handler => robotDeviceLimitedControl.RobotPositionUpdatedEvent += handler);
                        _messageProcessor.PlugEventInAnnouncement(AnnouncementType.RobotStatusUpdated, handler => robotDeviceLimitedControl.RobotStatusUpdatedEvent += handler);
                        _messageProcessor.PlugEventInCommand(CommandType.MoveRobotToPosition, handler => robotDeviceLimitedControl.MoveRobotToPositionCommand += handler, SendMethod.Direct, nameof(ExternalController));
                        _messageProcessor.PlugEventInCommand(CommandType.UpdateRobotPose, handler => robotDeviceLimitedControl.RequestUpdateRobotPoseCommand += handler, SendMethod.Direct, nameof(ExternalController));
                        //Error
                        _messageProcessor.PlugEventInError(ErrorLevel.Error, StandardErrors.DeviceError, handler => robotDeviceLimitedControl.ErrorDeviceEvent += handler);
                        // -- Plug Methods --
                        //Robot
                        _messageProcessor.PlugCommandInMethod(device, CommandType.SetRobotRoutine, robotDeviceLimitedControl.SetRobotRoutine);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ConvertPoseToRobotFormat, robotDeviceLimitedControl.ConvertPoseToRobotFormat);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.SetRobotPose, robotDeviceLimitedControl.SetRobotPose);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.GetRobotStatus, robotDeviceLimitedControl.GetRobotStatus);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.GetRobotPose, robotDeviceLimitedControl.GetRobotPose);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.UpdateRobotPose, robotDeviceLimitedControl.UpdateRobotPose);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.MoveRobotToPosition, robotDeviceLimitedControl.MoveRobotToPosition);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.RunRobotRoutine, robotDeviceLimitedControl.RunRoutine);
                        _messageProcessor.PlugAnnouncementInMethod(device, AnnouncementType.RobotMotionComplete, robotDeviceLimitedControl.MoveRobotToPositionCompleted);
                        //Device
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ConnectDevice, robotDeviceLimitedControl.Connect);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ConnectDeviceAsync, robotDeviceLimitedControl.ConnectAsync);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.DisconnectDevice, robotDeviceLimitedControl.Disconnect);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ConnectDeviceAsync, robotDeviceLimitedControl.DisconnectAsync);
                        _messageProcessor.PlugCommandInMethod(device, CommandType.ReloadSettings, robotDeviceLimitedControl.ReloadSettings);
                        break;
                    case DeviceTypes.IODevice:
                        break;
                }
                return true;
            }

        }

        public object? GetInstance(string strFullyQualifiedName, string settingsPath)
        {
            Type? type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch
                {
                    return null;
                }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                {
                    try
                    {
                        return Activator.CreateInstance(type, args: new object[] { settingsPath });
                    }
                    catch
                    {
                        try
                        {
                            return Activator.CreateInstance(type, args: new object[] { settingsPath, _emulateDevices });
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }
        protected void ReadSettings(string filePath)
        {
            _devicesManagerSettings = filePath;
            _listDevicesTypes = new List<string>();
            _listDevicesSettings = new List<string>();
            // load file 
            ReadWriteXML settingsFile = new ReadWriteXML(filePath + "DeviceManagerSettings.xml");
            bool deviceDefinitionExist = settingsFile.Exist("DeviceDefinition");

            if (deviceDefinitionExist)
            {
                XElement[] listTypes = settingsFile.ReadXMLNodeArrayElements(new List<string> { "DeviceDefinition", "DeviceType" });
                XElement[] listSettings = settingsFile.ReadXMLNodeArrayElements(new List<string> { "DeviceDefinition", "DeviceSettingsRelativePath" });

                for (int i = 0; i < listTypes.Length; i++)
                {

                    string deviceType = listTypes[i].FirstNode.ToString();
                    string deviceSettingsRelativePath = listSettings[i].FirstNode.ToString();
                    _listDevicesTypes.Add(deviceType);
                    _listDevicesSettings.Add(deviceSettingsRelativePath);
                }
            }
        }

        public void GetManagerStatus(out DevicesManagerStatusTypes devicesManagerStatus, out DevicesManagerErrorInfo? devicesManagerErrorInfo, out List<DevicesManagerErrorInfo> devicesErrorsInfo)
        {
            devicesManagerStatus = _status;
            devicesManagerErrorInfo = _errorInfo;

            devicesErrorsInfo = [];

            foreach (KeyValuePair<string, IDevice> device in _deviceRegistry)
            {
                switch (device.Value.DeviceStatus)
                {
                    case DeviceStatus.Unknown:
                        break;
                    case DeviceStatus.Discoverable:
                        break;
                    case DeviceStatus.Initialized:
                        break;
                    case DeviceStatus.Started:
                        break;
                    case DeviceStatus.Warning:
                        break;
                    case DeviceStatus.Emulated:
                        break;
                    case DeviceStatus.Error:
                        devicesErrorsInfo.Add(new DevicesManagerErrorInfo(DevicesManagerErrorLevels.Fatal, "The device (" + device.Key + ") is in error state"));
                        break;

                }

            }
        }


        private async Task SwitchDevicesRunMode(object sender, EventArgs e)
        {
            RunModeUpdateEventArgs? args = e as RunModeUpdateEventArgs;
            if (args == null) { return; }
            RunMode runMode = args.NewRunMode;
            switch (runMode)
            {
                case RunMode.online:
                    {
                        await _deviceHub.GoOnline();
                        break;
                    }
                case RunMode.offline:
                    {
                        await _deviceHub.GoOffline();
                        break;
                    }
            }
            OnDevicesRunModeUpdated(this, args);
        }

        private void OnDevicesRunModeUpdated(object sender, RunModeUpdateEventArgs eventArgs)
        {
            DevicesRunModeUpdatedEvent?.Invoke(sender, eventArgs);
        }
    }

    public class RegisterDeviceErrorEventArgs : EventArgs
    {
        private string _deviceSettingsRelativePath;
        private string _deviceType;
        public string DeviceSettingsRelativePath { get { return _deviceSettingsRelativePath; } }
        public string DeviceType { get { return _deviceType; } }

        public RegisterDeviceErrorEventArgs(string deviceSettingsRelativePath, string deviceType)
        {
            _deviceSettingsRelativePath = deviceSettingsRelativePath;
            _deviceType = deviceType;
        }
    }
}
