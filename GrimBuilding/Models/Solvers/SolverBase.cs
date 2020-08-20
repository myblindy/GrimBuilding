using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;

namespace GrimBuilding.Solvers
{
    public abstract class SolverBase
    {
        public abstract bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result);
    }
}
