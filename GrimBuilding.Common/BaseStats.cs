using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class BaseStats
    {
        public int LevelRequirement { get; set; }
        public double PhysiqueRequirement { get; set; }
        public double CunningRequirement { get; set; }
        public double SpiritRequirement { get; set; }

        public double Life { get; set; }
        public double LifeModifier { get; set; }
        public double LifeRegeneration { get; set; }
        public double LifeRegenerationModifier { get; set; }

        public double Physique { get; set; }
        public double PhysiqueModifier { get; set; }
        public double Cunning { get; set; }
        public double CunningModifier { get; set; }
        public double Spirit { get; set; }
        public double SpiritModifier { get; set; }

        public double Armor { get; set; }
        public double ArmorChance { get; set; }
        public double ArmorModifier { get; set; }
        public double ArmorModifierChance { get; set; }

        /// <summary>
        /// Only defensive absorption stat in use, maps to armor absorption %.
        /// </summary>
        public double ArmorAbsorptionModifier { get; set; }

        public double AttributeScalePercent { get; set; }
    }
}