using GrimBuilding.Common.Support;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    class ItemTypeToDisplayText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is null ? null :
            (ItemType)value switch
            {
                ItemType.Feet => "Boots",
                _ => value.ToString(),
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}