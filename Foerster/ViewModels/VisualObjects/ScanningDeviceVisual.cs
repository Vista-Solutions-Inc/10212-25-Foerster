using DevicesLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Wpf.Ui;

namespace Foerster.ViewModels.VisualObjects
{
    public partial class ScanningDeviceVisual : DeviceVisual
    {
        public ScanningDeviceVisual(IDevice device, ISnackbarService snackbarService) : base(device, snackbarService)
        {
            this.DeviceType = DeviceTypes.ScanningDevice;
            // set the background brush by imaging device type

            Type scanningDeviceType = device.GetType();
            string typeName = scanningDeviceType.Name;
            switch (typeName)
            {
                case "LJXDirectScanner":
                    TileBackground = new SolidColorBrush(Color.FromArgb(120, 0, 150, 255));
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/KeyenceLJ-X8000.png"));
                    break;
                case "VirtualLJXDirectScanner":
                    TileBackground = new SolidColorBrush(Color.FromArgb(120, 88, 0, 255));
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/VirtualScan.png"));
                    break;
                default:
                    TileBackground = new SolidColorBrush(Color.FromArgb(120, 0, 150, 255));
                    DeviceImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/KeyenceLJ-X8000.png"));
                    break;
            }
        }
        // Methods
        public override void OnLoosingFocus()
        {

        }
        public override void OnGainingFocus()
        {

        }
    }
}
