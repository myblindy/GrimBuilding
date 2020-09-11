using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    public class TotalCunningSolver : SolverBase
    {
        const int BaseCunning = 50;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalCunning =
                (BaseCunning + summedStats.Cunning + fullBuild.Cunning * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.CunningModifier / 100);

            return new($"{totalCunning:0} Total Cunning");
        }
    }
}
