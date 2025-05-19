using HalconDotNet;
using VistaHelpers.FileManagement;
using DevicesLibrary;
using MessageBusLibrary;
using System.Diagnostics;
using MessageBusLibrary.Interfaces;
using Foerster.Models.System;
using Jobs.BaseClasses;
using System.ComponentModel;
using Foerster.Models.JobManagement;
using VistaHelpers.Log4Net;
using VistaControls.UsersManagement.Authenticator;
using VistaControls.UsersManagement;

namespace Foerster.Models.Managers
{
    public class SystemManager : IGenericMessageBusConnector
    {
        // Events

        // Fields
        // --Configuration--
        private SystemConfiguration _systemConfiguration;
        private string _currentPath;
        private UserManager _userManager;

        // -- License periodic validation --
        private int _licenseValidationTimerPeriod = 20000;

        // --Devices--
        private DevicesManager _deviceManager;

        // --Message Bus--
        private MessageProcessor _messageProcessor;
        private Mediator _busMediator;
        private string _connectorName = "System Manager";

        // --Execution--
        private LicenseManager _licenseManager;
        private JobManager _jobManager;
        private ExternalController _externalController;

        // Properties 
        public Dictionary<string, IDevice> SystemDevices { get { return _deviceManager.DeviceRegistry; } }
        public RunMode RunMode {  get { return _systemConfiguration.RunMode; } }
        public JobManager JobManager { get { return _jobManager; } set { _jobManager = value; } }
        //Calibration
        private Process _externalAppProcess = new();

        // -- For Bus Connection --
        public MessageProcessor MessageProcessor { get { return _messageProcessor; } }
        public string ConnectorName { get { return _connectorName; } }

        // Constructors 
        public SystemManager()
        {

            _currentPath = Environment.CurrentDirectory;
            // read startup system settings 
            ReadSettings(_currentPath + "/Settings/SystemSettings.xml");
            // Initialize License Manager
            _licenseManager = LicenseManager.Instance;
            _licenseManager.ValidateLicense();
            _licenseManager.SetPeriodicValidation(_licenseValidationTimerPeriod);

            // Connect the manager to the bus
            ConnectModuleToBus();
            // Initialize System Configuration 
            _systemConfiguration =  SystemConfiguration.Instance;


            // Initialize Components
            InitializeSystemComponents();
            _jobManager = JobManager.Instance;
            _jobManager.JobsFolderPath = _systemConfiguration.JobsFolderPath;
            SetJobsType();

            _userManager = App.GetService<UserManager>();
            _userManager.UserLevelUpdatedEvent += OnUserRoleChanged;



        }

        // Methods 
        // --Initialization--
        public async void InitializeSystemComponents() 
        {
            // initialize Device Manager 
            _deviceManager = new DevicesManager();
            // initialize devices 
            await _deviceManager.InitializeDevices(RunMode, _currentPath);
        }

        public async void SetJobsType() 
        {
            BusInstruction updateJobInstruction = new(this, "Add new Job", CommandType.RegisterNewJobType, "JobManager");
            await updateJobInstruction.RunCommandAsync(new RegisterNewJobTypeEventArgs(typeof(LimitedConfigurationJob)));
            await updateJobInstruction.RunCommandAsync(new RegisterNewJobTypeEventArgs(typeof(FullConfigurationJob)));

            
            //Throw new NotImplementedException();
        }

        private void ReadSettings(string settingsPath)
        {
            // load file 
            ReadWriteXML settingsFile = new ReadWriteXML(settingsPath);
            // get & assign settings 
            SystemConfiguration.Instance.AuthenticationPath = settingsFile.ReadXML("SystemSettings", "AuthenticationPath");
            SystemConfiguration.Instance.JobsFolderPath = settingsFile.ReadXML("SystemSettings", "JobsFolderPath");
            SystemConfiguration.Instance.CameraCalibrationApp = settingsFile.ReadXML("SystemSettings", "CameraCalibrationApp");
        }

        // --Bus management--
        public void ConnectModuleToBus()
        {
            // Initialize MessageBus Mediator 
            _busMediator = Mediator.Instance;
            // initialize own Message Processor and suscribe to bus
            _messageProcessor = new MessageProcessor(MessageBus.Instance, this, _connectorName);
            // plug methods 
            _messageProcessor.PlugErrorInMethod(ErrorLevel.Error, StandardErrors.DeviceError, OnDeviceError);

            // plug events

        }
        public void OnDeviceError(object? sender, EventArgs e)
        {

        }
        private void OnUserRoleChanged(object sender, EventArgs e)
        {
            UserLevelEventArgs args = (UserLevelEventArgs)e;
            Enum.TryParse<UserRole>(args.UserLevel, true, out UserRole userRole);
            VistaLogger.Log(VistaLogger.LogLevel.Info, VistaLogger.VistaSubModules.MainSystem, $"Changing To Role : {userRole.ToString()}");
            _jobManager.CanModifyJobs = userRole > 0;
        }

        //public async Task LaunchCameraCalibrationApp(string deviceName)
        //{
        //    ProcessStartInfo startInfo = new ProcessStartInfo();

        //    // Set Arguments
        //    string settingsPath = _currentPath + "/Settings";

        //    if (string.IsNullOrEmpty(settingsPath)) throw new Exception("The file path for the device settings is invalid.");

        //    // disconnect all devices to allow the camera calibration app to use them 
        //    await _deviceManager.DeviceHub.DisconnectAllDevices();

        //    startInfo.Arguments =
        //        $"SettingsPath \"{settingsPath}\" " +
        //        $"SelectedDevice \"{deviceName}\" " +
        //        $"CalibrationType {CalibrationType.hand_eye_moving_cam.ToString()} " +
        //        $"";
        //    // -- Set UP for Application launch
        //    startInfo.RedirectStandardOutput = true;
        //    startInfo.ErrorDialog = true;
        //    startInfo.FileName = SystemConfiguration.Instance.CameraCalibrationApp;
        //    try
        //    {
        //        //-- Launch Calibration App
        //        _externalAppProcess = Process.Start(startInfo);
        //        // Wait for results
        //        _externalAppProcess.WaitForExit();
        //        // Reload Devices settings
        //        _deviceManager.DeviceHub.ReloadDevicesSettings();
        //        // Reconnect 
        //        _deviceManager.DeviceHub.ConnectAllDevices();
        //        // Start Robot Status monitor
        //        //_robotArm.StartStatusMonitor();
        //    }
        //    catch
        //    {

        //    }
        //    //* Read the output (or the error)
        //}
    }
}
