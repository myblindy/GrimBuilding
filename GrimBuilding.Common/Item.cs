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
            [ItemType.SuperWeapon] = new()
            {
                ItemType.WeaponDagger,
                ItemType.WeaponScepter,
                ItemType.WeaponOneHandedAxe,
                ItemType.WeaponOneHandedGun,
                ItemType.WeaponOneHandedMace,
                ItemType.WeaponOneHandedSword,
                ItemType.WeaponTwoHandedAxe,
                ItemType.WeaponTwoHandedGun,
                ItemType.WeaponTwoHandedMace,
                ItemType.WeaponTwoHandedSword,
                ItemType.WeaponCrossbow,
                ItemType.Shield,
                ItemType.OffhandFocus,
            },
            [ItemType.SuperOneHandedWeapon] = new()
            {
                ItemType.WeaponDagger,
                ItemType.WeaponScepter,
                ItemType.WeaponOneHandedAxe,
                ItemType.WeaponOneHandedGun,
                ItemType.WeaponOneHandedMace,
                ItemType.WeaponOneHandedSword,
                ItemType.Shield,
                ItemType.OffhandFocus,
            },
            [ItemType.SuperOneHandedMeleeWeapon] = new()
            {
                ItemType.WeaponDagger,
                ItemType.WeaponScepter,
                ItemType.WeaponOneHandedAxe,
                ItemType.WeaponOneHandedGun,
                ItemType.WeaponOneHandedMace,
                ItemType.WeaponOneHandedSword,
            },
            [ItemType.SuperOffhandWeapon] = new()
            {
                ItemType.Shield,
                ItemType.OffhandFocus,
            },
            [ItemType.SuperOneHandedRangedWeapon] = new()
            {
                ItemType.WeaponOneHandedGun,
            },
            [ItemType.SuperTwoHandedWeapon] = new()
            {
                ItemType.WeaponTwoHandedAxe,
                ItemType.WeaponTwoHandedGun,
                ItemType.WeaponTwoHandedMace,
                ItemType.WeaponTwoHandedSword,
                ItemType.WeaponCrossbow,
            },
            [ItemType.SuperTwoHandedMeleeWeapon] = new()
            {
                ItemType.WeaponTwoHandedAxe,
                ItemType.WeaponTwoHandedMace,
                ItemType.WeaponTwoHandedSword,
            },
            [ItemType.SuperTwoHandedRangedWeapon] = new()
            {
                ItemType.WeaponTwoHandedGun,
                ItemType.WeaponCrossbow,
            },
        };

        public bool IsOfType(ItemType type) => superTypes.TryGetValue(type, out var realTypes) ? realTypes.Contains(Type) : type == Type;
    }

    public enum ItemType
    {
        Feet, Hands, Head, Legs, Relic, Shoulders, Chest,
        WeaponOneHandedAxe, WeaponOneHandedSword, WeaponOneHandedMace, WeaponOneHandedGun, WeaponDagger, WeaponScepter,
        WeaponTwoHandedAxe, WeaponTwoHandedSword, WeaponTwoHandedMace, WeaponTwoHandedGun, WeaponCrossbow,
        OffhandFocus, Shield,
        Medal, Amulet, Ring, Belt,

        SuperWeapon,
        SuperOneHandedWeapon, SuperOneHandedMeleeWeapon, SuperOffhandWeapon, SuperOneHandedRangedWeapon,
        SuperTwoHandedWeapon, SuperTwoHandedMeleeWeapon, SuperTwoHandedRangedWeapon,
    }

    public enum ItemRarity
    {
        Broken, Common, Magical, Rare, Epic, Legendary, Quest,
        Artifact, ArtifactFormula, Enchantment, Relic,
    }

    public enum ItemArtifactRarity { None, Lesser, Greater, Divine }
}