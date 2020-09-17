using System.ComponentModel.DataAnnotations.Schema;

namespace GrimBuilding.Common.Support
{
    public class PlayerConstellationNebula
    {
        public int Id { get; set; }

        public string BitmapPath { get; set; }
        [NotMapped]
        public byte[] Bitmap { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }
}