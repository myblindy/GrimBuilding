using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrimBuilding.Solvers
{
    [SolverDependency(typeof(TotalCunningSolver))]
    public class WeaponHitDamageSolver : SolverBase
    {
        const double physicalPierceMultiplierPerCunningPoint = 0.23673469387755098 / 58.0;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalCunnning = results[typeof(TotalCunningSolver)].Values[0];

            var weaponHitMin = (1 + summedStats.OffensivePhysicalBaseMin) * (1 + summedStats.OffensivePhysicalModifier / 100 + totalCunnning * physicalPierceMultiplierPerCunningPoint);
            var weaponHitMax = (1 + summedStats.OffensivePhysicalBaseMax) * (1 + summedStats.OffensivePhysicalModifier / 100 + totalCunnning * physicalPierceMultiplierPerCunningPoint);

            result = new SolverResult
            {
                Values = new[] { weaponHitMin, weaponHitMax },
                Text = $"{Math.Round(weaponHitMin)}-{Math.Round(weaponHitMax)} Weapon Hit",
            };
            return true;
        }
    }
}
