using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace Foerster.Helpers
{
    #region Converters
    [ValueConversion(typeof(ObservableCollection<int>), typeof(string))]
    public class IndexListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var indicesList = value as ObservableCollection<int>;
            var prefix = parameter as string;

            if (indicesList != null)
            {
                return string.Join(", ", indicesList.Select(index => $"{prefix}{index}"));
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"'{nameof(IndexListToStringConverter)}' is a one-way converter.");
        }
    }

    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? boolValue = (bool?)value;
            Visibility visibilityValue;
            switch (boolValue)
            {
                case true:
                    visibilityValue = Visibility.Visible;
                    break;
                case false: 
                    visibilityValue = Visibility.Collapsed;
                    break;
                default:
                    visibilityValue = Visibility.Collapsed;
                    break;
            }
            
            return visibilityValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"'{nameof(BooleanToVisibilityConverter)}' is a one-way converter.");
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class TaskStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isConfigured = (bool)value;
            if (isConfigured)
            {
                return "Configured";
            }
            else
            {
                return "Not configured";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("TaskStatusToStringConverter is a one-way converter.");
        }
    }
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class TaskStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isConfigured = (bool)value;
            if (isConfigured)
            {
                return new SolidColorBrush(Colors.Green);
            }
            else
            {
                return new SolidColorBrush(Colors.Red);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("TaskStatusToColorConverter is a one-way converter.");
        }
    }

    #endregion
}
