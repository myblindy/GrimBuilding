using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public enum EquipSlotType
    {
        Relic,
        Chest,
        Feet,
        Finger1,
        Finger2,
        HandLeft,
        HandRight,
        Hands,
        Head,
        Legs,
        Medal,
        Neck,
        Shoulders,
        Waist,
    }

    public class EquipSlot
    {
        public int Id { get; set; }

        public EquipSlotType Type { get; set; }

        public string SilhouetteBitmapPath { get; set; }
        [BsonIgnore]
        public byte[] SilhouetteBitmap { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}