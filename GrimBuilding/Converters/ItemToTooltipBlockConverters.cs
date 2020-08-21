﻿using GrimBuilding.Common.Support;
using LiteDB;
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

    class ItemToRegularStatsTooltipBlockConverter : IValueConverter
    {
        static readonly Brush ItemTypeTooltipLineBrush = (Brush)Application.Current.Resources["ItemTypeTooltipLineBrush"];
        static Run ValueRun(double value) => new Run(value > 0 ? $"+{Math.Round(value)}" : Math.Round(value).ToString()) { Foreground = ItemTypeTooltipLineBrush };
        static Run ValueNoPlusRun(double value) => new Run(Math.Round(value).ToString()) { Foreground = ItemTypeTooltipLineBrush };
        static Run ValueNoPlusRangeRun(double v1, double v2) => v2 == 0
            ? new Run(Math.Round(v1).ToString()) { Foreground = ItemTypeTooltipLineBrush }
            : new Run($"{Math.Round(v1)}-{Math.Round(v2)}") { Foreground = ItemTypeTooltipLineBrush };
        static Run ValuePercentageRun(double value) => new Run(value > 0 ? $"+{Math.Round(value)}%" : Math.Round(value).ToString() + "%") { Foreground = ItemTypeTooltipLineBrush };

        static readonly Brush ItemTextTooltipLineBrush = (Brush)Application.Current.Resources["ItemTextTooltipLineBrush"];
        static Run TextRun(string value) => new Run(value) { Foreground = ItemTextTooltipLineBrush };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Item item)
            {
                var results = new List<Span>();

                results.AddSpan(item.OffensiveBleedDotDuration != 0, ValueRun(Math.Round(item.OffensiveBleedDotTickDamage * item.OffensiveBleedDotDuration * (1 + item.AttributeScalePercent))),
                    TextRun(" Bleeding Damage over "), ValueNoPlusRun(item.OffensiveBleedDotDuration), TextRun(" Seconds"));

                results.AddSpan(item.OffensivePhysicalBaseMin != 0, ValueNoPlusRangeRun(item.OffensivePhysicalBaseMin, item.OffensivePhysicalBaseMax), TextRun(" Physical Damage"));
                results.AddSpan(item.OffensivePierceBaseMin != 0, ValueNoPlusRangeRun(item.OffensivePierceBaseMin, item.OffensivePierceBaseMax), TextRun(" Pierce Damage"));
                results.AddSpan(item.OffensiveFireBaseMin != 0, ValueNoPlusRangeRun(item.OffensiveFireBaseMin, item.OffensiveFireBaseMax), TextRun(" Fire Damage"));
                results.AddSpan(item.OffensiveColdBaseMin != 0, ValueNoPlusRangeRun(item.OffensiveColdBaseMin, item.OffensiveColdBaseMax), TextRun(" Cold Damage"));
                results.AddSpan(item.OffensiveLightningBaseMin != 0, ValueNoPlusRangeRun(item.OffensiveLightningBaseMin, item.OffensiveLightningBaseMax), TextRun(" Lightning Damage"));
                results.AddSpan(item.OffensivePoisonBaseMin != 0, ValueNoPlusRangeRun(item.OffensivePoisonBaseMin, item.OffensivePoisonBaseMax), TextRun(" Poison Damage"));
                results.AddSpan(item.OffensiveVitalityBaseMin != 0, ValueNoPlusRangeRun(item.OffensiveVitalityBaseMin, item.OffensiveVitalityBaseMax), TextRun(" Vitality Damage"));
                results.AddSpan(item.OffensiveAetherBaseMin != 0, ValueNoPlusRangeRun(item.OffensiveAetherBaseMin, item.OffensiveAetherBaseMax), TextRun(" Aether Damage"));
                results.AddSpan(item.OffensiveChaosBaseMin != 0, ValueNoPlusRangeRun(item.OffensiveChaosBaseMin, item.OffensiveChaosBaseMax), TextRun(" Chaos Damage"));

                results.AddSpan(item.OffensivePhysicalModifier != 0, ValuePercentageRun(item.OffensivePhysicalModifier), TextRun(" Physical Damage"));
                results.AddSpan(item.OffensivePierceModifier != 0, ValuePercentageRun(item.OffensivePierceModifier), TextRun(" Pierce Damage"));
                results.AddSpan(item.OffensiveFireModifier != 0, ValuePercentageRun(item.OffensiveFireModifier), TextRun(" Fire Damage"));
                results.AddSpan(item.OffensiveColdModifier != 0, ValuePercentageRun(item.OffensiveColdModifier), TextRun(" Cold Damage"));
                results.AddSpan(item.OffensiveLightningModifier != 0, ValuePercentageRun(item.OffensiveLightningModifier), TextRun(" Lightning Damage"));
                results.AddSpan(item.OffensivePoisonModifier != 0, ValuePercentageRun(item.OffensivePoisonModifier), TextRun(" Poison Damage"));
                results.AddSpan(item.OffensiveVitalityModifier != 0, ValuePercentageRun(item.OffensiveVitalityModifier), TextRun(" Vitality Damage"));
                results.AddSpan(item.OffensiveAetherModifier != 0, ValuePercentageRun(item.OffensiveAetherModifier), TextRun(" Aether Damage"));
                results.AddSpan(item.OffensiveChaosModifier != 0, ValuePercentageRun(item.OffensiveChaosModifier), TextRun(" Chaos Damage"));
                results.AddSpan(item.OffensiveKnockdownModifier != 0, ValuePercentageRun(item.OffensiveKnockdownModifier), TextRun(" Knockdown Damage"));
                results.AddSpan(item.OffensiveBleedDotModifier != 0, ValuePercentageRun(item.OffensiveBleedDotModifier), TextRun(" Bleeding Damage"));
                results.AddSpan(item.OffensiveColdDotModifier != 0, ValuePercentageRun(item.OffensiveColdDotModifier), TextRun(" ColdDot Damage"));
                results.AddSpan(item.OffensiveFireDotModifier != 0, ValuePercentageRun(item.OffensiveFireDotModifier), TextRun(" Burn Damage"));
                results.AddSpan(item.OffensiveVitalityDotModifier != 0, ValuePercentageRun(item.OffensiveVitalityDotModifier), TextRun(" Vitality Decay Damage"));
                results.AddSpan(item.OffensiveLightningDotModifier != 0, ValuePercentageRun(item.OffensiveLightningDotModifier), TextRun(" Electrocute Damage"));
                results.AddSpan(item.OffensivePhysicalDotModifier != 0, ValuePercentageRun(item.OffensivePhysicalDotModifier), TextRun(" Internal Trauma Damage"));
                results.AddSpan(item.OffensivePoisonDotModifier != 0, ValuePercentageRun(item.OffensivePoisonDotModifier), TextRun(" PoisonDot Damage"));
                results.AddSpan(item.OffensiveStunModifier != 0, ValuePercentageRun(item.OffensiveStunModifier), TextRun(" Stun Damage"));

                results.AddSpan(item.Physique != 0, ValueRun(item.Physique), TextRun(" Physique"));
                results.AddSpan(item.PhysiqueModifier != 0, ValuePercentageRun(item.PhysiqueModifier), TextRun(" Physique"));
                results.AddSpan(item.Cunning != 0, ValueRun(item.Cunning), TextRun(" Cunning"));
                results.AddSpan(item.CunningModifier != 0, ValuePercentageRun(item.CunningModifier), TextRun(" Cunning"));
                results.AddSpan(item.Spirit != 0, ValueRun(item.Spirit), TextRun(" Spirit"));
                results.AddSpan(item.SpiritModifier != 0, ValuePercentageRun(item.SpiritModifier), TextRun(" Spirit"));

                results.AddSpan(item.OffensiveAbility != 0, ValueRun(item.OffensiveAbility), TextRun(" Offensive Ability"));
                results.AddSpan(item.OffensiveAbilityModifier != 0, ValuePercentageRun(item.OffensiveAbilityModifier), TextRun(" Offensive Abiltiy"));
                results.AddSpan(item.DefensiveAbility != 0, ValueRun(item.DefensiveAbility), TextRun(" Defensive Ability"));
                results.AddSpan(item.DefensiveAbilityModifier != 0, ValuePercentageRun(item.DefensiveAbilityModifier), TextRun(" Defensive Ability"));
                results.AddSpan(item.RunSpeedModifier != 0, ValuePercentageRun(item.RunSpeedModifier), TextRun(" Movement Speed"));

                results.AddSpan(item.Life != 0, ValueRun(item.Life), TextRun(" Health"));
                results.AddSpan(item.LifeModifier != 0, ValuePercentageRun(item.LifeModifier), TextRun(" Health"));
                results.AddSpan(item.LifeRegeneration != 0, ValueRun(item.LifeRegeneration), TextRun(" Health Regenerated per Second"));
                results.AddSpan(item.LifeRegenerationModifier != 0, TextRun("Increases Health Regeneration by "), ValuePercentageRun(item.LifeRegenerationModifier));
                results.AddSpan(item.Energy != 0, ValueRun(item.Energy), TextRun(" Energy"));
                results.AddSpan(item.EnergyModifier != 0, ValuePercentageRun(item.EnergyModifier), TextRun(" Energy"));
                results.AddSpan(item.EnergyRegeneration != 0, ValueRun(item.EnergyRegeneration), TextRun(" Energy Regenerated per Second"));
                results.AddSpan(item.EnergyRegenerationModifier != 0, TextRun("Increases Energy Regeneration by "), ValuePercentageRun(item.EnergyRegenerationModifier));

                results.AddSpan(item.AttackSpeedModifier != 0, ValuePercentageRun(item.AttackSpeedModifier), TextRun(" Attack Speed"));
                results.AddSpan(item.CastSpeedModifier != 0, ValuePercentageRun(item.CastSpeedModifier), TextRun(" Casting Speed"));

                results.AddSpan(item.ResistPhysical != 0, ValuePercentageRun(item.ResistPhysical), TextRun(" Physical Resistance"));
                results.AddSpan(item.ResistPierce != 0, ValuePercentageRun(item.ResistPierce), TextRun(" Pierce Resistance"));
                results.AddSpan(item.ResistFire != 0, ValuePercentageRun(item.ResistFire), TextRun(" Fire Resistance"));
                results.AddSpan(item.ResistCold != 0, ValuePercentageRun(item.ResistCold), TextRun(" Cold Resistance"));
                results.AddSpan(item.ResistLightning != 0, ValuePercentageRun(item.ResistLightning), TextRun(" Lightning Resistance"));
                results.AddSpan(item.ResistPoison != 0, ValuePercentageRun(item.ResistPoison), TextRun(" Poison & Acid Resistance"));
                results.AddSpan(item.ResistVitality != 0, ValuePercentageRun(item.ResistVitality), TextRun(" Vitality Resistance"));
                results.AddSpan(item.ResistAether != 0, ValuePercentageRun(item.ResistAether), TextRun(" Aether Resistance"));
                results.AddSpan(item.ResistChaos != 0, ValuePercentageRun(item.ResistChaos), TextRun(" Chaos Resistance"));
                results.AddSpan(item.ResistElemental != 0, ValuePercentageRun(item.ResistElemental), TextRun(" Elemental Resistance"));

                results.AddSpan(item.ResistDisruption != 0, ValuePercentageRun(item.ResistDisruption), TextRun(" Disruption Protection"));
                results.AddSpan(item.ResistBleed != 0, ValuePercentageRun(item.ResistBleed), TextRun(" Bleeding Resistance"));
                results.AddSpan(item.ResistStun != 0, ValuePercentageRun(item.ResistStun), TextRun(" Stun Resistance"));
                results.AddSpan(item.ResistSlow != 0, ValuePercentageRun(item.ResistSlow), TextRun(" Slow Resistance"));
                results.AddSpan(item.ResistKnockdown != 0, ValuePercentageRun(item.ResistKnockdown), TextRun(" Knockdown Resistance"));

                results.AddSpan(item.ShieldBlockChanceModifier != 0, ValuePercentageRun(item.ShieldBlockChanceModifier), TextRun(" Shield Block Chance"));
                results.AddSpan(item.ShieldDamageBlockModifier != 0, ValuePercentageRun(item.ShieldDamageBlockModifier), TextRun(" Shield Damage Blocked"));

                results.AddSpan(item.MaxResistPhysical != 0, ValuePercentageRun(item.MaxResistPhysical), TextRun(" Maximum Physical Resistance"));
                results.AddSpan(item.MaxResistPierce != 0, ValuePercentageRun(item.MaxResistPierce), TextRun(" Maximum Pierce Resistance"));
                results.AddSpan(item.MaxResistFire != 0, ValuePercentageRun(item.MaxResistFire), TextRun(" Maximum Fire Resistance"));
                results.AddSpan(item.MaxResistCold != 0, ValuePercentageRun(item.MaxResistCold), TextRun(" Maximum Cold Resistance"));
                results.AddSpan(item.MaxResistLightning != 0, ValuePercentageRun(item.MaxResistLightning), TextRun(" Maximum Lightning Resistance"));
                results.AddSpan(item.MaxResistPoison != 0, ValuePercentageRun(item.MaxResistPoison), TextRun(" Maximum Poison & Acid Resistance"));
                results.AddSpan(item.MaxResistVitality != 0, ValuePercentageRun(item.MaxResistVitality), TextRun(" Maximum Vitality Resistance"));
                results.AddSpan(item.MaxResistAether != 0, ValuePercentageRun(item.MaxResistAether), TextRun(" Maximum Aether Resistance"));
                results.AddSpan(item.MaxResistChaos != 0, ValuePercentageRun(item.MaxResistChaos), TextRun(" Maximum Chaos Resistance"));
                results.AddSpan(item.MaxResistAll != 0, ValuePercentageRun(item.MaxResistAll), TextRun(" Maximum All Resistances"));
                results.AddSpan(item.MaxResistStun != 0, ValuePercentageRun(item.MaxResistStun), TextRun(" Stun Resistance"));

                results.AddSpan(item.SkillCooldownReduction != 0, ValuePercentageRun(item.SkillCooldownReduction), TextRun(" Skill Cooldown Reduction"));

                foreach (var sq in item.SkillsWithQuantity)
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