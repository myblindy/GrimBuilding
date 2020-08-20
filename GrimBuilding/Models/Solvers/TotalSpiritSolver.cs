using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    public class TotalSpiritSolver : SolverBase
    {
        const int BaseSpirit = 50;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalSpirit =
                (BaseSpirit + summedStats.Spirit + fullBuild.Spirit * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.SpiritModifier / 100);
            result = new SolverResult { Text = $"{totalSpirit:0} Total Spirit", Value = totalSpirit };
            return totalSpirit != 0;
        }
    }
}
