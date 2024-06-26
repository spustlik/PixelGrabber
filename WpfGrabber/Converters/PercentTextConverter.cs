﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfGrabber
{
    [ValueConversion(typeof(double), typeof(string))]

    public class PercentTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return value;
            if(value is double d)
            {
                return (d * 100).ToString("0")+" %";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return value;
            if(value is string s)
            {
                //strange, but working on footer
                //s = s.Trim('%').Trim(' ');
                //if (!double.TryParse(s, out var d))
                //    return value;
                //return d / 100;
                if (double.TryParse(s, out var d))
                    return d;
            }
            return value;
        }
    }
}
