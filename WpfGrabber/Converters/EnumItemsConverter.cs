using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
            var attributes = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return (attributes.First() as DescriptionAttribute).Description;

            // If no description is found, the least we can do is replace underscores with spaces
            // You can add your own custom default formatting logic here
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }
    }


}
