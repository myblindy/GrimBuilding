using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrimBuilding.Solvers
{
    public class TotalPhysiqueSolver : SolverBase
    {
        const int BasePhysique = 50;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalPhysique =
                (BasePhysique + summedStats.Physique + fullBuild.Physique * FullBuildModel.TotalAttributesPerAttributePoint
                    + fullBuild.GetAllFromMasteries(w => w.Physique).Sum())
                * (1 + (summedStats.PhysiqueModifier + fullBuild.GetAllFromMasteries(w => w.PhysiqueModifier).Sum()) / 100);

            return new($"{totalPhysique:0} Total Physique");
        }
    }

    public class TotalCunningSolver : SolverBase
    {
        const int BaseCunning = 50;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalCunning =
                (BaseCunning + summedStats.Cunning + fullBuild.Cunning * FullBuildModel.TotalAttributesPerAttributePoint
                    + fullBuild.GetAllFromMasteries(w => w.Cunning).Sum())
                * (1 + (summedStats.CunningModifier + fullBuild.GetAllFromMasteries(w => w.CunningModifier).Sum()) / 100);

            return new($"{totalCunning:0} Total Cunning");
        }
    }

    public class TotalSpiritSolver : SolverBase
    {
        const int BaseSpirit = 50;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalSpirit =
                (BaseSpirit + summedStats.Spirit + fullBuild.Spirit * FullBuildModel.TotalAttributesPerAttributePoint
                    + fullBuild.GetAllFromMasteries(w => w.Spirit).Sum())
                * (1 + (summedStats.SpiritModifier + fullBuild.GetAllFromMasteries(w => w.SpiritModifier).Sum()) / 100);

            return new($"{totalSpirit:0} Total Spirit");
        }
    }
}
