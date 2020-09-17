using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimBuilding.Common
{
    public class PlayerResistance
    {
        public int Id { get; set; }
        public ResistanceType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string BitmapPath { get; set; }
        [NotMapped]
        public byte[] Bitmap { get; set; }
    }
}
