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
                        foreach (Match match in matches)
                            if (match.Groups[1].Success)
                            {
                                var valIdx = int.Parse(match.Groups[1].Value);
                                var valFmt = match.Groups[2].Success ? match.Groups[2].Value : null;

                                var value = result.Values[valIdx];
                                span.Inlines.Add(new Run(value.ToString(valFmt)));
                                var otherValue = otherResult.Values[valIdx];
                                if (value != otherValue)
                                {
                                    var difference = (otherValue - value).ToString(valFmt);
                                    span.Inlines.Add(new Run($"({(otherValue > value ? "+" : null)}{difference})") { Foreground = otherValue > value ? ConverterHelpers.PositiveDifferenceBrush : ConverterHelpers.NegativeDifferenceBrush });
                                }
                            }
                            else
                                span.Inlines.Add(new Run(match.Value));

                        return span;
                    }
                }

                return new Span
                {
                    Inlines = { new Run(result.FormattableString.ToString()) }
                };
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}