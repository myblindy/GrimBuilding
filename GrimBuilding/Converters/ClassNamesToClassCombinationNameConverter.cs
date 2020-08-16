using GrimBuilding.Common.Support;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (values[0] is null || values[1] is null || values[2] == DependencyProperty.UnsetValue) return null;

            var dict = (Dictionary<(string c1, string c2), string>)values[2];
            return dict.TryGetValue((((PlayerClass)values[0]).Name, ((PlayerClass)values[1]).Name), out var name) ? name : null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}