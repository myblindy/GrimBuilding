using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class Item : BaseStats
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public ItemArtifactRarity ArtifactRarity { get; set; }

        public int ItemLevel { get; set; }

        public string BitmapPath { get; set; }
        [BsonIgnore]
        public byte[] Bitmap { get; set; }
    }

    public enum ItemType
    {
        Feet, Hands, Head, Legs, Relic, Shoulders, Chest,
        WeaponOneHandedAxe, WeaponOneHandedSword, WeaponOneHandedMace, WeaponOneHandedGun, WeaponDagger,
        WeaponTwoHandedAxe, WeaponTwoHandedSword, WeaponTwoHandedMace, WeaponTwoHandedGun, WeaponCrossbow,
        OffhandFocus, Shield,
        Medal, Amulet, Ring, Belt,
    }

    public enum ItemRarity { Junk, Common, Magical, Rare, Epic, Legendary, Quest }

    public enum ItemArtifactRarity { None, Lesser, Greater, Divine }
}