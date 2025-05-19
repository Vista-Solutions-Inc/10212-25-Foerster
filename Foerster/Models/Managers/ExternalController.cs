using CommsLibrary.NamedPipes;
using DevicesLibrary;
using Foerster.Models.System;
using HalconDotNet;
using Jobs.BaseClasses;
using LJX8;
using MessageBusLibrary;
using MessageBusLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Wpf.Ui.Controls;
using VistaHelpers.Log4Net;
namespace Foerster.Models.Managers {

    public class ExternalController : IGenericMessageBusConnector {

        private int _currentStepID = -1;

        #region Set static
        private static ExternalController _instance = null;
        private static readonly object _lock = new object();
        public static ExternalController Instance {
            get {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = new ExternalController();
                    }
                    return _instance;
                }
            }
        }
        #endregion
        #region Message bus
        public event EventHandler CommunicationErrorEvent;

        private static readonly string _connectorName = nameof(ExternalController);
        public string ConnectorName { get { return _connectorName; } }
        MessageProcessor _messageProcessor;
        public MessageProcessor MessageProcessor { get { return _messageProcessor; } }
        public void ConnectModuleToBus() {
            _messageProcessor = new MessageProcessor(MessageBus.Instance, this, _connectorName);
            _messageProcessor.PlugCommandInMethod(this, CommandType.ExternalControllerStartServer, this.StartServer);
            _messageProcessor.PlugCommandInMethod(this, CommandType.ExternalControllerStopServer, this.StopServer);
            _messageProcessor.PlugEventInAnnouncement(AnnouncementType.CommunicationError, handler => CommunicationErrorEvent += handler);
        }

        #endregion
        #region Pipe Duplex
        private PipeDuplex _pipeDuplex;
        public PipeDuplex PipeDuplex { get { return _pipeDuplex; } }
        public async void StartServer(object? sender, EventArgs eventArgs) {
            await _pipeDuplex.GoLive();
        }
        public void StopServer(object? sender, EventArgs eventArgs) {
            _pipeDuplex.StopLive();
        }
        #endregion
        protected ExternalController() {
            // Connect to Bus
            ConnectModuleToBus();

            _pipeDuplex = new PipeDuplex("VistaCommsPipe", false, 50);
            _pipeDuplex.LogUpdateEvent += OnLogPipeAppend;
            _pipeDuplex.PipeStatusUpdateEvent += OnStatusUpdated;
            _pipeDuplex.NewPipeCommand += OnNewPipeCommand;
        }


        private async void OnNewPipeCommand(object? sender, EventArgs e) {
            //GenericVisionPipeCommandEventArgs? eventArgs = e as GenericVisionPipeCommandEventArgs;
            //if (eventArgs == null) return;
            //long? ExecuteJobPipeCommandsIsSuccessful = 0;
            //string? DeviceName;
            //List<string>? pipeContent = eventArgs.Content as List<string>;
            //BooleanResponsePipeCommand busyPipeCommand;
            //switch (eventArgs.Command) {
            //    #region Job Management 
            //    case PipeCommands.JobManagementTriggerJob:
            //        //ExecuteJobPipeCommandsIsSuccessful = await ExecuteJob();
            //        break;
            //    case PipeCommands.JobManagementTriggerStepNext:
            //        busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, true);
            //        await _pipeDuplex.SendCommand(busyPipeCommand);
            //        AckResponsePipeCommand stepTriggerAckPipeCommand = new AckResponsePipeCommand(AckResponse.StepTriggerAck, true);
            //        await _pipeDuplex.SendCommand(stepTriggerAckPipeCommand);

            //        if (_currentStepID == -1) {
            //            ExecuteJobPipeCommandsIsSuccessful = 12;
            //            break;
            //        }
            //        ExecuteJobPipeCommandsIsSuccessful = await ExecuteStep(_currentStepID);

            //        if (ExecuteJobPipeCommandsIsSuccessful == null) {
            //            BooleanResponsePipeCommand stepCompletedPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.StepTriggerCompleted, true);
            //            await _pipeDuplex.SendCommand(stepCompletedPipeCommand);
            //            // Probably will be get from the part, 128 boolean values each one is a weld. Each byte is a stream and a camera. The first 8 values correspond to INSP_RESULTS16, then 15 and so on. The first bit from the 8 values has the highest significance: 01111111 = 127.
            //            StepResultsPipeCommand stepResultsPipeCommand = new StepResultsPipeCommand(new List<bool>() { false, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false});
            //            await _pipeDuplex.SendCommand(stepResultsPipeCommand);
            //            // 1024 bits, one byte for each weld
            //            //ClassResultsPipeCommand classResultsDetailsPipeCommand = new ClassResultsPipeCommand(new List<bool>() { true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false });
            //            //await _pipeDuplex.SendCommand(classResultsDetailsPipeCommand);
            //            BooleanResponsePipeCommand resultsValidPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.InspResultsValid, true);
            //            await _pipeDuplex.SendCommand(resultsValidPipeCommand);
            //            busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, false);
            //            await _pipeDuplex.SendCommand(busyPipeCommand);
            //            return;
            //        }
            //        break;

            //    case PipeCommands.JobManagementTriggerStepX:
            //        break;
            //    case PipeCommands.JobManagementTriggerTaskNext:
            //        break;
            //    case PipeCommands.JobManagementTriggerTaskX:
            //        break;
            //    case PipeCommands.JobManagementReset:
            //        busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, true);
            //        await _pipeDuplex.SendCommand(busyPipeCommand);
            //        //AckResponsePipeCommand resetAckPipeCommand = new AckResponsePipeCommand(AckResponse.ResetAck, true);
            //        //await _pipeDuplex.SendCommand(resetAckPipeCommand);
            //        ExecuteJobPipeCommandsIsSuccessful = await ResetJob();
            //        //ExecuteJobPipeCommandsIsSuccessful = null;
            //        if (ExecuteJobPipeCommandsIsSuccessful == null) {
            //            //BooleanResponsePipeCommand resetCompletedPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.ResetCompleted, true);
            //            //await _pipeDuplex.SendCommand(resetCompletedPipeCommand);
            //            AckResponsePipeCommand resetAckPipeCommand = new AckResponsePipeCommand(AckResponse.ResetAck, true);
            //            await _pipeDuplex.SendCommand(resetAckPipeCommand);
            //            busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, false);
            //            await _pipeDuplex.SendCommand(busyPipeCommand);
            //            return;
            //        }
            //        break;
            //    case PipeCommands.JobManagementSetJobTrigger:
            //        busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, true);
            //        await _pipeDuplex.SendCommand(busyPipeCommand);
            //        AckResponsePipeCommand jobChangingAckPipeCommand = new AckResponsePipeCommand(AckResponse.JobChangingAck, true);
            //        await _pipeDuplex.SendCommand(jobChangingAckPipeCommand);
            //        BooleanResponsePipeCommand jobChangingPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.JobChangeExecuting, true);
            //        await _pipeDuplex.SendCommand(jobChangingPipeCommand);
            //        if (pipeContent == null || pipeContent.Count != 1) {
            //            ExecuteJobPipeCommandsIsSuccessful = 16;
            //            break;
            //        }
            //        ExecuteJobPipeCommandsIsSuccessful = await SetCurrentJob(pipeContent[0]);
            //        if (ExecuteJobPipeCommandsIsSuccessful == null) {
            //            BooleanResponsePipeCommand jobChangingFinishedPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.JobChangeExecuting, false);
            //            await _pipeDuplex.SendCommand(jobChangingFinishedPipeCommand);
            //            LongResponsePipeCommand jobEchoPipeCommand = new LongResponsePipeCommand(LongResponse.JobSelectorEcho, long.Parse(pipeContent[0]));
            //            await _pipeDuplex.SendCommand(jobEchoPipeCommand);
            //            busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, false);
            //            await _pipeDuplex.SendCommand(busyPipeCommand);
            //            return;
            //        }
            //        break;
            //    case PipeCommands.JobManagementSetStepTrigger:
            //        busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, true);
            //        await _pipeDuplex.SendCommand(busyPipeCommand);
            //        if (pipeContent == null || pipeContent.Count != 1 || pipeContent[0] == "") {
            //            ExecuteJobPipeCommandsIsSuccessful = 11;
            //            break;
            //        }
            //        bool success = int.TryParse(pipeContent[0], out int stepIndex);
            //        if (success) {
            //            _currentStepID = stepIndex-1;
            //            LongResponsePipeCommand stepEchoPipeCommand = new LongResponsePipeCommand(LongResponse.StepSelectorEcho, long.Parse(pipeContent[0]));
            //            await _pipeDuplex.SendCommand(stepEchoPipeCommand);
            //            busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, false);
            //            await _pipeDuplex.SendCommand(busyPipeCommand);
            //            break;
            //        }
            //        else {
            //            ExecuteJobPipeCommandsIsSuccessful = 11;
            //            break;
            //        }
            //    case PipeCommands.JobManagementSetPartNumberEcho:
            //        busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, true);
            //        await _pipeDuplex.SendCommand(busyPipeCommand);
            //        if (pipeContent == null || pipeContent.Count != 1 || pipeContent[0] == "")
            //        {
            //            ExecuteJobPipeCommandsIsSuccessful = 11;
            //            break;
            //        }
            //        // Set the part number
            //        if (JobManager.Instance.CurrentJob != null)
            //        {
            //            JobManager.Instance.CurrentJob.Part.SerialNumber = pipeContent[0];
            //        }
            //        else
            //        {
            //            ExecuteJobPipeCommandsIsSuccessful = 11;
            //            break;
            //        }
            //        StringResponsePipeCommand partNumberEchoPipeCommand = new StringResponsePipeCommand(StringResponse.PartNumberEcho, pipeContent[0]);
            //        await _pipeDuplex.SendCommand(partNumberEchoPipeCommand);
            //        busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, false);
            //        await _pipeDuplex.SendCommand(busyPipeCommand);
            //        break;
            //    #endregion
            //    #region System Management 
            //    case PipeCommands.SystemClearErrors:
            //        ExecuteJobPipeCommandsIsSuccessful = await SystemClearErrors();
            //        break;
            //    case PipeCommands.SystemReset:
            //        ExecuteJobPipeCommandsIsSuccessful = await SystemReset();
            //        break;
            //    case PipeCommands.SystemGetStatus:
            //        //ExecuteJobPipeCommandsIsSuccessful = await SystemGetStatus();
            //        break;
            //    #endregion
            //    #region Devices
            //    case PipeCommands.DevicesResetAll:
            //        ExecuteJobPipeCommandsIsSuccessful = await DevicesConnect();
            //        break;
            //    case PipeCommands.DeviceReset:
            //        if (pipeContent == null || pipeContent.Count < 1) {
            //            ExecuteJobPipeCommandsIsSuccessful = 31;
            //            break;
            //        }
            //        ExecuteJobPipeCommandsIsSuccessful = await DeviceConnect(pipeContent);
            //        break;
            //    case PipeCommands.DeviceTrigger:
            //        if (pipeContent == null || pipeContent.Count < 1)
            //        {
            //            ExecuteJobPipeCommandsIsSuccessful = 32;
            //            break;
            //        }
            //        ExecuteJobPipeCommandsIsSuccessful = await DeviceTrigger(pipeContent);
            //        break;
            //    case PipeCommands.DeviceSetParam:
            //        ExecuteJobPipeCommandsIsSuccessful = 0;
            //        OnCommunicationError("The command could not be executed because it is not supported in the vision system or has not been implemented.");
            //        break;
            //    case PipeCommands.DeviceSetRobotPose:
            //        if (pipeContent == null || pipeContent.Count != 7) {
            //            ExecuteJobPipeCommandsIsSuccessful = 31;
            //            break;
            //        }
            //        ExecuteJobPipeCommandsIsSuccessful = await DeviceSetRobotPose(pipeContent);
            //        break;
            //    #endregion
            //    case PipeCommands.PipeCommandsError:
            //        ExecuteJobPipeCommandsIsSuccessful = 0;
            //        string? eventArgsContent = eventArgs.Content as string;
            //        OnCommunicationError((eventArgsContent != null) ? eventArgsContent : "The command could not be executed because it was not recognized. However, the received data cannot be displayed due to it being of string type or null.");

            //        break;
            //    default:
            //        ExecuteJobPipeCommandsIsSuccessful = 0;
            //        OnCommunicationError("The command cannot be executed because it is not valid for the vision system.");
            //        break;
            //}

            //if (ExecuteJobPipeCommandsIsSuccessful != 0) {
            //    LongResponsePipeCommand errorCodePipeCommand = new LongResponsePipeCommand(LongResponse.ErrorCode, (long)ExecuteJobPipeCommandsIsSuccessful);
            //    await _pipeDuplex.SendCommand(errorCodePipeCommand);
            //    BooleanResponsePipeCommand enableErrorPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Error, true);
            //    await _pipeDuplex.SendCommand(enableErrorPipeCommand);
            //    busyPipeCommand = new BooleanResponsePipeCommand(BooleanResponse.Busy, false);
            //    await _pipeDuplex.SendCommand(busyPipeCommand);
            //}
        }

        private void OnStatusUpdated(object? sender, EventArgs e) {
            //throw new NotImplementedException();
        }
        private void OnLogPipeAppend(object? sender, EventArgs e) {
            //throw new NotImplementedException();
        }

        /*****************************   Execute Pipe Command   **********************************************************/

        #region Job Management Coomands

        /// <summary>
        /// 
        /// </summary>
        /// <returns>10 if error</returns>
        private async Task<long?> ExecuteJob() {
            BusInstruction instruction = new(this, "Run Job", CommandType.ExecuteJob, nameof(JobManager), AnnouncementType.JobExecutionCompleted, StandardErrors.JobExecutionError, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new EventArgs()));
            return (instruction.IsSuccessful) ? null : 10;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>11 if error</returns>

        private async Task<long?> ExecuteNextStep() {
            BusInstruction instruction = new(this, "Run Current Step", CommandType.ExecuteStep, nameof(JobManager), AnnouncementType.StepExecutionCompleted, StandardErrors.StepExecutionError, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new JobExecutionEventArgs()));
            return (instruction.IsSuccessful) ? null : 11;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepIndex"></param>
        /// <returns>12 if error</returns>
        private async Task<long?> ExecuteStep(int stepIndex) {
            BusInstruction instruction = new(this, "Run Step", CommandType.ExecuteStep, nameof(JobManager), AnnouncementType.StepExecutionCompleted, StandardErrors.StepExecutionError, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new JobExecutionEventArgs(stepIndex)));
            return (instruction.IsSuccessful) ? null : 12;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>13 if error</returns>
        private async Task<long?> ExecuteNextTask() {
            return 13;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIndex"></param>
        /// <returns>14 if error</returns>
        private async Task<long?> ExecuteTask(int taskIndex) {
            return 14;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>15 if error</returns>
        private async Task<long?> ResetJob() {
            BusInstruction instruction = new(this, "Reset Job", CommandType.RestartJob, nameof(JobManager), AnnouncementType.RestartJobCompleted, StandardErrors.RestartJobError, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new EventArgs()));
            return (instruction.IsSuccessful) ? null : 15;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns>16 if error</returns>
        private async Task<long?> SetCurrentJob(string? jobId) {
            BusInstruction instruction = new(this, "Set Current Job :" + jobId, CommandType.SetCurrentJob, nameof(JobManager), AnnouncementType.SetCurrentJobCompleted, StandardErrors.SetCurrentJobError, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new ChangeJobEventArgs(jobID: jobId)));
            return (instruction.IsSuccessful) ? null : 16;
        }

        #endregion

        #region System Coomands
        private async Task<long?> SystemClearErrors() {
            BusInstruction instruction = new(this, "Reset System", CommandType.ClearSystemErrors, nameof(SystemManager), AnnouncementType.ClearSystemErrorsCompleted, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new EventArgs()));
            return (instruction.IsSuccessful) ? null : 20;
        }
        private async Task<long?> SystemReset() {

            BusInstruction instruction = new(this, "Reset System", CommandType.ResetSystem, nameof(SystemManager), AnnouncementType.ResetSystemCompleted, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new EventArgs()));
            return (instruction.IsSuccessful) ? null : 21;
        }

        #endregion

        #region Devices Coomands
        private async Task<long?> DevicesConnect() {
            BusInstruction instruction = new(this, "Reconnect All Devices", CommandType.ReconnectAllDevices, nameof(DeviceHub), AnnouncementType.ReconnectAllDevicesCompleted, waitConfirmation: true);
            await Task.Run(async () => await instruction.RunCommandAsync(new EventArgs()));
            return (instruction.IsSuccessful) ? null : 30;
        }
        private async Task<long?> DeviceConnect(List<string> BoolList)
        {
            List<string> DeviceList = TranslateBoolListToDeviceList(BoolList);
            foreach (string DeviceName in DeviceList)
            {
                if (string.IsNullOrWhiteSpace(DeviceName))
                {
                    return 31;
                }

                BusInstruction instruction = new(this, "Get Device Info", CommandType.GetDeviceInfo, nameof(DeviceHub), AnnouncementType.RequestedDeviceInfo, waitConfirmation: true);
                await Task.Run(async () => await instruction.RunCommandAsync(new RequestDeviceInfoEventArgs(DeviceName)));
                if (instruction.IsSuccessful)
                {
                    IDevice? device = ((DeviceManagerEventArgs)(instruction.ResponseContent)).Device as IDevice;
                    if (device == null)
                    {
                        return 32;
                    }

                    if (device.DeviceType != DeviceTypes.ImagingDevice)
                    {
                        return 33;
                    }
                    try
                    {
                        instruction = new(this, "Capture Image External Controller", CommandType.ConnectDevice, device.DeviceName, AnnouncementType.DeviceConnected, waitConfirmation: true);
                        await Task.Run(async () => await instruction.RunCommandAsync(new EventArgs()));
                        if (!instruction.IsSuccessful) return 34;
                    }
                    catch
                    {
                        return 34;

                    }


                }
                else
                {
                    return 32;

                }
            }
            return null;
        }
        private async Task<long?> DeviceTrigger(List<string>BoolList) {
            List<string> DeviceList = TranslateBoolListToDeviceList(BoolList);
            foreach (string DeviceName in DeviceList)
            {
                if (string.IsNullOrWhiteSpace(DeviceName)) {
                    return 31;
                }
                BusInstruction instruction = new(this, "Get Device Info", CommandType.GetDeviceInfo, nameof(DeviceHub), AnnouncementType.RequestedDeviceInfo, waitConfirmation: true);
                await Task.Run(async () => await instruction.RunCommandAsync(new RequestDeviceInfoEventArgs(DeviceName)));
                if (instruction.IsSuccessful) {
                    IDevice? device = ((DeviceManagerEventArgs)(instruction.ResponseContent)).Device as IDevice;
                    if (device == null) {
                        return 32;
                    }

                    if (device.DeviceType != DeviceTypes.ImagingDevice) {
                        return 33;
                    }
                    try {
                        instruction = new(this, "Capture Image External Controller", CommandType.CaptureImage, device.DeviceName, AnnouncementType.ImageCaptured, StandardErrors.DeviceError, waitConfirmation: true);
                        await Task.Run(async () => await instruction.RunCommandAsync(new EventArgs()));
                        if (!instruction.IsSuccessful) return 34;

                    }
                    catch {
                        return 34;
                    }

                }
                else {
                    return 32;

                }
            }
            return null;

        }
        private async Task<long?> DeviceSetParam() {
            return 36;
        }
        private async Task<long?> DeviceSetRobotPose(List<string> data) {
            double x =0 ;
            double y = 0;
            double z = 0;
            double rx = 0;
            double ry = 0;
            double rz = 0;
            int type = 0;
            bool parseSuccess = double.TryParse(data[0], out  x);
            parseSuccess = parseSuccess && double.TryParse(data[1], out y );
            parseSuccess = parseSuccess && double.TryParse(data[2], out  z);
            parseSuccess = parseSuccess && double.TryParse(data[3], out  rx);
            parseSuccess = parseSuccess && double.TryParse(data[4], out  ry);
            parseSuccess = parseSuccess && double.TryParse(data[5], out  rz);
            parseSuccess = parseSuccess && int.TryParse(data[6], out type);

            if (!parseSuccess) {
                return 37;
            }

            else {
                return 38;
            }
        }
        #endregion
        private void OnCommunicationError(string message) {
            CommunicationErrorEvent?.Invoke(this, new CommunicationErrorEventArgs(message));
        }

        private List<string> TranslateBoolListToDeviceList (List<string> data)
        {
            List<string> deviceList = [];
            for (int i = 0; i<data.Count; i++)
            {
                bool parseSuccess = bool.TryParse(data[i], out bool enableDevice);
                if (!parseSuccess)
                {
                    parseSuccess = int.TryParse(data[i], out int intEnableDevice);
                    if (!parseSuccess) continue;
                    enableDevice = !(intEnableDevice == 0);
                }
                if (!enableDevice) continue;
                switch (i)
                {
                    case 0:
                        deviceList.Add("Photoneo");
                        break;
                }
            }
            return deviceList;
        }


    }

    public class CommunicationErrorEventArgs : EventArgs {
        string _message;
        public string Message { get { return _message; } }
        public CommunicationErrorEventArgs(string message) {
            _message = message;
        }
    }

}
