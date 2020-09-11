using System;
using System.Linq;

namespace GrimBuilding.Solvers
{
    public class SolverResult
    {
        public SolverResult(FormattableString formattableString)
        {
            FormattableString = formattableString;
            Values = new(formattableString.GetArguments().Select(w => Convert.ToDouble(w)).ToArray());
        }

        public FormattableString FormattableString { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "indexer helper class, let it be internal")]
        public class ValuesWrapper
        {
            readonly double[] values;
            public ValuesWrapper(double[] values) => this.values = values;

            public double this[int index] => values[index];
        }

        public ValuesWrapper Values { get; }
    }
}
