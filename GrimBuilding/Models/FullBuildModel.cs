using GrimBuilding.Common.Support;
using GrimBuilding.ViewModels;
using MoreLinq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

        public const int TotalAttributesPerAttributePoint = 8;

        int physique, cunning, spirit;
        public int Physique { get => physique; set { this.RaiseAndSetIfChanged(ref physique, value); } }
        public int Cunning { get => cunning; set { this.RaiseAndSetIfChanged(ref cunning, value); } }
        public int Spirit { get => spirit; set { this.RaiseAndSetIfChanged(ref spirit, value); } }

        public int MaxAttributePoints => 107;
        readonly ObservableAsPropertyHelper<int> totalAttributePoints, unassignedAttributePoints;
        public int TotalAttributePoints => totalAttributePoints.Value;
        public int UnassignedAttributePoints => unassignedAttributePoints.Value;

        public EquipSlotWithItem[] EquipSlotWithItems { get; set; }

        public FullBuildModel()
        {
            this.WhenAnyValue(x => x.Physique, x => x.Cunning, x => x.Spirit, (p, c, s) => (p, c, s))
                .Select(w => w.p + w.c + w.s)
                .ToProperty(this, x => x.TotalAttributePoints, out totalAttributePoints);
            this.WhenAnyValue(x => x.Physique, x => x.Cunning, x => x.Spirit, (p, c, s) => (p, c, s))
                .Select(w => MaxAttributePoints - (w.p + w.c + w.s))
                .ToProperty(this, x => x.UnassignedAttributePoints, out unassignedAttributePoints);
        }

        private static readonly (SolverBase solver, Type type, SolverDependencyAttribute[] dependencies)[] registeredSolvers = Assembly.GetEntryAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(SolverBase)))
            .Select(t => ((SolverBase)Activator.CreateInstance(t), t, t.GetCustomAttributes<SolverDependencyAttribute>().ToArray()))
            .ToArray();

        public const double BaseHealth = 25;

        public IEnumerable<SolverResult> GetSolverResults()
        {
            var summedStats = new BaseStats();
            foreach (var item in EquipSlotWithItems.Select(es => es.Item).Where(w => w is not null))
                summedStats.AddFrom(item);

            var results = new Dictionary<Type, SolverResult>();

            while (results.Count != registeredSolvers.Length)
                foreach (var (solver, type, dependencies) in registeredSolvers.Where(w => !results.ContainsKey(w.type)))
                {
                    if (!dependencies.Any(d => !results.ContainsKey(d.Dependency)))
                        if (solver.Solve(this, summedStats, results, out var result))
                            yield return results[type] = result;
                }
        }
    }

    public class SolverResult
    {
        public double Value { get; set; }
        public string Text { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    class SolverDependencyAttribute : Attribute
    {
        public Type Dependency { get; }
        public SolverDependencyAttribute(Type dependency) => Dependency = dependency;
    }

    public abstract class SolverBase
    {
        public abstract bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result);
    }

    [SolverDependency(typeof(TotalPhysiqueSolver)), SolverDependency(typeof(TotalCunningSolver)), SolverDependency(typeof(TotalSpiritSolver))]
    public class TotalHealthSolver : SolverBase
    {
        const double HealthPerPhysiquePoint = 20.0 / 8.0;
        const int HealthPerOtherPoint = 1;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalPhysique = results[typeof(TotalPhysiqueSolver)].Value;
            var totalCunning = results[typeof(TotalCunningSolver)].Value;
            var totalSpirit = results[typeof(TotalSpiritSolver)].Value;

            var totalHealth =
                (FullBuildModel.BaseHealth + summedStats.Life +
                    HealthPerPhysiquePoint * totalPhysique +
                    HealthPerOtherPoint * (totalCunning + totalSpirit))
                * (1 + summedStats.LifeModifier / 100);
            result = new SolverResult { Text = $"{totalHealth:0} Total Health", Value = totalHealth };
            return totalHealth != 0;
        }
    }

    public class TotalPhysiqueSolver : SolverBase
    {
        const int BasePhysique = 50;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalPhysique =
                (BasePhysique + summedStats.Physique + fullBuild.Physique * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.PhysiqueModifier / 100);
            result = new SolverResult { Text = $"{totalPhysique:0} Total Physique", Value = totalPhysique };
            return totalPhysique != 0;
        }
    }
    public class TotalCunningSolver : SolverBase
    {
        const int BaseCunning = 50;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalCunning =
                (BaseCunning + summedStats.Cunning + fullBuild.Cunning * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.CunningModifier / 100);
            result = new SolverResult { Text = $"{totalCunning:0} Total Cunning", Value = totalCunning };
            return totalCunning != 0;
        }
    }

    public class TotalSpiritSolver : SolverBase
    {
        const int BaseSpirit = 50;

        public override bool Solve(FullBuildModel fullBuild, BaseStats summedStats, Dictionary<Type, SolverResult> results, out SolverResult result)
        {
            var totalSpirit =
                (BaseSpirit + summedStats.Spirit + fullBuild.Spirit * FullBuildModel.TotalAttributesPerAttributePoint)
                * (1 + summedStats.SpiritModifier / 100);
            result = new SolverResult { Text = $"{totalSpirit:0} Total Spirit", Value = totalSpirit };
            return totalSpirit != 0;
        }
    }
}
