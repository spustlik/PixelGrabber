﻿using System;
using System.Windows;
using System.Windows.Data;

namespace WpfGrabber.Converters
{
    public class EqualsConverter : IValueConverter
    {
        public IValueConverter Inner { get; set; }
        public bool Negate { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            var equals = value.Equals(parameter);
            if (Negate)
                equals = !equals;
            object result = equals;
            if (Inner != null)
            {
                result = Inner.Convert(result, targetType, null, culture);
            }
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
