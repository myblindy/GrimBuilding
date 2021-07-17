using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GrimBuilding.Converters.Support
{
    static class ConverterHelpers
    {
        public static readonly Regex SolverFormattableStringRegex = new(@"\{(\d+)(?::([^}]+))?\}|([^{]+)", RegexOptions.Compiled);
        public static readonly Brush PositiveDifferenceBrush = Brushes.MediumSeaGreen;
        public static readonly Brush NegativeDifferenceBrush = Brushes.Red;
    }
}
