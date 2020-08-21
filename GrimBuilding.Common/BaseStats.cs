

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace GrimBuilding.Common.Support
{
    [GeneratedCode("BaseStats.tt", null)]
    public class BaseStats
    {
                    /// <summary>  </summary>
            public int LevelRequirement { get; set; }
                    /// <summary>  </summary>
            public double PhysiqueRequirement { get; set; }
                    /// <summary>  </summary>
            public double CunningRequirement { get; set; }
                    /// <summary>  </summary>
            public double SpiritRequirement { get; set; }
                    /// <summary>  </summary>
            public double Life { get; set; }
                    /// <summary>  </summary>
            public double LifeModifier { get; set; }
                    /// <summary>  </summary>
            public double LifeRegeneration { get; set; }
                    /// <summary>  </summary>
            public double LifeRegenerationModifier { get; set; }
                    /// <summary>  </summary>
            public double Energy { get; set; }
                    /// <summary>  </summary>
            public double EnergyModifier { get; set; }
                    /// <summary>  </summary>
            public double EnergyRegeneration { get; set; }
                    /// <summary>  </summary>
            public double EnergyRegenerationModifier { get; set; }
                    /// <summary>  </summary>
            public double Physique { get; set; }
                    /// <summary>  </summary>
            public double PhysiqueModifier { get; set; }
                    /// <summary>  </summary>
            public double Cunning { get; set; }
                    /// <summary>  </summary>
            public double CunningModifier { get; set; }
                    /// <summary>  </summary>
            public double Spirit { get; set; }
                    /// <summary>  </summary>
            public double SpiritModifier { get; set; }
                    /// <summary>  </summary>
            public double Armor { get; set; }
                    /// <summary>  </summary>
            public double ArmorChance { get; set; }
                    /// <summary>  </summary>
            public double ArmorModifier { get; set; }
                    /// <summary>  </summary>
            public double ArmorModifierChance { get; set; }
                    /// <summary> Only defensive absorption stat in use, maps to armor absorption %. </summary>
            public double ArmorAbsorptionModifier { get; set; }
                    /// <summary>  </summary>
            public double ResistPhysical { get; set; }
                    /// <summary>  </summary>
            public double ResistPierce { get; set; }
                    /// <summary>  </summary>
            public double ResistFire { get; set; }
                    /// <summary>  </summary>
            public double ResistCold { get; set; }
                    /// <summary>  </summary>
            public double ResistLightning { get; set; }
                    /// <summary>  </summary>
            public double ResistPoison { get; set; }
                    /// <summary>  </summary>
            public double ResistVitality { get; set; }
                    /// <summary>  </summary>
            public double ResistAether { get; set; }
                    /// <summary>  </summary>
            public double ResistChaos { get; set; }
                    /// <summary>  </summary>
            public double ResistElemental { get; set; }
                    /// <summary>  </summary>
            public double ResistDisruption { get; set; }
                    /// <summary>  </summary>
            public double ResistBleed { get; set; }
                    /// <summary>  </summary>
            public double ResistStun { get; set; }
                    /// <summary>  </summary>
            public double ResistSlow { get; set; }
                    /// <summary>  </summary>
            public double ResistKnockdown { get; set; }
                    /// <summary> Increases damage done with Aether by this %. </summary>
            public double OffensiveAetherModifier { get; set; }
                    /// <summary> Increases damage done with Chaos by this %. </summary>
            public double OffensiveChaosModifier { get; set; }
                    /// <summary> Increases damage done with Cold by this %. </summary>
            public double OffensiveColdModifier { get; set; }
                    /// <summary> Increases damage done with Fire by this %. </summary>
            public double OffensiveFireModifier { get; set; }
                    /// <summary> Increases damage done with Knockdown by this %. </summary>
            public double OffensiveKnockdownModifier { get; set; }
                    /// <summary> Increases damage done with Vitality by this %. </summary>
            public double OffensiveVitalityModifier { get; set; }
                    /// <summary> Increases damage done with Lightning by this %. </summary>
            public double OffensiveLightningModifier { get; set; }
                    /// <summary> Increases damage done with Physical by this %. </summary>
            public double OffensivePhysicalModifier { get; set; }
                    /// <summary> Increases damage done with Pierce by this %. </summary>
            public double OffensivePierceModifier { get; set; }
                    /// <summary> Increases damage done with Poison by this %. </summary>
            public double OffensivePoisonModifier { get; set; }
                    /// <summary> Increases damage done with BleedDot by this %. </summary>
            public double OffensiveBleedDotModifier { get; set; }
                    /// <summary> Deals the BleedDot damage over this duration. </summary>
            public double OffensiveBleedDotDuration { get; set; }
                    /// <summary> Deals this BleedDot damage per tick over its duration. </summary>
            public double OffensiveBleedDotTickDamage { get; set; }
                    /// <summary> Increases damage done with ColdDot by this %. </summary>
            public double OffensiveColdDotModifier { get; set; }
                    /// <summary> Deals the ColdDot damage over this duration. </summary>
            public double OffensiveColdDotDuration { get; set; }
                    /// <summary> Deals this ColdDot damage per tick over its duration. </summary>
            public double OffensiveColdDotTickDamage { get; set; }
                    /// <summary> Increases damage done with FireDot by this %. </summary>
            public double OffensiveFireDotModifier { get; set; }
                    /// <summary> Deals the FireDot damage over this duration. </summary>
            public double OffensiveFireDotDuration { get; set; }
                    /// <summary> Deals this FireDot damage per tick over its duration. </summary>
            public double OffensiveFireDotTickDamage { get; set; }
                    /// <summary> Increases damage done with VitalityDot by this %. </summary>
            public double OffensiveVitalityDotModifier { get; set; }
                    /// <summary> Deals the VitalityDot damage over this duration. </summary>
            public double OffensiveVitalityDotDuration { get; set; }
                    /// <summary> Deals this VitalityDot damage per tick over its duration. </summary>
            public double OffensiveVitalityDotTickDamage { get; set; }
                    /// <summary> Increases damage done with LightningDot by this %. </summary>
            public double OffensiveLightningDotModifier { get; set; }
                    /// <summary> Deals the LightningDot damage over this duration. </summary>
            public double OffensiveLightningDotDuration { get; set; }
                    /// <summary> Deals this LightningDot damage per tick over its duration. </summary>
            public double OffensiveLightningDotTickDamage { get; set; }
                    /// <summary> Increases damage done with PhysicalDot by this %. </summary>
            public double OffensivePhysicalDotModifier { get; set; }
                    /// <summary> Deals the PhysicalDot damage over this duration. </summary>
            public double OffensivePhysicalDotDuration { get; set; }
                    /// <summary> Deals this PhysicalDot damage per tick over its duration. </summary>
            public double OffensivePhysicalDotTickDamage { get; set; }
                    /// <summary> Increases damage done with PoisonDot by this %. </summary>
            public double OffensivePoisonDotModifier { get; set; }
                    /// <summary> Deals the PoisonDot damage over this duration. </summary>
            public double OffensivePoisonDotDuration { get; set; }
                    /// <summary> Deals this PoisonDot damage per tick over its duration. </summary>
            public double OffensivePoisonDotTickDamage { get; set; }
                    /// <summary> Increases damage done with Stun by this %. </summary>
            public double OffensiveStunModifier { get; set; }
                    /// <summary>  </summary>
            public double OffensiveAbility { get; set; }
                    /// <summary>  </summary>
            public double OffensiveAbilityModifier { get; set; }
                    /// <summary>  </summary>
            public double DefensiveAbility { get; set; }
                    /// <summary>  </summary>
            public double DefensiveAbilityModifier { get; set; }
                    /// <summary>  </summary>
            public double AttackSpeedModifier { get; set; }
                    /// <summary>  </summary>
            public double CastSpeedModifier { get; set; }
                    /// <summary>  </summary>
            public double RunSpeedModifier { get; set; }
                    /// <summary>  </summary>
            public double AttributeScalePercent { get; set; }
        
        public List<PlayerSkillAugmentWithQuantity> SkillsWithQuantity { get; set; }

        public void AddFrom(BaseStats other)
        {
                            LevelRequirement += other.LevelRequirement;
                            PhysiqueRequirement += other.PhysiqueRequirement;
                            CunningRequirement += other.CunningRequirement;
                            SpiritRequirement += other.SpiritRequirement;
                            Life += other.Life;
                            LifeModifier += other.LifeModifier;
                            LifeRegeneration += other.LifeRegeneration;
                            LifeRegenerationModifier += other.LifeRegenerationModifier;
                            Energy += other.Energy;
                            EnergyModifier += other.EnergyModifier;
                            EnergyRegeneration += other.EnergyRegeneration;
                            EnergyRegenerationModifier += other.EnergyRegenerationModifier;
                            Physique += other.Physique;
                            PhysiqueModifier += other.PhysiqueModifier;
                            Cunning += other.Cunning;
                            CunningModifier += other.CunningModifier;
                            Spirit += other.Spirit;
                            SpiritModifier += other.SpiritModifier;
                            Armor += other.Armor;
                            ArmorChance += other.ArmorChance;
                            ArmorModifier += other.ArmorModifier;
                            ArmorModifierChance += other.ArmorModifierChance;
                            ArmorAbsorptionModifier += other.ArmorAbsorptionModifier;
                            ResistPhysical += other.ResistPhysical;
                            ResistPierce += other.ResistPierce;
                            ResistFire += other.ResistFire;
                            ResistCold += other.ResistCold;
                            ResistLightning += other.ResistLightning;
                            ResistPoison += other.ResistPoison;
                            ResistVitality += other.ResistVitality;
                            ResistAether += other.ResistAether;
                            ResistChaos += other.ResistChaos;
                            ResistElemental += other.ResistElemental;
                            ResistDisruption += other.ResistDisruption;
                            ResistBleed += other.ResistBleed;
                            ResistStun += other.ResistStun;
                            ResistSlow += other.ResistSlow;
                            ResistKnockdown += other.ResistKnockdown;
                            OffensiveAetherModifier += other.OffensiveAetherModifier;
                            OffensiveChaosModifier += other.OffensiveChaosModifier;
                            OffensiveColdModifier += other.OffensiveColdModifier;
                            OffensiveFireModifier += other.OffensiveFireModifier;
                            OffensiveKnockdownModifier += other.OffensiveKnockdownModifier;
                            OffensiveVitalityModifier += other.OffensiveVitalityModifier;
                            OffensiveLightningModifier += other.OffensiveLightningModifier;
                            OffensivePhysicalModifier += other.OffensivePhysicalModifier;
                            OffensivePierceModifier += other.OffensivePierceModifier;
                            OffensivePoisonModifier += other.OffensivePoisonModifier;
                            OffensiveBleedDotModifier += other.OffensiveBleedDotModifier;
                            OffensiveBleedDotDuration += other.OffensiveBleedDotDuration;
                            OffensiveBleedDotTickDamage += other.OffensiveBleedDotTickDamage;
                            OffensiveColdDotModifier += other.OffensiveColdDotModifier;
                            OffensiveColdDotDuration += other.OffensiveColdDotDuration;
                            OffensiveColdDotTickDamage += other.OffensiveColdDotTickDamage;
                            OffensiveFireDotModifier += other.OffensiveFireDotModifier;
                            OffensiveFireDotDuration += other.OffensiveFireDotDuration;
                            OffensiveFireDotTickDamage += other.OffensiveFireDotTickDamage;
                            OffensiveVitalityDotModifier += other.OffensiveVitalityDotModifier;
                            OffensiveVitalityDotDuration += other.OffensiveVitalityDotDuration;
                            OffensiveVitalityDotTickDamage += other.OffensiveVitalityDotTickDamage;
                            OffensiveLightningDotModifier += other.OffensiveLightningDotModifier;
                            OffensiveLightningDotDuration += other.OffensiveLightningDotDuration;
                            OffensiveLightningDotTickDamage += other.OffensiveLightningDotTickDamage;
                            OffensivePhysicalDotModifier += other.OffensivePhysicalDotModifier;
                            OffensivePhysicalDotDuration += other.OffensivePhysicalDotDuration;
                            OffensivePhysicalDotTickDamage += other.OffensivePhysicalDotTickDamage;
                            OffensivePoisonDotModifier += other.OffensivePoisonDotModifier;
                            OffensivePoisonDotDuration += other.OffensivePoisonDotDuration;
                            OffensivePoisonDotTickDamage += other.OffensivePoisonDotTickDamage;
                            OffensiveStunModifier += other.OffensiveStunModifier;
                            OffensiveAbility += other.OffensiveAbility;
                            OffensiveAbilityModifier += other.OffensiveAbilityModifier;
                            DefensiveAbility += other.DefensiveAbility;
                            DefensiveAbilityModifier += other.DefensiveAbilityModifier;
                            AttackSpeedModifier += other.AttackSpeedModifier;
                            CastSpeedModifier += other.CastSpeedModifier;
                            RunSpeedModifier += other.RunSpeedModifier;
                            AttributeScalePercent += other.AttributeScalePercent;
            
            // add the skills as well
            var otherList = other.SkillsWithQuantity ??= new();
            foreach(var skillWithQuantity in SkillsWithQuantity ??= new())
            {
                var otherSkillWithQuantity = otherList.FirstOrDefault(sq => sq.Skill == skillWithQuantity.Skill);
                if(otherSkillWithQuantity is null)
                    otherList.Add(otherSkillWithQuantity = new PlayerSkillAugmentWithQuantity { Skill = skillWithQuantity.Skill});
                otherSkillWithQuantity.Quantity += skillWithQuantity.Quantity;
            }
        }
    }

    public class PlayerSkillAugmentWithQuantity
    {
        [BsonRef]
        public PlayerSkill Skill { get; set; }
        public int Quantity { get; set; }
    }
}
