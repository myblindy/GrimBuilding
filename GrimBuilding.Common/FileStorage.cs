using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimBuilding.Common
{
    [Index("Path", IsUnique = true)]
    public class FileData
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }
}
