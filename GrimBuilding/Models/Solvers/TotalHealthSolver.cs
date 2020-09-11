﻿using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    [SolverDependency(typeof(TotalPhysiqueSolver)), SolverDependency(typeof(TotalCunningSolver)), SolverDependency(typeof(TotalSpiritSolver))]
    public class TotalHealthSolver : SolverBase
    {
        const double HealthPerPhysiquePoint = 20.0 / 8.0;
        const int HealthPerOtherPoint = 1;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalPhysique = results[typeof(TotalPhysiqueSolver)].Values[0];
            var totalCunning = results[typeof(TotalCunningSolver)].Values[0];
            var totalSpirit = results[typeof(TotalSpiritSolver)].Values[0];

            var totalHealth =
                (FullBuildModel.BaseHealth + summedStats.Life +
                    HealthPerPhysiquePoint * totalPhysique +
                    HealthPerOtherPoint * (totalCunning + totalSpirit))
                * (1 + summedStats.LifeModifier / 100);

            return new($"{totalHealth:0} Total Health");
        }
    }
}
