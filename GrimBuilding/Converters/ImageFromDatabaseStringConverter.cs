using GrimBuildingCodecs;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    class ImageFromDatabaseStringConverter : IMultiValueConverter
    {
        readonly static Dictionary<string, BitmapSource> cache = new Dictionary<string, BitmapSource>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is null || values[1] == DependencyProperty.UnsetValue) return null;

            string file = (string)values[0];
            if (cache.TryGetValue(file, out var bmp))
                return bmp;

            using var stream = ((LiteDatabase)values[1]).FileStorage.OpenRead(file);
            var bytes = new byte[stream.Length];
            stream.Read(bytes);

            if (!WebP.Decode(bytes, out var hasAlpha, out var width, out var height, out var stride, out var outputBytes))
                throw new InvalidOperationException();

            bmp = BitmapSource.Create(width, height, 0, 0, hasAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24, null, outputBytes, stride);

            cache.Add(file, bmp);

            return bmp;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}