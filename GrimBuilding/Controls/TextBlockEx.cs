using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GrimBuilding.Controls
{
    public static class TextBlockEx
    {
        public static Inline GetFormattedText(DependencyObject obj) =>
            (Inline)obj?.GetValue(FormattedTextProperty);

        public static void SetFormattedText(DependencyObject obj, Inline value) =>
            obj?.SetValue(FormattedTextProperty, value);

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText", typeof(Inline), typeof(TextBlockEx), new PropertyMetadata(null, OnFormattedTextChanged));

        private static void OnFormattedTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (!(o is TextBlock textBlock)) return;

            var inline = (Inline)e.NewValue;
            textBlock.Inlines.Clear();
            if (inline != null)
                textBlock.Inlines.Add(inline);
        }
    }
}