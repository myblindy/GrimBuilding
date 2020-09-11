

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
            public double RestoreLifePercent { get; set; }
                    /// <summary> % Health at which this skill triggers. </summary>
            public double LifeMonitorPercent { get; set; }
                    /// <summary>  </summary>
            public double SkillDuration { get; set; }
                    /// <summary>  </summary>
            public double SkillCooldown { get; set; }
                    /// <summary>  </summary>
            public double Energy { get; set; }
                    /// <summary>  </summary>
            public double EnergyModifier { get; set; }
                    /// <summary>  </summary>
            public double EnergyRegeneration { get; set; }
                    /// <summary>  </summary>
            public double EnergyRegenerationModifier { get; set; }
                    /// <summary>  </summary>
            public double EnergyCost { get; set; }
                    /// <summary>  </summary>
            public double EnergyCostModifier { get; set; }
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
                    /// <summary> Increases resistance against Physical damage. </summary>
            public double ResistPhysical { get; set; }
                    /// <summary> Deals the maximum resistance against Physical damage. </summary>
            public double MaxResistPhysical { get; set; }
                    /// <summary> Increases resistance against Pierce damage. </summary>
            public double ResistPierce { get; set; }
                    /// <summary> Deals the maximum resistance against Pierce damage. </summary>
            public double MaxResistPierce { get; set; }
                    /// <summary> Increases resistance against Fire damage. </summary>
            public double ResistFire { get; set; }
                    /// <summary> Deals the maximum resistance against Fire damage. </summary>
            public double MaxResistFire { get; set; }
                    /// <summary> Increases resistance against Cold damage. </summary>
            public double ResistCold { get; set; }
                    /// <summary> Deals the maximum resistance against Cold damage. </summary>
            public double MaxResistCold { get; set; }
                    /// <summary> Increases resistance against Lightning damage. </summary>
            public double ResistLightning { get; set; }
                    /// <summary> Deals the maximum resistance against Lightning damage. </summary>
            public double MaxResistLightning { get; set; }
                    /// <summary> Increases resistance against Poison damage. </summary>
            public double ResistPoison { get; set; }
                    /// <summary> Deals the maximum resistance against Poison damage. </summary>
            public double MaxResistPoison { get; set; }
                    /// <summary> Increases resistance against Vitality damage. </summary>
            public double ResistVitality { get; set; }
                    /// <summary> Deals the maximum resistance against Vitality damage. </summary>
            public double MaxResistVitality { get; set; }
                    /// <summary> Increases resistance against Aether damage. </summary>
            public double ResistAether { get; set; }
                    /// <summary> Deals the maximum resistance against Aether damage. </summary>
            public double MaxResistAether { get; set; }
                    /// <summary> Increases resistance against Chaos damage. </summary>
            public double ResistChaos { get; set; }
                    /// <summary> Deals the maximum resistance against Chaos damage. </summary>
            public double MaxResistChaos { get; set; }
                    /// <summary> Increases resistance against Stun damage. </summary>
            public double ResistStun { get; set; }
                    /// <summary> Deals the maximum resistance against Stun damage. </summary>
            public double MaxResistStun { get; set; }
                    /// <summary>  </summary>
            public double MaxResistAll { get; set; }
                    /// <summary>  </summary>
            public double ResistElemental { get; set; }
                    /// <summary>  </summary>
            public double ResistDisruption { get; set; }
                    /// <summary>  </summary>
            public double ResistBleed { get; set; }
                    /// <summary>  </summary>
            public double ResistSlow { get; set; }
                    /// <summary>  </summary>
            public double ResistKnockdown { get; set; }
                    /// <summary>  </summary>
            public double DamageAbsorptionPercent { get; set; }
                    /// <summary> Increases damage done with Aether by this %. </summary>
            public double OffensiveAetherModifier { get; set; }
                    /// <summary> Minimum damage dealt as Aether. </summary>
            public double OffensiveAetherBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Aether. If 0, it's not a range, use only <see cref="OffensiveAetherBaseMin"/>. </summary>
            public double OffensiveAetherBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Aether. </summary>
            public double OffensiveAetherBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Aether. If 0, it's not a range, use only <see cref="OffensiveAetherBonusMin"/>. </summary>
            public double OffensiveAetherBonusMax { get; set; }
                    /// <summary> Increases damage done with Chaos by this %. </summary>
            public double OffensiveChaosModifier { get; set; }
                    /// <summary> Minimum damage dealt as Chaos. </summary>
            public double OffensiveChaosBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Chaos. If 0, it's not a range, use only <see cref="OffensiveChaosBaseMin"/>. </summary>
            public double OffensiveChaosBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Chaos. </summary>
            public double OffensiveChaosBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Chaos. If 0, it's not a range, use only <see cref="OffensiveChaosBonusMin"/>. </summary>
            public double OffensiveChaosBonusMax { get; set; }
                    /// <summary> Increases damage done with Cold by this %. </summary>
            public double OffensiveColdModifier { get; set; }
                    /// <summary> Minimum damage dealt as Cold. </summary>
            public double OffensiveColdBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Cold. If 0, it's not a range, use only <see cref="OffensiveColdBaseMin"/>. </summary>
            public double OffensiveColdBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Cold. </summary>
            public double OffensiveColdBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Cold. If 0, it's not a range, use only <see cref="OffensiveColdBonusMin"/>. </summary>
            public double OffensiveColdBonusMax { get; set; }
                    /// <summary> Increases damage done with Fire by this %. </summary>
            public double OffensiveFireModifier { get; set; }
                    /// <summary> Minimum damage dealt as Fire. </summary>
            public double OffensiveFireBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Fire. If 0, it's not a range, use only <see cref="OffensiveFireBaseMin"/>. </summary>
            public double OffensiveFireBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Fire. </summary>
            public double OffensiveFireBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Fire. If 0, it's not a range, use only <see cref="OffensiveFireBonusMin"/>. </summary>
            public double OffensiveFireBonusMax { get; set; }
                    /// <summary> Increases damage done with Elemental by this %. </summary>
            public double OffensiveElementalModifier { get; set; }
                    /// <summary> Minimum damage dealt as Elemental. </summary>
            public double OffensiveElementalBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Elemental. If 0, it's not a range, use only <see cref="OffensiveElementalBaseMin"/>. </summary>
            public double OffensiveElementalBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Elemental. </summary>
            public double OffensiveElementalBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Elemental. If 0, it's not a range, use only <see cref="OffensiveElementalBonusMin"/>. </summary>
            public double OffensiveElementalBonusMax { get; set; }
                    /// <summary> Increases damage done with Knockdown by this %. </summary>
            public double OffensiveKnockdownModifier { get; set; }
                    /// <summary> Minimum damage dealt as Knockdown. </summary>
            public double OffensiveKnockdownBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Knockdown. If 0, it's not a range, use only <see cref="OffensiveKnockdownBaseMin"/>. </summary>
            public double OffensiveKnockdownBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Knockdown. </summary>
            public double OffensiveKnockdownBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Knockdown. If 0, it's not a range, use only <see cref="OffensiveKnockdownBonusMin"/>. </summary>
            public double OffensiveKnockdownBonusMax { get; set; }
                    /// <summary> Increases damage done with Vitality by this %. </summary>
            public double OffensiveVitalityModifier { get; set; }
                    /// <summary> Minimum damage dealt as Vitality. </summary>
            public double OffensiveVitalityBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Vitality. If 0, it's not a range, use only <see cref="OffensiveVitalityBaseMin"/>. </summary>
            public double OffensiveVitalityBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Vitality. </summary>
            public double OffensiveVitalityBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Vitality. If 0, it's not a range, use only <see cref="OffensiveVitalityBonusMin"/>. </summary>
            public double OffensiveVitalityBonusMax { get; set; }
                    /// <summary> Increases damage done with Lightning by this %. </summary>
            public double OffensiveLightningModifier { get; set; }
                    /// <summary> Minimum damage dealt as Lightning. </summary>
            public double OffensiveLightningBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Lightning. If 0, it's not a range, use only <see cref="OffensiveLightningBaseMin"/>. </summary>
            public double OffensiveLightningBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Lightning. </summary>
            public double OffensiveLightningBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Lightning. If 0, it's not a range, use only <see cref="OffensiveLightningBonusMin"/>. </summary>
            public double OffensiveLightningBonusMax { get; set; }
                    /// <summary> Increases damage done with Physical by this %. </summary>
            public double OffensivePhysicalModifier { get; set; }
                    /// <summary> Minimum damage dealt as Physical. </summary>
            public double OffensivePhysicalBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Physical. If 0, it's not a range, use only <see cref="OffensivePhysicalBaseMin"/>. </summary>
            public double OffensivePhysicalBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Physical. </summary>
            public double OffensivePhysicalBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Physical. If 0, it's not a range, use only <see cref="OffensivePhysicalBonusMin"/>. </summary>
            public double OffensivePhysicalBonusMax { get; set; }
                    /// <summary> Increases damage done with Pierce by this %. </summary>
            public double OffensivePierceModifier { get; set; }
                    /// <summary> Minimum damage dealt as Pierce. </summary>
            public double OffensivePierceBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Pierce. If 0, it's not a range, use only <see cref="OffensivePierceBaseMin"/>. </summary>
            public double OffensivePierceBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Pierce. </summary>
            public double OffensivePierceBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Pierce. If 0, it's not a range, use only <see cref="OffensivePierceBonusMin"/>. </summary>
            public double OffensivePierceBonusMax { get; set; }
                    /// <summary> Increases damage done with Poison by this %. </summary>
            public double OffensivePoisonModifier { get; set; }
                    /// <summary> Minimum damage dealt as Poison. </summary>
            public double OffensivePoisonBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Poison. If 0, it's not a range, use only <see cref="OffensivePoisonBaseMin"/>. </summary>
            public double OffensivePoisonBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Poison. </summary>
            public double OffensivePoisonBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Poison. If 0, it's not a range, use only <see cref="OffensivePoisonBonusMin"/>. </summary>
            public double OffensivePoisonBonusMax { get; set; }
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
                    /// <summary> Minimum damage dealt as Stun. </summary>
            public double OffensiveStunBaseMin { get; set; }
                    /// <summary> Maximum damage dealt as Stun. If 0, it's not a range, use only <see cref="OffensiveStunBaseMin"/>. </summary>
            public double OffensiveStunBaseMax { get; set; }
                    /// <summary> Minimum bonus damage dealt as Stun. </summary>
            public double OffensiveStunBonusMin { get; set; }
                    /// <summary> Maximum bonus damage dealt as Stun. If 0, it's not a range, use only <see cref="OffensiveStunBonusMin"/>. </summary>
            public double OffensiveStunBonusMax { get; set; }
                    /// <summary>  </summary>
            public double BlockValue { get; set; }
                    /// <summary>  </summary>
            public double BlockChance { get; set; }
                    /// <summary>  </summary>
            public double BlockRecoveryTime { get; set; }
                    /// <summary>  </summary>
            public double ShieldBlockChanceModifier { get; set; }
                    /// <summary>  </summary>
            public double ShieldDamageBlockModifier { get; set; }
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
            public double TotalDamageModifier { get; set; }
                    /// <summary>  </summary>
            public double WeaponDamageModifier { get; set; }
                    /// <summary>  </summary>
            public double SkillCooldownReduction { get; set; }
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
                            RestoreLifePercent += other.RestoreLifePercent;
                            LifeMonitorPercent += other.LifeMonitorPercent;
                            SkillDuration += other.SkillDuration;
                            SkillCooldown += other.SkillCooldown;
                            Energy += other.Energy;
                            EnergyModifier += other.EnergyModifier;
                            EnergyRegeneration += other.EnergyRegeneration;
                            EnergyRegenerationModifier += other.EnergyRegenerationModifier;
                            EnergyCost += other.EnergyCost;
                            EnergyCostModifier += other.EnergyCostModifier;
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
                            MaxResistPhysical += other.MaxResistPhysical;
                            ResistPierce += other.ResistPierce;
                            MaxResistPierce += other.MaxResistPierce;
                            ResistFire += other.ResistFire;
                            MaxResistFire += other.MaxResistFire;
                            ResistCold += other.ResistCold;
                            MaxResistCold += other.MaxResistCold;
                            ResistLightning += other.ResistLightning;
                            MaxResistLightning += other.MaxResistLightning;
                            ResistPoison += other.ResistPoison;
                            MaxResistPoison += other.MaxResistPoison;
                            ResistVitality += other.ResistVitality;
                            MaxResistVitality += other.MaxResistVitality;
                            ResistAether += other.ResistAether;
                            MaxResistAether += other.MaxResistAether;
                            ResistChaos += other.ResistChaos;
                            MaxResistChaos += other.MaxResistChaos;
                            ResistStun += other.ResistStun;
                            MaxResistStun += other.MaxResistStun;
                            MaxResistAll += other.MaxResistAll;
                            ResistElemental += other.ResistElemental;
                            ResistDisruption += other.ResistDisruption;
                            ResistBleed += other.ResistBleed;
                            ResistSlow += other.ResistSlow;
                            ResistKnockdown += other.ResistKnockdown;
                            DamageAbsorptionPercent += other.DamageAbsorptionPercent;
                            OffensiveAetherModifier += other.OffensiveAetherModifier;
                            OffensiveAetherBaseMin += other.OffensiveAetherBaseMin;
                            OffensiveAetherBaseMax += other.OffensiveAetherBaseMax;
                            OffensiveAetherBonusMin += other.OffensiveAetherBonusMin;
                            OffensiveAetherBonusMax += other.OffensiveAetherBonusMax;
                            OffensiveChaosModifier += other.OffensiveChaosModifier;
                            OffensiveChaosBaseMin += other.OffensiveChaosBaseMin;
                            OffensiveChaosBaseMax += other.OffensiveChaosBaseMax;
                            OffensiveChaosBonusMin += other.OffensiveChaosBonusMin;
                            OffensiveChaosBonusMax += other.OffensiveChaosBonusMax;
                            OffensiveColdModifier += other.OffensiveColdModifier;
                            OffensiveColdBaseMin += other.OffensiveColdBaseMin;
                            OffensiveColdBaseMax += other.OffensiveColdBaseMax;
                            OffensiveColdBonusMin += other.OffensiveColdBonusMin;
                            OffensiveColdBonusMax += other.OffensiveColdBonusMax;
                            OffensiveFireModifier += other.OffensiveFireModifier;
                            OffensiveFireBaseMin += other.OffensiveFireBaseMin;
                            OffensiveFireBaseMax += other.OffensiveFireBaseMax;
                            OffensiveFireBonusMin += other.OffensiveFireBonusMin;
                            OffensiveFireBonusMax += other.OffensiveFireBonusMax;
                            OffensiveElementalModifier += other.OffensiveElementalModifier;
                            OffensiveElementalBaseMin += other.OffensiveElementalBaseMin;
                            OffensiveElementalBaseMax += other.OffensiveElementalBaseMax;
                            OffensiveElementalBonusMin += other.OffensiveElementalBonusMin;
                            OffensiveElementalBonusMax += other.OffensiveElementalBonusMax;
                            OffensiveKnockdownModifier += other.OffensiveKnockdownModifier;
                            OffensiveKnockdownBaseMin += other.OffensiveKnockdownBaseMin;
                            OffensiveKnockdownBaseMax += other.OffensiveKnockdownBaseMax;
                            OffensiveKnockdownBonusMin += other.OffensiveKnockdownBonusMin;
                            OffensiveKnockdownBonusMax += other.OffensiveKnockdownBonusMax;
                            OffensiveVitalityModifier += other.OffensiveVitalityModifier;
                            OffensiveVitalityBaseMin += other.OffensiveVitalityBaseMin;
                            OffensiveVitalityBaseMax += other.OffensiveVitalityBaseMax;
                            OffensiveVitalityBonusMin += other.OffensiveVitalityBonusMin;
                            OffensiveVitalityBonusMax += other.OffensiveVitalityBonusMax;
                            OffensiveLightningModifier += other.OffensiveLightningModifier;
                            OffensiveLightningBaseMin += other.OffensiveLightningBaseMin;
                            OffensiveLightningBaseMax += other.OffensiveLightningBaseMax;
                            OffensiveLightningBonusMin += other.OffensiveLightningBonusMin;
                            OffensiveLightningBonusMax += other.OffensiveLightningBonusMax;
                            OffensivePhysicalModifier += other.OffensivePhysicalModifier;
                            OffensivePhysicalBaseMin += other.OffensivePhysicalBaseMin;
                            OffensivePhysicalBaseMax += other.OffensivePhysicalBaseMax;
                            OffensivePhysicalBonusMin += other.OffensivePhysicalBonusMin;
                            OffensivePhysicalBonusMax += other.OffensivePhysicalBonusMax;
                            OffensivePierceModifier += other.OffensivePierceModifier;
                            OffensivePierceBaseMin += other.OffensivePierceBaseMin;
                            OffensivePierceBaseMax += other.OffensivePierceBaseMax;
                            OffensivePierceBonusMin += other.OffensivePierceBonusMin;
                            OffensivePierceBonusMax += other.OffensivePierceBonusMax;
                            OffensivePoisonModifier += other.OffensivePoisonModifier;
                            OffensivePoisonBaseMin += other.OffensivePoisonBaseMin;
                            OffensivePoisonBaseMax += other.OffensivePoisonBaseMax;
                            OffensivePoisonBonusMin += other.OffensivePoisonBonusMin;
                            OffensivePoisonBonusMax += other.OffensivePoisonBonusMax;
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
                            OffensiveStunBaseMin += other.OffensiveStunBaseMin;
                            OffensiveStunBaseMax += other.OffensiveStunBaseMax;
                            OffensiveStunBonusMin += other.OffensiveStunBonusMin;
                            OffensiveStunBonusMax += other.OffensiveStunBonusMax;
                            BlockValue += other.BlockValue;
                            BlockChance += other.BlockChance;
                            BlockRecoveryTime += other.BlockRecoveryTime;
                            ShieldBlockChanceModifier += other.ShieldBlockChanceModifier;
                            ShieldDamageBlockModifier += other.ShieldDamageBlockModifier;
                            OffensiveAbility += other.OffensiveAbility;
                            OffensiveAbilityModifier += other.OffensiveAbilityModifier;
                            DefensiveAbility += other.DefensiveAbility;
                            DefensiveAbilityModifier += other.DefensiveAbilityModifier;
                            AttackSpeedModifier += other.AttackSpeedModifier;
                            CastSpeedModifier += other.CastSpeedModifier;
                            TotalDamageModifier += other.TotalDamageModifier;
                            WeaponDamageModifier += other.WeaponDamageModifier;
                            SkillCooldownReduction += other.SkillCooldownReduction;
                            RunSpeedModifier += other.RunSpeedModifier;
                            AttributeScalePercent += other.AttributeScalePercent;
            
            // add the skills as well
            var otherList = other.SkillsWithQuantity ??= new();
            foreach(var skillWithQuantity in SkillsWithQuantity ??= new())
            {
                var otherSkillWithQuantity = otherList.FirstOrDefault(sq => sq.Skill == skillWithQuantity.Skill);
                if(otherSkillWithQuantity is null)
                    otherList.Add(otherSkillWithQuantity = new() { Skill = skillWithQuantity.Skill});
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
