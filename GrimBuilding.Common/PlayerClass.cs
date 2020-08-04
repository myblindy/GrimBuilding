using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerSkill
    {
        public int PlayerSkillId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string BitmapUpPath { get; set; }
        [BsonIgnore]
        public byte[] BitmapUp { get; set; }
        public string BitmapDownPath { get; set; }
        [BsonIgnore]
        public byte[] BitmapDown { get; set; }

        public string BitmapFrameUpPath { get; set; }
        [BsonIgnore]
        public byte[] BitmapFrameUp { get; set; }
        public string BitmapFrameInFocusPath { get; set; }
        [BsonIgnore]
        public byte[] BitmapFrameInFocus { get; set; }
        public string BitmapFrameDownPath { get; set; }
        [BsonIgnore]
        public byte[] BitmapFrameDown { get; set; }

        public string[] BitmapSkillConnectionOffPaths { get; set; }
        [BsonIgnore]
        public byte[][] BitmapSkillConnectionsOff { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int MaximumLevel { get; set; }
        public int? UltimateLevel { get; set; }
        public int? MasteryLevelRequirement { get; set; }
        public bool Circular { get; set; }
    }

    public class PlayerClass
    {
        public int PlayerClassId { get; set; }

        public string Name { get; set; }

        [BsonRef]
        public PlayerSkill[] Skills { get; set; }
    }
}