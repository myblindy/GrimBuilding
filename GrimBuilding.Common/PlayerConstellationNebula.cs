using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerConstellationNebula
    {
        public int Id { get; set; }

        public string BitmapPath { get; set; }
        [BsonIgnore]
        public byte[] Bitmap { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }
}