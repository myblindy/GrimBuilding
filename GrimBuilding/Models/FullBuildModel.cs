using GrimBuilding.Common.Support;
using GrimBuilding.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GrimBuilding
{
    public class FullBuildModel : ReactiveObject
    {
        PlayerClass class1, class2;
        public PlayerClass Class1 { get => class1; set => this.RaiseAndSetIfChanged(ref class1, value); }
        public PlayerClass Class2 { get => class2; set => this.RaiseAndSetIfChanged(ref class2, value); }

        public EquipSlotWithItem[] EquipSlotWithItems { get; set; }

        private static readonly SolverBase[] registeredSolvers = Assembly.GetEntryAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(SolverBase)))
            .Select(t => (SolverBase)Activator.CreateInstance(t))
            .ToArray();

        public const double BaseHealth = 250;

        public IEnumerable<SolverResult> GetSolverResults()
        {
            foreach (var solver in registeredSolvers)
                if (solver.Solve(this, out var result))
                    yield return result;
        }
    }

    public struct SolverResult
    {
        public string Text;
    }

    public abstract class SolverBase
    {
        public abstract bool Solve(FullBuildModel fullBuild, out SolverResult result);
    }

    public class TotalHealthSolver : SolverBase
    {
        public override bool Solve(FullBuildModel fullBuild, out SolverResult result)
        {
            var totalHealth = (FullBuildModel.BaseHealth + 20 * fullBuild.EquipSlotWithItems.Sum(es => es.Item?.Physique ?? 0) + fullBuild.EquipSlotWithItems.Sum(es => es.Item?.Life ?? 0))
                * (1 + fullBuild.EquipSlotWithItems.Sum(es => es.Item?.LifeModifier ?? 0));
            result = new SolverResult { Text = $"{totalHealth} Total Health" };
            return totalHealth != 0;
        }
    }
}
