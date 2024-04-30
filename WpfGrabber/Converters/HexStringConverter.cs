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
    [ValueConversion(typeof(string), typeof(int))]
    public class HexStringConverter : IValueConverter
    {
        /// <summary>
        /// number of hex numbers, 0 means do not convert to hex string
        /// </summary>
        public int HexNums { get; set; } = 4;
        public string Prefix { get; set; } = "0x";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return value;
            if ((value is int i) && HexNums>0)
            {
                return Prefix + i.ToString("X" + HexNums);
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
            if (s.StartsWith("x"))
                return int.Parse(s.Substring(1), NumberStyles.HexNumber);
            if (s.StartsWith("0x"))
                return int.Parse(s.Substring(2), NumberStyles.HexNumber);
            return value;
        }
    }
}
