using GrimBuilding.Solvers;
using GrimBuilding.Codecs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GrimBuilding.Converters.Support;

namespace GrimBuilding.Converters
{
    class FormattableStringWithOtherFullBuildConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is SolverResult result)
            {
                if (values[1] is IEnumerable<SolverResult> otherResults)
                {
                    var otherResult = otherResults.FirstOrDefault(r => r.FormattableString.Format == result.FormattableString.Format);
                    if (otherResult is not null)
                    {
                        var matches = ConverterHelpers.SolverFormattableStringRegex.Matches(result.FormattableString.Format);

                        var span = new Span();
                        int valIdx = 0;
                        foreach (Match match in matches)
                            if (match.Value[0] == '{')
                            {
                                var value = result.Values[valIdx];
                                var format0 = Regex.Replace(match.Value, @"^{\d+(\:[^}]+)?}$", "{0$1}");
                                span.Inlines.Add(new Run(string.Format(format0, value)));
                                var otherValue = otherResult.Values[valIdx];
                                if (value != otherValue)
                                    span.Inlines.Add(new Run($"({(otherValue > value ? "+" : null)}{otherValue - value:0})") { Foreground = otherValue > value ? ConverterHelpers.PositiveDifferenceBrush : ConverterHelpers.NegativeDifferenceBrush });

                                ++valIdx;
                            }
                            else
                                span.Inlines.Add(new Run(match.Value));

                        return span;
                    }
                }

                return new Span { Inlines = { new Run(result.FormattableString.ToString()) } };
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}