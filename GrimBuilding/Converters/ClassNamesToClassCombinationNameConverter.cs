using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    class ClassNamesToClassCombinationNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            static string GetName(string name)
            {
                var m = Regex.Match(name, @"\[ms\]([^[]*)");
                return m.Success ? m.Groups[1].Value : name;
            }

            if (values[0] is PlayerClass class1 && values[1] is PlayerClass class2 && values[2] is Dictionary<(string c1, string c2), string> dict)
                return dict.TryGetValue((class1.Name, class2.Name), out var name) ? GetName(name) : null;
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}