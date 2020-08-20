using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    public class TotalPhysiqueSolver : SolverBase
    {
        const int BasePhysique = 50;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalPhysique =
                (BasePhysique + summedStats.Physique + fullBuild.Physique * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.PhysiqueModifier / 100);
            result = new SolverResult { Text = $"{totalPhysique:0} Total Physique", Value = totalPhysique };
            return totalPhysique != 0;
        }
    }
}
