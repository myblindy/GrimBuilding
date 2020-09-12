using GrimBuilding.Common.Support;
using GrimBuilding.ViewModels;
using GrimBuilding.Codecs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    class ItemRarityToItemRarityStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => 
            values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue
                ? null
                : ((MainWindowViewModel)values[1]).ItemRarityTextStyles[(ItemRarity)values[0]];

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}