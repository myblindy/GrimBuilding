using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    static class SpanExtensions
    {
        internal static void AddSpan(this List<Span> spans, bool condition, params Run[] runs)
        {
            if (condition)
            {
                var span = new Span();
                span.Inlines.AddRange(runs);
                spans.Add(span);
            }
        }
    }

    class ItemEnumerableToRegularStatsTooltipBlockConverter : IMultiValueConverter
    {
        static readonly ItemToRegularStatsTooltipBlockConverter converter = new();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values[0] is IList<BaseStats> enumerableBaseStats && values[1] is int index
                ? index <= 0 || index >= enumerableBaseStats.Count ? null : converter.Convert(enumerableBaseStats[index - 1], targetType, parameter, culture)
                : null;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ItemToRegularStatsTooltipBlockConverter : IValueConverter
    {
        static readonly Brush ItemTypeTooltipLineBrush = (Brush)Application.Current.Resources["ItemTypeTooltipLineBrush"];
        static Run ValueRun(double value) => new(value > 0 ? $"+{Math.Round(value)}" : Math.Round(value).ToString()) { Foreground = ItemTypeTooltipLineBrush };
        static Run ValueNoPlusRun(double value) => new(Math.Round(value).ToString()) { Foreground = ItemTypeTooltipLineBrush };
        static Run ValueNoPlusRangeRun(double v1, double v2) => v2 == 0
            ? new Run(Math.Round(v1).ToString()) { Foreground = ItemTypeTooltipLineBrush }
            : new Run($"{Math.Round(v1)}-{Math.Round(v2)}") { Foreground = ItemTypeTooltipLineBrush };
        static Run ValuePercentageRun(double value) => new(value > 0 ? $"+{Math.Round(value)}%" : Math.Round(value).ToString() + "%") { Foreground = ItemTypeTooltipLineBrush };
        static Run ValueNoPlusPercentageRun(double value) => new(Math.Round(value).ToString() + "%") { Foreground = ItemTypeTooltipLineBrush };

        static readonly Brush ItemTextTooltipLineBrush = (Brush)Application.Current.Resources["ItemTextTooltipLineBrush"];
        static Run TextRun(string value) => new(value) { Foreground = ItemTextTooltipLineBrush };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BaseStats baseStats)
            {
                var results = new List<Span>();

                results.AddSpan(baseStats.EnergyCost != 0, ValueNoPlusRun(baseStats.EnergyCost), TextRun(" Energy Cost"));
                results.AddSpan(baseStats.LifeMonitorPercent != 0, TextRun("Activates when Health drops below "), ValueNoPlusPercentageRun(baseStats.LifeMonitorPercent));
                results.AddSpan(baseStats.SkillCooldown != 0, ValueNoPlusRun(baseStats.SkillCooldown), TextRun(" Second Skill Recharge"));
                results.AddSpan(baseStats.SkillDuration != 0, ValueNoPlusRun(baseStats.SkillDuration), TextRun(" Second Duration"));

                results.AddSpan(baseStats.RestoreLifePercent != 0, ValueNoPlusPercentageRun(baseStats.RestoreLifePercent), TextRun(" Health Restored"));

                results.AddSpan(baseStats.OffensivePhysicalBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensivePhysicalBonusMin, baseStats.OffensivePhysicalBonusMax), TextRun(" Physical Damage"));
                results.AddSpan(baseStats.OffensivePierceBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensivePierceBonusMin, baseStats.OffensivePierceBonusMax), TextRun(" Pierce Damage"));
                results.AddSpan(baseStats.OffensiveFireBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveFireBonusMin, baseStats.OffensiveFireBonusMax), TextRun(" Fire Damage"));
                results.AddSpan(baseStats.OffensiveElementalBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveElementalBonusMin, baseStats.OffensiveElementalBonusMax), TextRun(" Elemental Damage"));
                results.AddSpan(baseStats.OffensiveColdBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveColdBonusMin, baseStats.OffensiveColdBonusMax), TextRun(" Cold Damage"));
                results.AddSpan(baseStats.OffensiveLightningBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveLightningBonusMin, baseStats.OffensiveLightningBonusMax), TextRun(" Lightning Damage"));
                results.AddSpan(baseStats.OffensivePoisonBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensivePoisonBonusMin, baseStats.OffensivePoisonBonusMax), TextRun(" Poison Damage"));
                results.AddSpan(baseStats.OffensiveVitalityBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveVitalityBonusMin, baseStats.OffensiveVitalityBonusMax), TextRun(" Vitality Damage"));
                results.AddSpan(baseStats.OffensiveAetherBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveAetherBonusMin, baseStats.OffensiveAetherBonusMax), TextRun(" Aether Damage"));
                results.AddSpan(baseStats.OffensiveChaosBonusMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveChaosBonusMin, baseStats.OffensiveChaosBonusMax), TextRun(" Chaos Damage"));

                results.AddSpan(baseStats.OffensiveBleedDotDuration != 0, ValueRun(Math.Round(baseStats.OffensiveBleedDotTickDamage * baseStats.OffensiveBleedDotDuration * (1 + baseStats.AttributeScalePercent))),
                    TextRun(" Bleeding Damage over "), ValueNoPlusRun(baseStats.OffensiveBleedDotDuration), TextRun(" Seconds"));

                results.AddSpan(baseStats.OffensivePhysicalBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensivePhysicalBaseMin, baseStats.OffensivePhysicalBaseMax), TextRun(" Physical Damage"));
                results.AddSpan(baseStats.OffensivePierceBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensivePierceBaseMin, baseStats.OffensivePierceBaseMax), TextRun(" Pierce Damage"));
                results.AddSpan(baseStats.OffensiveFireBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveFireBaseMin, baseStats.OffensiveFireBaseMax), TextRun(" Fire Damage"));
                results.AddSpan(baseStats.OffensiveElementalBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveElementalBaseMin, baseStats.OffensiveElementalBaseMax), TextRun(" Elemental Damage"));
                results.AddSpan(baseStats.OffensiveColdBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveColdBaseMin, baseStats.OffensiveColdBaseMax), TextRun(" Cold Damage"));
                results.AddSpan(baseStats.OffensiveLightningBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveLightningBaseMin, baseStats.OffensiveLightningBaseMax), TextRun(" Lightning Damage"));
                results.AddSpan(baseStats.OffensivePoisonBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensivePoisonBaseMin, baseStats.OffensivePoisonBaseMax), TextRun(" Poison Damage"));
                results.AddSpan(baseStats.OffensiveVitalityBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveVitalityBaseMin, baseStats.OffensiveVitalityBaseMax), TextRun(" Vitality Damage"));
                results.AddSpan(baseStats.OffensiveAetherBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveAetherBaseMin, baseStats.OffensiveAetherBaseMax), TextRun(" Aether Damage"));
                results.AddSpan(baseStats.OffensiveChaosBaseMin != 0, ValueNoPlusRangeRun(baseStats.OffensiveChaosBaseMin, baseStats.OffensiveChaosBaseMax), TextRun(" Chaos Damage"));

                results.AddSpan(baseStats.OffensivePhysicalModifier != 0, ValuePercentageRun(baseStats.OffensivePhysicalModifier), TextRun(" Physical Damage"));
                results.AddSpan(baseStats.OffensivePierceModifier != 0, ValuePercentageRun(baseStats.OffensivePierceModifier), TextRun(" Pierce Damage"));
                results.AddSpan(baseStats.OffensiveFireModifier != 0, ValuePercentageRun(baseStats.OffensiveFireModifier), TextRun(" Fire Damage"));
                results.AddSpan(baseStats.OffensiveColdModifier != 0, ValuePercentageRun(baseStats.OffensiveColdModifier), TextRun(" Cold Damage"));
                results.AddSpan(baseStats.OffensiveLightningModifier != 0, ValuePercentageRun(baseStats.OffensiveLightningModifier), TextRun(" Lightning Damage"));
                results.AddSpan(baseStats.OffensiveElementalModifier != 0, ValuePercentageRun(baseStats.OffensiveElementalModifier), TextRun(" Elemental Damage"));
                results.AddSpan(baseStats.OffensivePoisonModifier != 0, ValuePercentageRun(baseStats.OffensivePoisonModifier), TextRun(" Poison Damage"));
                results.AddSpan(baseStats.OffensiveVitalityModifier != 0, ValuePercentageRun(baseStats.OffensiveVitalityModifier), TextRun(" Vitality Damage"));
                results.AddSpan(baseStats.OffensiveAetherModifier != 0, ValuePercentageRun(baseStats.OffensiveAetherModifier), TextRun(" Aether Damage"));
                results.AddSpan(baseStats.OffensiveChaosModifier != 0, ValuePercentageRun(baseStats.OffensiveChaosModifier), TextRun(" Chaos Damage"));
                results.AddSpan(baseStats.OffensiveKnockdownModifier != 0, ValuePercentageRun(baseStats.OffensiveKnockdownModifier), TextRun(" Knockdown Damage"));
                results.AddSpan(baseStats.OffensiveBleedDotModifier != 0, ValuePercentageRun(baseStats.OffensiveBleedDotModifier), TextRun(" Bleeding Damage"));
                results.AddSpan(baseStats.OffensiveColdDotModifier != 0, ValuePercentageRun(baseStats.OffensiveColdDotModifier), TextRun(" ColdDot Damage"));
                results.AddSpan(baseStats.OffensiveFireDotModifier != 0, ValuePercentageRun(baseStats.OffensiveFireDotModifier), TextRun(" Burn Damage"));
                results.AddSpan(baseStats.OffensiveVitalityDotModifier != 0, ValuePercentageRun(baseStats.OffensiveVitalityDotModifier), TextRun(" Vitality Decay Damage"));
                results.AddSpan(baseStats.OffensiveLightningDotModifier != 0, ValuePercentageRun(baseStats.OffensiveLightningDotModifier), TextRun(" Electrocute Damage"));
                results.AddSpan(baseStats.OffensivePhysicalDotModifier != 0, ValuePercentageRun(baseStats.OffensivePhysicalDotModifier), TextRun(" Internal Trauma Damage"));
                results.AddSpan(baseStats.OffensivePoisonDotModifier != 0, ValuePercentageRun(baseStats.OffensivePoisonDotModifier), TextRun(" PoisonDot Damage"));
                results.AddSpan(baseStats.OffensiveStunModifier != 0, ValuePercentageRun(baseStats.OffensiveStunModifier), TextRun(" Stun Damage"));

                results.AddSpan(baseStats.WeaponDamageModifier != 0, ValuePercentageRun(baseStats.WeaponDamageModifier), TextRun(" Weapon Damage"));
                results.AddSpan(baseStats.TotalDamageModifier != 0, TextRun("Total Damage Modified by "), ValuePercentageRun(baseStats.TotalDamageModifier));

                results.AddSpan(baseStats.Physique != 0, ValueRun(baseStats.Physique), TextRun(" Physique"));
                results.AddSpan(baseStats.PhysiqueModifier != 0, ValuePercentageRun(baseStats.PhysiqueModifier), TextRun(" Physique"));
                results.AddSpan(baseStats.Cunning != 0, ValueRun(baseStats.Cunning), TextRun(" Cunning"));
                results.AddSpan(baseStats.CunningModifier != 0, ValuePercentageRun(baseStats.CunningModifier), TextRun(" Cunning"));
                results.AddSpan(baseStats.Spirit != 0, ValueRun(baseStats.Spirit), TextRun(" Spirit"));
                results.AddSpan(baseStats.SpiritModifier != 0, ValuePercentageRun(baseStats.SpiritModifier), TextRun(" Spirit"));

                results.AddSpan(baseStats.OffensiveAbility != 0, ValueRun(baseStats.OffensiveAbility), TextRun(" Offensive Ability"));
                results.AddSpan(baseStats.OffensiveAbilityModifier != 0, ValuePercentageRun(baseStats.OffensiveAbilityModifier), TextRun(" Offensive Abiltiy"));
                results.AddSpan(baseStats.DefensiveAbility != 0, ValueRun(baseStats.DefensiveAbility), TextRun(" Defensive Ability"));
                results.AddSpan(baseStats.DefensiveAbilityModifier != 0, ValuePercentageRun(baseStats.DefensiveAbilityModifier), TextRun(" Defensive Ability"));
                results.AddSpan(baseStats.RunSpeedModifier != 0, ValuePercentageRun(baseStats.RunSpeedModifier), TextRun(" Movement Speed"));

                results.AddSpan(baseStats.Life != 0, ValueRun(baseStats.Life), TextRun(" Health"));
                results.AddSpan(baseStats.LifeModifier != 0, ValuePercentageRun(baseStats.LifeModifier), TextRun(" Health"));
                results.AddSpan(baseStats.LifeRegeneration != 0, ValueRun(baseStats.LifeRegeneration), TextRun(" Health Regenerated per Second"));
                results.AddSpan(baseStats.LifeRegenerationModifier != 0, TextRun("Increases Health Regeneration by "), ValuePercentageRun(baseStats.LifeRegenerationModifier));
                results.AddSpan(baseStats.Energy != 0, ValueRun(baseStats.Energy), TextRun(" Energy"));
                results.AddSpan(baseStats.EnergyModifier != 0, ValuePercentageRun(baseStats.EnergyModifier), TextRun(" Energy"));
                results.AddSpan(baseStats.EnergyRegeneration != 0, ValueRun(baseStats.EnergyRegeneration), TextRun(" Energy Regenerated per Second"));
                results.AddSpan(baseStats.EnergyRegenerationModifier != 0, TextRun("Increases Energy Regeneration by "), ValuePercentageRun(baseStats.EnergyRegenerationModifier));

                results.AddSpan(baseStats.AttackSpeedModifier != 0, ValuePercentageRun(baseStats.AttackSpeedModifier), TextRun(" Attack Speed"));
                results.AddSpan(baseStats.CastSpeedModifier != 0, ValuePercentageRun(baseStats.CastSpeedModifier), TextRun(" Casting Speed"));

                results.AddSpan(baseStats.ResistPhysical != 0, ValuePercentageRun(baseStats.ResistPhysical), TextRun(" Physical Resistance"));
                results.AddSpan(baseStats.ResistPierce != 0, ValuePercentageRun(baseStats.ResistPierce), TextRun(" Pierce Resistance"));
                results.AddSpan(baseStats.ResistFire != 0, ValuePercentageRun(baseStats.ResistFire), TextRun(" Fire Resistance"));
                results.AddSpan(baseStats.ResistCold != 0, ValuePercentageRun(baseStats.ResistCold), TextRun(" Cold Resistance"));
                results.AddSpan(baseStats.ResistLightning != 0, ValuePercentageRun(baseStats.ResistLightning), TextRun(" Lightning Resistance"));
                results.AddSpan(baseStats.ResistPoison != 0, ValuePercentageRun(baseStats.ResistPoison), TextRun(" Poison & Acid Resistance"));
                results.AddSpan(baseStats.ResistVitality != 0, ValuePercentageRun(baseStats.ResistVitality), TextRun(" Vitality Resistance"));
                results.AddSpan(baseStats.ResistAether != 0, ValuePercentageRun(baseStats.ResistAether), TextRun(" Aether Resistance"));
                results.AddSpan(baseStats.ResistChaos != 0, ValuePercentageRun(baseStats.ResistChaos), TextRun(" Chaos Resistance"));
                results.AddSpan(baseStats.ResistElemental != 0, ValuePercentageRun(baseStats.ResistElemental), TextRun(" Elemental Resistance"));

                results.AddSpan(baseStats.ResistDisruption != 0, ValuePercentageRun(baseStats.ResistDisruption), TextRun(" Disruption Protection"));
                results.AddSpan(baseStats.ResistBleed != 0, ValuePercentageRun(baseStats.ResistBleed), TextRun(" Bleeding Resistance"));
                results.AddSpan(baseStats.ResistStun != 0, ValuePercentageRun(baseStats.ResistStun), TextRun(" Stun Resistance"));
                results.AddSpan(baseStats.ResistSlow != 0, ValuePercentageRun(baseStats.ResistSlow), TextRun(" Slow Resistance"));
                results.AddSpan(baseStats.ResistKnockdown != 0, ValuePercentageRun(baseStats.ResistKnockdown), TextRun(" Knockdown Resistance"));

                results.AddSpan(baseStats.ShieldBlockChanceModifier != 0, ValuePercentageRun(baseStats.ShieldBlockChanceModifier), TextRun(" Shield Block Chance"));
                results.AddSpan(baseStats.ShieldDamageBlockModifier != 0, ValuePercentageRun(baseStats.ShieldDamageBlockModifier), TextRun(" Shield Damage Blocked"));

                results.AddSpan(baseStats.MaxResistPhysical != 0, ValuePercentageRun(baseStats.MaxResistPhysical), TextRun(" Maximum Physical Resistance"));
                results.AddSpan(baseStats.MaxResistPierce != 0, ValuePercentageRun(baseStats.MaxResistPierce), TextRun(" Maximum Pierce Resistance"));
                results.AddSpan(baseStats.MaxResistFire != 0, ValuePercentageRun(baseStats.MaxResistFire), TextRun(" Maximum Fire Resistance"));
                results.AddSpan(baseStats.MaxResistCold != 0, ValuePercentageRun(baseStats.MaxResistCold), TextRun(" Maximum Cold Resistance"));
                results.AddSpan(baseStats.MaxResistLightning != 0, ValuePercentageRun(baseStats.MaxResistLightning), TextRun(" Maximum Lightning Resistance"));
                results.AddSpan(baseStats.MaxResistPoison != 0, ValuePercentageRun(baseStats.MaxResistPoison), TextRun(" Maximum Poison & Acid Resistance"));
                results.AddSpan(baseStats.MaxResistVitality != 0, ValuePercentageRun(baseStats.MaxResistVitality), TextRun(" Maximum Vitality Resistance"));
                results.AddSpan(baseStats.MaxResistAether != 0, ValuePercentageRun(baseStats.MaxResistAether), TextRun(" Maximum Aether Resistance"));
                results.AddSpan(baseStats.MaxResistChaos != 0, ValuePercentageRun(baseStats.MaxResistChaos), TextRun(" Maximum Chaos Resistance"));
                results.AddSpan(baseStats.MaxResistAll != 0, ValuePercentageRun(baseStats.MaxResistAll), TextRun(" Maximum All Resistances"));
                results.AddSpan(baseStats.MaxResistStun != 0, ValuePercentageRun(baseStats.MaxResistStun), TextRun(" Stun Resistance"));

                results.AddSpan(baseStats.DamageAbsorptionPercent != 0, ValueNoPlusPercentageRun(baseStats.DamageAbsorptionPercent), TextRun(" Damage Absorption"));

                results.AddSpan(baseStats.SkillCooldownReduction != 0, ValuePercentageRun(baseStats.SkillCooldownReduction), TextRun(" Skill Cooldown Reduction"));
                results.AddSpan(baseStats.EnergyCostModifier != 0, ValuePercentageRun(-baseStats.EnergyCostModifier), TextRun(" Skill Energy Cost"));

                if (baseStats.SkillsWithQuantity is not null)
                    foreach (var sq in baseStats.SkillsWithQuantity)
                        results.AddSpan(true, ValueRun(sq.Quantity), TextRun($" to {sq.Skill.Name}"));

                return results;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}