using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    public class TotalCunningSolver : SolverBase
    {
        const int BaseCunning = 50;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalCunning =
                (BaseCunning + summedStats.Cunning + fullBuild.Cunning * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.CunningModifier / 100);
            result = new SolverResult { Text = $"{totalCunning:0} Total Cunning", Value = totalCunning };
            return totalCunning != 0;
        }
    }
}
