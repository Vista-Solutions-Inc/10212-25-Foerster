using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevicesLibrary;
using DevicesLibrary.Devices.Imaging;
using HalconDotNet;
using MessageBusLibrary;
using MessageBusLibrary.Interfaces;

namespace Foerster.Models
{
    public class JobTaskInstructions : IGenericMessageBusConnector
    {
        #region Fields
        // For singleton instance
        private static JobTaskInstructions _instance = null;
        private static readonly object _lock = new object();
        // For the Message Bus
        private string _connectorName = nameof(JobTaskInstructions);
        private MessageProcessor _messageProcessor;
        #endregion

        #region Properties
        // For singleton instance
        public static JobTaskInstructions Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new JobTaskInstructions();
                    }
                    return _instance;
                }
            }
        }        // Message Bus
        public string ConnectorName => _connectorName;
        public MessageProcessor MessageProcessor => _messageProcessor;
        #endregion

        #region Constructors
        protected JobTaskInstructions()
        {
            ConnectModuleToBus();
        }
        #endregion

        #region Internal Static Methods
        internal static async Task<HImage?> CaptureImage(string caller, string imagingDeviceName)
        {
            HImage? image = null;
            BusInstruction captureInstruction = new BusInstruction(Instance, $"{caller}_Capture_Image", CommandType.CaptureImage, imagingDeviceName, AnnouncementType.ImageCaptured, StandardErrors.DeviceError, true);
            await captureInstruction.RunCommandAsync();
            if (typeof(ImageCaptureEventArgs) == captureInstruction.ResponseContent.GetType())
            {
                ImageCaptureEventArgs captureArgs = (ImageCaptureEventArgs)captureInstruction.ResponseContent;
                if (captureArgs != null)
                {
                    image = captureArgs.Image;
                }
            }
            return image;
        }
        internal static IDevice? GetDevice(string caller, string deviceName)
        {
            IDevice? device = null;
            BusInstruction requestDeviceInstruction = new BusInstruction(Instance, $"{caller}_Get_Device", CommandType.GetDevice, "DeviceHub", AnnouncementType.DeviceReferenceSent, true);
            requestDeviceInstruction.RunCommand(new RequestDeviceInfoEventArgs(deviceName));
            DeviceManagerEventArgs? response = requestDeviceInstruction.ResponseContent as DeviceManagerEventArgs;
            if (response != null)
            {
                device = response.Device;
            }
            return device;
        }
        #endregion

        #region Public Instance Methods
        public void ConnectModuleToBus()
        {
            _messageProcessor = new MessageProcessor(MessageBus.Instance, this, _connectorName);
        }
        #endregion
    }
}