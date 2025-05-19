using DevicesLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui;

namespace Foerster.ViewModels.VisualObjects
{
    public partial class RobotDeviceLimitedControlVisual : DeviceVisual
    {


        [ObservableProperty] double _setHomePositionX = 0;
        [ObservableProperty] double _setHomePositionY = 0;
        [ObservableProperty] double _setHomePositionZ = 0;
        [ObservableProperty] double _setHomePositionRX = 0;
        [ObservableProperty] double _setHomePositionRY = 0;
        [ObservableProperty] double _setHomePositionRZ = 0;

        [ObservableProperty] double _currentPositionX = 0;
        [ObservableProperty] double _currentPositionY = 0;
        [ObservableProperty] double _currentPositionZ = 0;
        [ObservableProperty] double _currentPositionRX = 0;
        [ObservableProperty] double _currentPositionRY = 0;
        [ObservableProperty] double _currentPositionRZ = 0;
        public RobotDeviceLimitedControlVisual(IDevice device, ISnackbarService snackbarService) : base(device, snackbarService)
        {
            this.DeviceType = DeviceTypes.RobotDeviceLimitedControl;
            // set the background brush by imaging device type
            TileBackground = new SolidColorBrush(Color.FromArgb(120, 204, 255, 229));
            SettingsFilePath = ((RobotDeviceLimitedControl)device).RobotSettingsPath;
            Type robotDeviceType = device.GetType();
            string typeString = robotDeviceType.Name;
            if (((RobotDeviceLimitedControl)device).MotionRoutines.ContainsKey("Home") && ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList != null && ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList.Count==1)
            {

                SetHomePositionX = ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList[0].X;
                SetHomePositionY = ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList[0].Y;
                SetHomePositionZ = ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList[0].Z;

                SetHomePositionRX = ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList[0].RX;
                SetHomePositionRY = ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList[0].RY;
                SetHomePositionRZ = ((RobotDeviceLimitedControl)device).MotionRoutines["Home"].MotionList[0].RZ;
            }
            

            
            switch (typeString)
            {
                case "MotomanHP20":
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/YaskawaMotomanS.png"));
                    break;
                default:
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/RobotWhiteS.png"));
                    break;
            }
            RobotDeviceLimitedControl robotDevice = (RobotDeviceLimitedControl)Device;
            robotDevice.RobotPositionUpdatedEvent -= UpdateCurrentPose;
            robotDevice.RobotPositionUpdatedEvent += UpdateCurrentPose;
        }
        // Methods 


        [RelayCommand]
        private async Task OnGetPosition(object content)
        {
            RobotDeviceLimitedControl robotDevice = (RobotDeviceLimitedControl)Device;
            await robotDevice.GetRobotPose(this, new EventArgs());
        }

        private void UpdateCurrentPose(object? sender, EventArgs e)
        {
            RobotPoseEventArgs robotPoseEventArgs = e as RobotPoseEventArgs;
            if (robotPoseEventArgs == null) return;
            CurrentPositionX = robotPoseEventArgs.RobotPose.X;
            CurrentPositionY = robotPoseEventArgs.RobotPose.Y;
            CurrentPositionZ = robotPoseEventArgs.RobotPose.Z;

            CurrentPositionRX = robotPoseEventArgs.RobotPose.RX;
            CurrentPositionRY = robotPoseEventArgs.RobotPose.RY;
            CurrentPositionRZ = robotPoseEventArgs.RobotPose.RZ;

        }



        [RelayCommand]
        private void OnSetHomePosition(object content)
        {
            //throw new NotImplementedException();
            //
            //robotDevice.GoHome();
            RobotDeviceLimitedControl robotDevice = (RobotDeviceLimitedControl)Device;
            robotDevice.SetHome(new RobotPose(SetHomePositionX, SetHomePositionY, SetHomePositionZ, SetHomePositionRX, SetHomePositionRY, SetHomePositionRZ, robotDevice.PoseType));
        }

        [RelayCommand]
        private void OnSendRobotHome(object content)
        {
            //throw new NotImplementedException();
            RobotDeviceLimitedControl robotDevice = (RobotDeviceLimitedControl)Device;
            robotDevice.GoHome();
        }

        [RelayCommand]
        private void OnStopRobotMotion(object content)
        {
            RobotDeviceLimitedControl robotDevice = (RobotDeviceLimitedControl)Device;
            //robotDevice.StopRobot();
        }

        public override void OnLoosingFocus()
        {

        }
        public override void OnGainingFocus()
        {

        }
    }
}
