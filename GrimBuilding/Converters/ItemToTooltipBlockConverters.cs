using GrimBuilding.Common.Support;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    static class SpanExtensions
    {
        internal static void AddSpan(this List<Span> spans, bool condition, params Run[] runs)
        {
            if (condition)
            {
                var span = new Span();
                span.Inlines.AddRange(runs);
                spans.Add(span);
            }
        }
    }

    class ItemToRegularStatsTooltipBlockConverter : IValueConverter
    {
        static readonly Brush ItemTypeTooltipLineBrush = (Brush)Application.Current.Resources["ItemTypeTooltipLineBrush"];
        static Run ValueRun(double value) => new Run(value > 0 ? $"+{value}" : value.ToString()) { Foreground = ItemTypeTooltipLineBrush };
        static Run ValuePercentageRun(double value) => new Run(value > 0 ? $"+{value}%" : value.ToString() + "%") { Foreground = ItemTypeTooltipLineBrush };

        static readonly Brush ItemTextTooltipLineBrush = (Brush)Application.Current.Resources["ItemTextTooltipLineBrush"];
        static Run TextRun(string value) => new Run(value) { Foreground = ItemTextTooltipLineBrush };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Item item)
            {
                var results = new List<Span>();

                results.AddSpan(item.Life != 0, ValueRun(item.Life), TextRun(" Health"));
                results.AddSpan(item.LifeModifier != 0, ValuePercentageRun(item.LifeModifier), TextRun(" Health"));
                results.AddSpan(item.Physique != 0, ValueRun(item.Physique), TextRun(" Physique"));
                results.AddSpan(item.Cunning != 0, ValueRun(item.Cunning), TextRun(" Cunning"));
                results.AddSpan(item.Spirit != 0, ValueRun(item.Spirit), TextRun(" Spirit"));

                return results;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}