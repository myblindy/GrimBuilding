using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    [SolverDependency(typeof(TotalPhysiqueSolver)), SolverDependency(typeof(TotalCunningSolver)), SolverDependency(typeof(TotalSpiritSolver))]
    public class TotalHealthSolver : SolverBase
    {
        const double HealthPerPhysiquePoint = 20.0 / 8.0;
        const int HealthPerOtherPoint = 1;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalPhysique = results[typeof(TotalPhysiqueSolver)].Value;
            var totalCunning = results[typeof(TotalCunningSolver)].Value;
            var totalSpirit = results[typeof(TotalSpiritSolver)].Value;

            var totalHealth =
                (FullBuildModel.BaseHealth + summedStats.Life +
                    HealthPerPhysiquePoint * totalPhysique +
                    HealthPerOtherPoint * (totalCunning + totalSpirit))
                * (1 + summedStats.LifeModifier / 100);
            result = new SolverResult { Text = $"{totalHealth:0} Total Health", Value = totalHealth };
            return totalHealth != 0;
        }
    }
}
