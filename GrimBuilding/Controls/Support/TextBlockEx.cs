using GrimBuilding.Common.Support;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GrimBuilding.Controls.Support
{
    public static class TextBlockEx
    {
        public static Inline GetFormattedText(DependencyObject obj) =>
            (Inline)obj?.GetValue(FormattedTextProperty);

        public static void SetFormattedText(DependencyObject obj, Inline value) =>
            obj?.SetValue(FormattedTextProperty, value);

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText", typeof(Inline), typeof(TextBlockEx), new(OnFormattedTextChanged));

        private static void OnFormattedTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not TextBlock textBlock) return;

            var inline = (Inline)e.NewValue;
            textBlock.Inlines.Clear();
            if (inline != null)
                textBlock.Inlines.Add(inline);
        }

        public static ItemRarityTextStyle GetDatabaseStyle(DependencyObject obj) =>
            (ItemRarityTextStyle)obj?.GetValue(DatabaseStyleProperty);

        public static void SetDatabaseStyle(DependencyObject obj, ItemRarityTextStyle value) =>
            obj?.SetValue(DatabaseStyleProperty, value);

        public static readonly DependencyProperty DatabaseStyleProperty =
            DependencyProperty.RegisterAttached("DatabaseStyle", typeof(ItemRarityTextStyle), typeof(TextBlockEx), new(OnDatabaseStyleChanged));

        private static void OnDatabaseStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is TextBlock textBlock && e.NewValue is ItemRarityTextStyle itemRarityTextStyle)
            {
                textBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(itemRarityTextStyle.R * 255), (byte)(itemRarityTextStyle.G * 255), (byte)(itemRarityTextStyle.B * 255)));
                textBlock.FontWeight = itemRarityTextStyle.Bold ? FontWeights.Bold : FontWeights.Normal;
                textBlock.FontStyle = itemRarityTextStyle.Italic ? FontStyles.Italic : FontStyles.Normal;
            }
        }
    }
}