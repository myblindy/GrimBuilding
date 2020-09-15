using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GrimBuilding.Solvers
{
    public class TotalResistancesSolver : SolverBase
    {
        private static readonly ResistanceType[] ResistanceOrder = new[]
        {
            ResistanceType.Fire, ResistanceType.Cold, ResistanceType.Lightning, ResistanceType.Poison, ResistanceType.Pierce,
            ResistanceType.Bleed, ResistanceType.Vitality, ResistanceType.Aether, ResistanceType.Stun, ResistanceType.Chaos
        };

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var formatBuilder = new StringBuilder();
            int formatIndex = 0;
            foreach (var part in ResistanceOrder.Select((res, idx) => $"$({res}ResistanceImage) {{{idx}:0}}%"))
            {
                formatBuilder.Append(part);
                formatBuilder.Append(formatIndex == 4 ? "$(NewLine)" : formatIndex < 9 ? "$(NewCell)" : "");
            }

            return new TotalResistancesSolverResult(FormattableStringFactory.Create(formatBuilder.ToString(),
                ResistanceOrder.Select(res => summedStats.GetResistance(res) +
                    (res == ResistanceType.Fire || res == ResistanceType.Cold || res == ResistanceType.Lightning ? summedStats.ResistElemental : 0))
                    .ToArray()));
        }
    }

    public class TotalResistancesSolverResult : SolverResult
    {
        public TotalResistancesSolverResult(FormattableString formattableString) : base(formattableString)
        {
        }
    }
}
