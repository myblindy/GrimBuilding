using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class Item : BaseStats
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public string ArmorClassificationText { get; set; }
        public ItemRarity Rarity { get; set; }
        public ItemArtifactRarity ArtifactRarity { get; set; }

        public int ItemLevel { get; set; }
        public string ItemStyleText { get; set; }

        public string BitmapPath { get; set; }
        [NotMapped]
        public byte[] Bitmap { get; set; }

        private static readonly Dictionary<ItemType, HashSet<ItemType>> superTypes = new()
        {
            [ItemType.Weapon] = new(){ItemType.WeaponCrossbow, ItemType.WeaponDagger, ItemType.WeaponOneHandedAxe, ItemType.WeaponOneHandedGun, ItemType.WeaponOneHandedAxe, ItemType.WeaponOneHandedMace, ItemType.WeaponOneHandedSword, ItemType.WeaponOneHandedAxe, ItemType.WeaponOneHandedAxe, ItemType.WeaponOneHandedAxe, ItemType.WeaponOneHandedAxe, }
        };

        public bool IsOfType(ItemType type) => superTypes[type].Contains(Type);

        [NotMapped]
        public bool IsWeaponOrOffHand => Type == ItemType.WeaponCrossbow || Type == ItemType.WeaponDagger || Type == ItemType.WeaponOneHandedAxe || Type == ItemType.WeaponOneHandedGun
            || Type == ItemType.WeaponOneHandedMace || Type == ItemType.WeaponOneHandedSword || Type == ItemType.WeaponTwoHandedAxe || Type == ItemType.WeaponTwoHandedGun
            || Type == ItemType.WeaponTwoHandedMace || Type == ItemType.WeaponTwoHandedSword || Type == ItemType.Shield || Type == ItemType.OffhandFocus;
    }

    public enum ItemType
    {
        Feet, Hands, Head, Legs, Relic, Shoulders, Chest,
        WeaponOneHandedAxe, WeaponOneHandedSword, WeaponOneHandedMace, WeaponOneHandedGun, WeaponDagger,
        WeaponTwoHandedAxe, WeaponTwoHandedSword, WeaponTwoHandedMace, WeaponTwoHandedGun, WeaponCrossbow,
        OffhandFocus, Shield,
        Medal, Amulet, Ring, Belt,

        Weapon,
        OneHandedWeapon, OneHandedMeleeWeapon, OffhandWeapon, OneHandedRangedWeapon,
        TwoHandedWeapon, TwoHandedMeleeWeapon, TwoHandedRangedWeapon,
    }

    public enum ItemRarity
    {
        Broken, Common, Magical, Rare, Epic, Legendary, Quest,
        Artifact, ArtifactFormula, Enchantment, Relic,
    }

    public enum ItemArtifactRarity { None, Lesser, Greater, Divine }
}