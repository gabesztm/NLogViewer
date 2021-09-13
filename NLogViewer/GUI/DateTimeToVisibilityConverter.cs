using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NLogViewer
{
    class DateTimeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                return Binding.DoNothing;
            }

            DateTime dateTime = (DateTime)value;
            if(dateTime == DateTime.MinValue)
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
