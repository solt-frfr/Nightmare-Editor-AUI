using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Nightmare_Editor_AUI.Controls;

    public class PixelWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && double.Parse(parameter.ToString()) is double width)
                return d * width / 240.0;

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PixelMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                var parts = parameter.ToString().Split('|');
                if (parts.Length == 1)
                {
                    return new Thickness(d * double.Parse(parts[0]) / 240.0);
                }
                else if (parts.Length == 2)
                {
                    return new Thickness(d * double.Parse(parts[0]) / 240.0, d * double.Parse(parts[1]) / 240.0);
                }
                else if (parts.Length == 4)
                {
                    return new Thickness(d * double.Parse(parts[0]) / 240.0, d * double.Parse(parts[1]) / 240.0, d * double.Parse(parts[2]) / 240.0, d * double.Parse(parts[3]) / 240.0);
                }
            }

            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    
    public class PixelPoint : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                var parts = parameter.ToString().Split('.');
                if (parts.Length == 2)
                {
                    return new Point(d * double.Parse(parts[0]) / 240.0, d * double.Parse(parts[1]) / 240.0);
                }
            }

            return new Point(0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }