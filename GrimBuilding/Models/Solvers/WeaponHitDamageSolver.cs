using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrimBuilding.Solvers
{
    // some references:
    // https://onedrive.live.com/redir?resid=86B50E1BA0EAB821%21146166&authkey=%21AMbqibmMzge55r8&page=View&wd=target%28Offense%20%28Damage%20Calculation%5C%29.one%7C3e3f6269-3cda-4f80-8884-fc6e1159ee6d%2FDamage%20over%20Time%20%28DoT%5C%29%7Cb497443b-4a91-4c89-96bb-5e84ed9b38cf%2F%29
    //
    // The order of events for Conversion is as follows: Base Skill > Skill Modifiers > Conversion on the Skill or Transmuter > Conversion on Equipment and Buffs > Equipment, Auras and Passives
    // 
    // There are four steps:
    //      Skill's base damage is computed. (Based on skill ranks and, if the skill has %weapon damage, weapon damage + global flat damage bonuses.)
    //      +%Damage modifiers on the skill(or any of its nodes) is multiplied.
    //      Damage conversion is applied. (Itself quite a complicated step, depending on the skill and any conversion you have on gear.)
    //      +%Damage modifiers from gear, devotions, temporary buffs, or passive skills is applied.
    // So it is true in a sense that you never "double dip," but damage modifiers on the skill, such as from Cadence or Fighting Form, will get applied before any damage conversion occurs.

    [SolverDependency(typeof(TotalCunningSolver))]
    public class WeaponHitDamageSolver : SolverBase
    {
        const double physicalPierceMultiplierPerCunningPoint = 1.0 / 245.0;
        const double physicalDurationMultiplierPerCunningPoint = 1.0 / 215.0;

        public override SolverResult Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results)
        {
            var totalCunnning = results[typeof(TotalCunningSolver)].Values[0];

            var weaponHitMin = (1 + summedStats.OffensivePhysicalBaseMin + summedStats.OffensivePhysicalBonusMin) * (1 + summedStats.OffensivePhysicalModifier / 100 + totalCunnning * physicalPierceMultiplierPerCunningPoint)
               + summedStats.OffensiveBleedDotTickDamage * (1 + summedStats.OffensiveBleedDotModifier / 100 + totalCunnning * physicalDurationMultiplierPerCunningPoint);
            var weaponHitMax = (1 + summedStats.OffensivePhysicalBaseMax + summedStats.OffensivePhysicalBonusMax) * (1 + summedStats.OffensivePhysicalModifier / 100 + totalCunnning * physicalPierceMultiplierPerCunningPoint)
               + summedStats.OffensiveBleedDotTickDamage * (1 + summedStats.OffensiveBleedDotModifier / 100 + totalCunnning * physicalDurationMultiplierPerCunningPoint);

            return new($"{Math.Round(weaponHitMin)}-{Math.Round(weaponHitMax)} Weapon Hit");
        }
    }
}
