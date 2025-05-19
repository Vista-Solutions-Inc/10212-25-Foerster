using System.Windows.Controls;

namespace Foerster.ViewModels.Helpers
{
    public class DetailsTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if(element != null && item is DeviceVisual device) 
            {
                switch (device.DeviceType)
                {
                    case DevicesLibrary.DeviceTypes.RobotDeviceFullControl:
                        return element.FindResource("RobotDeviceFullControlTemplate") as DataTemplate;
                        break;

                    case DevicesLibrary.DeviceTypes.RobotDeviceLimitedControl:
                        return element.FindResource("RobotDetailsLimitedControlTemplate") as DataTemplate;
                        break;
                    case DevicesLibrary.DeviceTypes.ImagingDevice:
                        return element.FindResource("CameraDetailsTemplate") as DataTemplate;
                        break;
                }
            }
            return null;
            }
    }
}
