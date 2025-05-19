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
    public partial class RobotDeviceFullControlVisual : DeviceVisual
    {
        public RobotDeviceFullControlVisual(IDevice device, ISnackbarService snackbarService) : base(device, snackbarService)
        {
            this.DeviceType = DeviceTypes.RobotDeviceFullControl ;
            // set the background brush by imaging device type
            TileBackground = new SolidColorBrush(Color.FromArgb(120, 204, 255, 229));
            Type robotDeviceType = device.GetType();
            string typeString = robotDeviceType.Name;
            switch (typeString)
            {
                case "MotomanHP20":
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/YaskawaMotomanS.png"));
                    break;
                default:
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/RobotWhiteS.png"));
                    break;
            }
        }
        // Methods 

        [RelayCommand]
        private void OnSendRobotHome(object content)
        {
            throw new NotImplementedException();
        }

        [RelayCommand]
        private void OnStopRobotMotion(object content)
        {
            throw new NotImplementedException();
        }

        public override void OnLoosingFocus()
        {

        }
        public override void OnGainingFocus()
        {

        }
    }
}
