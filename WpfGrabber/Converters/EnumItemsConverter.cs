using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfGrabber.Converters
{
    public class ValueDescription
    {
        public object Value { get; set; }
        public string Description { get; set; }
    }

    //[ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescription>))]
    public class EnumItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return value;
            var t = value.GetType();
            if (!t.IsEnum)
                throw new ArgumentException($"{t.Name} must be an enum type");

            return Enum
                .GetValues(t)
                .Cast<Enum>()
                .Select((e) => new ValueDescription() { Value = e, Description = GetDescription(e) })
                .ToList();

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public static string GetDescription(Enum value)
        {
            var attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttribute<DescriptionAttribute>();
            if (attribute != null)
                return attribute.Description;
            var v = value.ToString();
            return AddSpacesToUpperCase(v);
        }

        public static string AddSpacesToUpperCase(string v)
        {
            bool isBig(char c) => Char.IsUpper(c);

            if (v == null || string.IsNullOrEmpty(v))
                return v;
            var last = isBig(v[0]);
            var r = "";
            for (int i = 0; i < v.Length; i++)
            {
                var c = v[i];
                if (!last && isBig(c))
                {
                    r += " ";
                }
                last = isBig(c);
                r += c;
            }
            return r;

        }
    }


}
