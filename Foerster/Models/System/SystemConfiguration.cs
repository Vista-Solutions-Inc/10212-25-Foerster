using HalconDotNet;
using DevicesLibrary;
using MessageBusLibrary.Interfaces;
using MessageBusLibrary;
using Jobs.BaseClasses;

namespace Foerster.Models.System
{
    public enum RunMode
    {
        online,
        semiauto,
        offline,
    }
    
    public class SystemConfiguration : IGenericMessageBusConnector
    {
        // Events
        public event EventHandler? RunModeUpdateRequestEvent;
        public event EventHandler? RunModeUpdateCompletedEvent;
        public event EventHandler? CameraConfigurationUpdatedEvent;
        public event EventHandler? CaptureOnlyModeChangedEvent;
        public event EventHandler? DebugModeUpdatedEvent;
        // Fields
        private static SystemConfiguration _instance = null;
        private static readonly object _lock = new object();
        private RunMode _runMode = RunMode.offline; 
        private bool _debugEnable = false;
        private bool _captureOnlyEnable = false;
        private MessageProcessor _messageProcessor;
        private string _authenticationPath;
        private string _jobsFolderPath;
        private string _cameraCalibrationApp;
        public const string SystemLogger = "VisionSystemLogger";
        public const string ExternalControllerLogger = "ExternalControllerLogger";

        
        // Properties 
        public RunMode RunMode { get { return _runMode; } set { SwitchRunMode(value); } }
        public bool DebugEnable { get { return _debugEnable; } set { _debugEnable = value; OnDebugModeUpdated(new EventArgs ()); } }
        public bool CaptureOnlyEnable
        {
            get { return _captureOnlyEnable; }
            set
            {
                _captureOnlyEnable = value;
                OnCaptureModeChanged(new CaptureModeUpdateEventArgs(value));
            }
        }
        public string JobsFolderPath { get { return _jobsFolderPath; } set { _jobsFolderPath = value; } }
        public string CameraCalibrationApp { get { return _cameraCalibrationApp; } set { _cameraCalibrationApp = value; } }

        

        // --Bus interaction--
        public MessageProcessor MessageProcessor { get { return _messageProcessor; } }
        public string ConnectorName { get { return "System Configuration"; } }
        public string AuthenticationPath { get { return _authenticationPath; } set { _authenticationPath = value; } }


        #region Constructors
        protected SystemConfiguration()
        {
            ConnectModuleToBus();
        }

        public static SystemConfiguration Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SystemConfiguration();
                    }
                    return _instance;
                }
            }
        }
        #endregion

        // Methods
        public void ConnectModuleToBus()
        {
            _messageProcessor = new MessageProcessor(MessageBus.Instance, this, this.ConnectorName);
            // plug events
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.CaptureOnlyModeChanged, handler => CaptureOnlyModeChangedEvent += handler);
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.DevicesOperationModeChangeRequested, handler => RunModeUpdateRequestEvent += handler);
            // plug methods
            _messageProcessor.PlugAnnouncementInMethod(this, AnnouncementType.DevicesModeUpdated, DevicesModeSwitchCompleted);
        }
        public void SwitchRunMode(RunMode newMode)
        {
            if (newMode != _runMode)
            {
                RunMode previousMode = _runMode;
                OnRunModeUpdateRequest(new RunModeUpdateEventArgs(previousMode, newMode));
            }
        }

        public void DevicesModeSwitchCompleted(object sender, EventArgs e)
        {
            RunModeUpdateEventArgs? args = e as RunModeUpdateEventArgs;
            if (args != null)
            {
                _runMode = args.NewRunMode;
                OnRunModeUpdateCompleted(args);
            }
        }

        private void OnRunModeUpdateRequest(RunModeUpdateEventArgs e)
        {
            RunModeUpdateRequestEvent?.Invoke(this, e);
        }


        private void OnDebugModeUpdated(EventArgs e)
        {
            DebugModeUpdatedEvent?.Invoke(this, e);
        }
        private void OnRunModeUpdateCompleted(RunModeUpdateEventArgs e)
        {
            RunModeUpdateCompletedEvent?.Invoke(this, e);
        }
        private void OnCaptureModeChanged(CaptureModeUpdateEventArgs e)
        {
            CaptureOnlyModeChangedEvent?.Invoke(this, e);
        }

    }
    public class RunModeUpdateEventArgs : EventArgs
    {
        // Fields
        private RunMode _newRunMode;
        private RunMode _prevRunMode;
        // Properties
        public RunMode NewRunMode { get { return _newRunMode; } }
        public RunMode PrevRunMode { get { return _prevRunMode; } }
        // Constructors
        public RunModeUpdateEventArgs(RunMode prevRunMode, RunMode newRunMode)
        {
            _newRunMode = newRunMode;
            _prevRunMode = prevRunMode;
        }
    }

}
