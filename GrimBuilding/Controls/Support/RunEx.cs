using GrimBuilding.Common.Support;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GrimBuilding.Controls.Support
{
    public static class RunEx
    {
        public static ItemRarityTextStyle GetDatabaseStyle(DependencyObject obj) =>
            (ItemRarityTextStyle)obj?.GetValue(DatabaseStyleProperty);

        public static void SetDatabaseStyle(DependencyObject obj, ItemRarityTextStyle value) =>
            obj?.SetValue(DatabaseStyleProperty, value);

        public static readonly DependencyProperty DatabaseStyleProperty =
            DependencyProperty.RegisterAttached("DatabaseStyle", typeof(ItemRarityTextStyle), typeof(RunEx), new(null, OnDatabaseStyleChanged));

        private static void OnDatabaseStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is Run run && e.NewValue is ItemRarityTextStyle itemRarityTextStyle)
            {
                run.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(itemRarityTextStyle.R * 255), (byte)(itemRarityTextStyle.G * 255), (byte)(itemRarityTextStyle.B * 255)));
                run.FontWeight = itemRarityTextStyle.Bold ? FontWeights.Bold : FontWeights.Normal;
                run.FontStyle = itemRarityTextStyle.Italic ? FontStyles.Italic : FontStyles.Normal;
            }
        }
    }
}