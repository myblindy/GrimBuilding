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
    class ImageFromDatabaseStringConverter : IMultiValueConverter
    {
        readonly static Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is null || values[1] == DependencyProperty.UnsetValue) return null;

            string file = (string)values[0];
            if (cache.TryGetValue(file, out var bmp))
                return bmp;

            using var stream = ((LiteDatabase)values[1]).FileStorage.OpenRead(file);

            bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = stream;
            bmp.EndInit();
            cache.Add(file, bmp);

            return bmp;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}