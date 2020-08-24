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
    class DescriptionToSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var span = new Span();

            if (value is string s)
                foreach (var item in s.Split('^'))
                    if (span.Inlines.Count == 0)
                        span.Inlines.Add(new Run { Text = item });
                    else
                        span.Inlines.Add(new Run
                        {
                            Text = item[1..],
                            Foreground = item[0] switch
                            {
                                'o' => Brushes.DarkOrange,
                                'w' => Brushes.Black,
                                _ => throw new NotImplementedException()
                            }
                        });

            return span;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}