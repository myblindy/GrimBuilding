﻿using GrimBuilding.Codecs;
using GrimBuilding.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    class ImageFromDatabaseStringConverter : IMultiValueConverter
    {
        readonly static ConcurrentDictionary<string, BitmapSource> cache = new();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is null || values[1] == DependencyProperty.UnsetValue) return null;

            string file = (string)values[0];
            return cache.GetOrAdd(file, file =>
            {
                var bytes = ((GdDbContext)values[1]).Files.First(w => w.Path == file).Data;

                if (!WebP.Decode(bytes, out var hasAlpha, out var width, out var height, out var stride, out var outputBytes))
                    throw new InvalidOperationException();

                return BitmapSource.Create(width, height, 0, 0, hasAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24, null, outputBytes, stride);
            });
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}