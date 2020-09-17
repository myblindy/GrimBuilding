using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GrimBuilding.Converters
{
    class MasterySkillAllocatedPointDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values[0] is int allocated && values[1] is int maxLevel
                ? $"{Math.Clamp(allocated, 0, (values[2] as int?) ?? maxLevel)} / {maxLevel}"
                : null;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class MasterySkillAllocatedPointDisplayForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values[0] is int allocated && values[1] is int maxLevel && allocated > maxLevel
                ? Brushes.Blue
                : Brushes.White;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
