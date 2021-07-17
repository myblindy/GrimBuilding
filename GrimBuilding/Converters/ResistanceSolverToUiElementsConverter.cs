using GrimBuilding.Common;
using GrimBuilding.Common.Support;
using GrimBuilding.Controls;
using GrimBuilding.Converters.Support;
using GrimBuilding.Solvers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    class ResistanceSolverToUiElementsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is TotalResistancesSolverResult result && values[1] is Dictionary<ResistanceType, PlayerResistance> resistances)
            {
                var cells = result.FormattableString.Format.Split(TotalResistancesSolverResult.NewCellMarker);
                var otherResults = values[2] as IEnumerable<SolverResult>;
                var otherResult = otherResults?.FirstOrDefault(r => r.FormattableString.Format == result.FormattableString.Format);

                return cells.Select(cell =>
                {
                    var cellMatch = TotalResistancesSolverResult.ResistanceDataRegex.Match(cell);
                    var cellIndex = System.Convert.ToInt32(cellMatch.Groups[2].Value);

                    var tbSpan = new Span
                    {
                        Inlines =
                        {
                            result.Values[cellIndex].ToString("0") + "%",
                        }
                    };
                    if (otherResult is not null)
                    {
                        var difference = otherResult.Values[cellIndex] - result.Values[cellIndex];
                        if (difference != 0)
                            tbSpan.Inlines.Add(new Run($"({(difference > 0 ? "+" : "")}{difference:0.##})") { Foreground = difference > 0 ? ConverterHelpers.PositiveDifferenceBrush : ConverterHelpers.NegativeDifferenceBrush });
                    }

                    return new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Children =
                        {
                            new DatabaseImageControl
                            {
                                Path = resistances[Enum.Parse<ResistanceType>(cellMatch.Groups[1].Value)].BitmapPath,
                                MaxHeight = 24,
                            },
                            new TextBlock
                            {
                                TextAlignment = TextAlignment.Center,
                                Inlines = { tbSpan },
                            }
                        },
                    };
                }).ToList();
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}