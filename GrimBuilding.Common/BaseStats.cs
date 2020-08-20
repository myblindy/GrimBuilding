

using System.CodeDom.Compiler;

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
                    /// <summary>  </summary>
            public double OffensiveAbility { get; set; }
                    /// <summary>  </summary>
            public double OffensiveAbilityModifier { get; set; }
                    /// <summary>  </summary>
            public double DefensiveAbility { get; set; }
                    /// <summary>  </summary>
            public double DefensiveAbilityModifier { get; set; }
                    /// <summary>  </summary>
            public double RunSpeedModifier { get; set; }
                    /// <summary>  </summary>
            public double AttributeScalePercent { get; set; }
        
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
                            OffensiveAbility += other.OffensiveAbility;
                            OffensiveAbilityModifier += other.OffensiveAbilityModifier;
                            DefensiveAbility += other.DefensiveAbility;
                            DefensiveAbilityModifier += other.DefensiveAbilityModifier;
                            RunSpeedModifier += other.RunSpeedModifier;
                            AttributeScalePercent += other.AttributeScalePercent;
                    }
    }
}