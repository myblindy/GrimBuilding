using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrimBuilding.Solvers
{
    [SolverDependency(typeof(TotalCunningSolver))]
    public class TotalOffensiveAbilitySolver : SolverBase
    {
        const int BaseOA = 1265, ExtraOA = 53;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalCunning = results[typeof(TotalCunningSolver)].Values[0];
            var totalOA = (BaseOA + totalCunning / 2 + summedStats.OffensiveAbility + fullBuild.GetAllFromMasteries(w => w.OffensiveAbility).Sum())
                * (1 + (summedStats.OffensiveAbilityModifier + fullBuild.GetAllFromMasteries(w => w.OffensiveAbilityModifier).Sum()) / 100)
                + ExtraOA;

            return new($"{totalOA:0} Offensive Ability");
        }
    }

    [SolverDependency(typeof(TotalPhysiqueSolver))]
    public class TotalDefensiveAbilitySolver : SolverBase
    {
        const int BaseDA = 1265, ExtraDA = 53;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalPhysique = results[typeof(TotalPhysiqueSolver)].Values[0];
            var totalDA = (BaseDA + totalPhysique / 2 + summedStats.DefensiveAbility + fullBuild.GetAllFromMasteries(w => w.DefensiveAbility).Sum())
                * (1 + (summedStats.DefensiveAbilityModifier + fullBuild.GetAllFromMasteries(w => w.DefensiveAbilityModifier).Sum()) / 100)
                + ExtraDA;

            return new($"{totalDA:0} Defensive Ability");
        }
    }
}
