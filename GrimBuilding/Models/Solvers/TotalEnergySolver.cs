using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    [SolverDependency(typeof(TotalSpiritSolver))]
    public class TotalEnergySolver : SolverBase
    {
        const int EnergyPerSpiritPoint = 2;
        const int BaseEnergyAt50Spirit = 250;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalSpirit = results[typeof(TotalSpiritSolver)].Values[0];

            var totalEnergy =
                (summedStats.Energy + EnergyPerSpiritPoint * (totalSpirit - 50) + BaseEnergyAt50Spirit)
                * (1 + summedStats.EnergyModifier / 100);
            result = new SolverResult { Text = $"{totalEnergy:0} Total Energy", Values = new[] { totalEnergy } };
            return totalEnergy != 0;
        }
    }
}
