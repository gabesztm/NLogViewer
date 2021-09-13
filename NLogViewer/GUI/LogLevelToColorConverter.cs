using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NLogViewer
{
    public class LogLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is LogLevel)
            {
                LogLevel level = (LogLevel)value;
                Color color;
                switch (level)
                {
                    default:
                    case LogLevel.Trace:
                        color = Colors.SkyBlue;
                        break;
                    case LogLevel.Info:
                        color = Colors.Gray;
                        break;
                    case LogLevel.Warning:
                        color = Colors.DarkOrange;
                        break;
                    case LogLevel.Error:
                        color = (Color)ColorConverter.ConvertFromString("#cc0000");
                        break;
                }
                return new SolidColorBrush(color);
            }
            return Binding.DoNothing;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
