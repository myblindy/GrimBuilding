using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace GrimBuilding.Solvers
{
    public class TotalResistancesSolver : SolverBase
    {
        private static readonly ResistanceType[] ResistanceOrder = new[]
        {
            ResistanceType.Fire, ResistanceType.Cold, ResistanceType.Lightning, ResistanceType.Poison, ResistanceType.Pierce,
            ResistanceType.Bleed, ResistanceType.Vitality, ResistanceType.Aether, ResistanceType.Stun, ResistanceType.Chaos
        };

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results) =>
            new TotalResistancesSolverResult(FormattableStringFactory.Create(
                string.Join(TotalResistancesSolverResult.NewCellMarker, ResistanceOrder.Select((res, idx) => $"$({res}ResistanceImage) {{{idx}}}%")),
                ResistanceOrder.Select(res => summedStats.GetResistance(res) +
                    (res == ResistanceType.Fire || res == ResistanceType.Cold || res == ResistanceType.Lightning ? summedStats.ResistElemental : 0))
                    .Cast<object>()
                    .ToArray()));
    }

    public class TotalResistancesSolverResult : SolverResult
    {
        public const string NewCellMarker = "$(NewCell)";
        public static readonly Regex ResistanceDataRegex = new(@"^\$\((.*)ResistanceImage\) \{(\d+)\}\%$", RegexOptions.Compiled);

        public TotalResistancesSolverResult(FormattableString formattableString) : base(formattableString) { }
    }
}
