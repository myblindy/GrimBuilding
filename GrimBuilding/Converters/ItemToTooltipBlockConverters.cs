using GrimBuilding.Common.Support;
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
        static Run ValueRun(double value) => new Run(value > 0 ? $"+{value}" : value.ToString()) { Foreground = ItemTypeTooltipLineBrush };
        static Run ValuePercentageRun(double value) => new Run(value > 0 ? $"+{value}%" : value.ToString() + "%") { Foreground = ItemTypeTooltipLineBrush };

        static readonly Brush ItemTextTooltipLineBrush = (Brush)Application.Current.Resources["ItemTextTooltipLineBrush"];
        static Run TextRun(string value) => new Run(value) { Foreground = ItemTextTooltipLineBrush };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Item item)
            {
                var results = new List<Span>();

                results.AddSpan(item.Physique != 0, ValueRun(item.Physique), TextRun(" Physique"));
                results.AddSpan(item.PhysiqueModifier != 0, ValuePercentageRun(item.PhysiqueModifier), TextRun(" Physique"));
                results.AddSpan(item.Cunning != 0, ValueRun(item.Cunning), TextRun(" Cunning"));
                results.AddSpan(item.CunningModifier != 0, ValuePercentageRun(item.CunningModifier), TextRun(" Cunning"));
                results.AddSpan(item.Spirit != 0, ValueRun(item.Spirit), TextRun(" Spirit"));
                results.AddSpan(item.SpiritModifier != 0, ValuePercentageRun(item.SpiritModifier), TextRun(" Spirit"));
                results.AddSpan(item.Life != 0, ValueRun(item.Life), TextRun(" Health"));
                results.AddSpan(item.LifeModifier != 0, ValuePercentageRun(item.LifeModifier), TextRun(" Health"));

                results.AddSpan(item.OffensiveAbility != 0, ValueRun(item.OffensiveAbility), TextRun(" Offensive Ability"));
                results.AddSpan(item.OffensiveAbilityModifier != 0, ValuePercentageRun(item.OffensiveAbilityModifier), TextRun(" Offensive Abiltiy"));
                results.AddSpan(item.DefensiveAbility != 0, ValueRun(item.DefensiveAbility), TextRun(" Defensive Ability"));
                results.AddSpan(item.DefensiveAbilityModifier != 0, ValuePercentageRun(item.DefensiveAbilityModifier), TextRun(" Defensive Ability"));
                results.AddSpan(item.RunSpeedModifier != 0, ValuePercentageRun(item.RunSpeedModifier), TextRun(" Movement Speed"));

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

                results.AddSpan(item.LifeRegeneration != 0, ValueRun(item.LifeRegeneration), TextRun(" Health Regenerated per Second"));
                results.AddSpan(item.LifeRegenerationModifier != 0, TextRun("Increases Health Regeneration by "), ValuePercentageRun(item.LifeRegenerationModifier));

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