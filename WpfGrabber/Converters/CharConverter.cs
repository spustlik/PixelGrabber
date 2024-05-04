using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfGrabber
{
    [ValueConversion(typeof(int), typeof(char))]

    public class CharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return value;
            if ((value is int i))
            {
                return Char.ConvertFromUtf32(i);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return value;
            var s = value as string;
            if (String.IsNullOrEmpty(s))
                return value;

            return value;
        }
    }
}
