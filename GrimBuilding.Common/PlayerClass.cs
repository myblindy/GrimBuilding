using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerClass
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string BitmapPath { get; set; }
        [NotMapped]
        public byte[] Bitmap { get; set; }

        public List<PlayerSkill> Skills { get; set; }
    }
}