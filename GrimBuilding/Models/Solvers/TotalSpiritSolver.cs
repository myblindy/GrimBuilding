using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    public class TotalSpiritSolver : SolverBase
    {
        const int BaseSpirit = 50;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalSpirit =
                (BaseSpirit + summedStats.Spirit + fullBuild.Spirit * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.SpiritModifier / 100);

            return new($"{totalSpirit:0} Total Spirit");
        }
    }
}
