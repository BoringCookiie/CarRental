using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarRental.BackOffice.Converters
{
    /// <summary>
    /// Converts a boolean to Visibility (true = Visible, false = Collapsed)
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts a boolean to inverted Visibility (true = Collapsed, false = Visible)
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Converts null to bool (not null = true, null = false)
    /// </summary>
    public class NotNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts RentalStatus to a SolidColorBrush for visual indication
    /// </summary>
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CarRental.Core.Enums.RentalStatus status)
            {
                return status switch
                {
                    CarRental.Core.Enums.RentalStatus.Pending => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange),
                    CarRental.Core.Enums.RentalStatus.Confirmed => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue),
                    CarRental.Core.Enums.RentalStatus.Active => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green),
                    CarRental.Core.Enums.RentalStatus.Completed => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray),
                    CarRental.Core.Enums.RentalStatus.Cancelled => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red),
                    _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray)
                };
            }
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
