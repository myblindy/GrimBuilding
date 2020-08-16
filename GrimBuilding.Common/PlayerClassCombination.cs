using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerClassCombination
    {
        public int Id { get; set; }

        public string ClassName1 { get; set; }
        public string ClassName2 { get; set; }
        public string Name { get; set; }
    }
}