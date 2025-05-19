using DevicesLibrary;
using HalconDotNet;
using MessageBusLibrary;
using MessageBusLibrary.Interfaces;
using DevicesLibrary.Devices.Imaging;


namespace Foerster.Models.System
{
    public static class SystemInstructions
    {
        // --Bus Instructions--
        public static  async Task<HImage> GetCurrentImage(IGenericMessageBusConnector caller, string cameraName)
        {
            // Capture Image command
            HImage capturedImage = new HImage();
            BusInstruction captureInstruction = new BusInstruction(caller, "CaptureImageDetection", CommandType.CaptureImage, cameraName, AnnouncementType.ImageCaptured, true);
            await captureInstruction.RunCommandAsync();
            ImageCaptureEventArgs captureArgs = (ImageCaptureEventArgs)captureInstruction.ResponseContent;
            if (captureArgs != null)
            {
                capturedImage = captureArgs.Image;
            }
            return capturedImage;
        }

        public static async Task<HTuple> GetRobotPose(IGenericMessageBusConnector caller, string RobotName)
        {
            HTuple robotPose = new HTuple();
            BusInstruction getPoseInstruction = new BusInstruction(caller, "GetRobotPose", CommandType.GetRobotPose, RobotName, AnnouncementType.RobotPositionUpdated, StandardErrors.DeviceCommsError);
            await getPoseInstruction.RunCommandAsync();
            if (getPoseInstruction.IsSuccessful)
            {
                RobotPoseEventArgs rpuEventArgs = (RobotPoseEventArgs)getPoseInstruction.ResponseContent;
                if (rpuEventArgs != null)
                {
                    robotPose = rpuEventArgs.RobotPose.ToHPose();
                }
                return robotPose;
            }
            else
            {
                RobotCommunicationErrorEventArgs rceEventArgs = (RobotCommunicationErrorEventArgs)getPoseInstruction.ResponseContent;
                return new HTuple();
            }

        }

        public static async Task RequestRobotMotion(IGenericMessageBusConnector caller, string robotName, RobotPose newPose, bool isApproach)
        {
            BusInstruction moveRobotInstruction = new BusInstruction(caller, "MoveRobotToPose", CommandType.MoveRobotToPosition, robotName, AnnouncementType.RobotMotionComplete, true);
            await moveRobotInstruction.RunCommandAsync(new RobotMotionRequestEventArgs(newPose, isApproach));
            return;
        }
    }

}
