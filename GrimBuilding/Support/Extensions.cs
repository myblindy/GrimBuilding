using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimBuilding.Support
{
    public static class Extensions
    {
        public static double If0(this double val, double fallback) => val == 0 ? fallback : val;
    }
}
