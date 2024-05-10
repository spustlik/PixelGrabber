using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfGrabber
{
    [ValueConversion(typeof(double), typeof(double))]

    [ValueConversion(typeof(int), typeof(int))]

    public class Mul100Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d * 100;
            if (value is int i)
                return i * 100;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d / 100;
            if (value is int i)
                return i / 100;
            return value;
        }
    }
}
