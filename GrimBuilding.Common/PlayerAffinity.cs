using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerAffinity
    {
        public int PlayerAffinityId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}