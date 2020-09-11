using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    public class TotalPhysiqueSolver : SolverBase
    {
        const int BasePhysique = 50;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalPhysique =
                (BasePhysique + summedStats.Physique + fullBuild.Physique * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.PhysiqueModifier / 100);

            return new($"{totalPhysique:0} Total Physique");
        }
    }
}
