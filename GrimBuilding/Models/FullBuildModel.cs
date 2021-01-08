using GrimBuilding.Common.Support;
using GrimBuilding.Solvers;
using GrimBuilding.ViewModels;
using MoreLinq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrimBuilding
{
    public class FullBuildModel : ReactiveObject
    {
        PlayerClass class1, class2;
        public PlayerClass Class1 { get => class1; set => this.RaiseAndSetIfChanged(ref class1, value); }
        public PlayerClass Class2 { get => class2; set => this.RaiseAndSetIfChanged(ref class2, value); }

        readonly ObservableAsPropertyHelper<PlayerMasterySkillWithCountModel[]> skillsWithCount1, skillsWithCount2;
        public PlayerMasterySkillWithCountModel[] SkillsWithCount1 => skillsWithCount1.Value;
        public PlayerMasterySkillWithCountModel[] SkillsWithCount2 => skillsWithCount2.Value;

        public const int TotalAttributesPerAttributePoint = 8;

        int physique, cunning, spirit;
        public int Physique { get => physique; set => this.RaiseAndSetIfChanged(ref physique, value); }
        public int Cunning { get => cunning; set => this.RaiseAndSetIfChanged(ref cunning, value); }
        public int Spirit { get => spirit; set => this.RaiseAndSetIfChanged(ref spirit, value); }

        public static int MaxAttributePoints => 107;
        readonly ObservableAsPropertyHelper<int> totalAttributePoints, unassignedAttributePoints;
        public int TotalAttributePoints => totalAttributePoints.Value;
        public int UnassignedAttributePoints => unassignedAttributePoints.Value;

        public EquipSlotWithItem[] EquipSlotWithItems { get; set; }

        public ObservableCollection<Item> BuiltItems { get; } = new();
        public ICommand CreateNewItemCommand { get; }

        public FullBuildModel(MainWindowViewModel vm)
        {
            this.WhenAnyValue(x => x.Class1)
                .Select(c => c?.Skills?.Select(s => new PlayerMasterySkillWithCountModel(s)).ToArray())
                .ToProperty(this, x => x.SkillsWithCount1, out skillsWithCount1);
            this.WhenAnyValue(x => x.Class2)
                .Select(c => c?.Skills?.Select(s => new PlayerMasterySkillWithCountModel(s)).ToArray())
                .ToProperty(this, x => x.SkillsWithCount2, out skillsWithCount2);

            this.WhenAnyValue(x => x.Physique, x => x.Cunning, x => x.Spirit, (p, c, s) => (p, c, s))
                .Select(w => w.p + w.c + w.s)
                .ToProperty(this, x => x.TotalAttributePoints, out totalAttributePoints);
            this.WhenAnyValue(x => x.Physique, x => x.Cunning, x => x.Spirit, (p, c, s) => (p, c, s))
                .Select(w => MaxAttributePoints - (w.p + w.c + w.s))
                .ToProperty(this, x => x.UnassignedAttributePoints, out unassignedAttributePoints);

            CreateNewItemCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var newItem = await vm.EditItemInteraction.Handle(null);
                if (newItem is not null)
                    BuiltItems.Add(newItem);
            });
        }

        private static readonly (SolverBase solver, Type type, SolverDependencyAttribute[] dependencies)[] registeredSolvers =
            Assembly.GetEntryAssembly().GetTypes()
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
                    if (!dependencies.Any(d => !results.ContainsKey(d.Dependency)))
                        yield return results[type] = solver.Solve(this, summedStats, results);
        }

        public IEnumerable<double> GetAllFromMasteries(Func<BaseStats, double> selector) =>
            SkillsWithCount1.Concat(SkillsWithCount2).Select(w => w.Allocated == 0 ? 0 : selector(w.Skill.BaseStatLevels[w.Allocated - 1]));
    }

    public class PlayerMasterySkillWithCountModel : ReactiveObject
    {
        public PlayerSkill Skill { get; private set; }

        int allocated;
        public int Allocated { get => allocated; set => this.RaiseAndSetIfChanged(ref allocated, value); }

        public ICommand IncreaseSkillCommand { get; }
        public ICommand DecreaseSkillCommand { get; }

        public PlayerMasterySkillWithCountModel(PlayerSkill skill) =>
            (Skill, IncreaseSkillCommand, DecreaseSkillCommand) =
                (skill,
                    ReactiveCommand.Create(() => ++Allocated, this.WhenAnyValue(x => x.Allocated, a => a < skill.MaximumLevel)),
                    ReactiveCommand.Create(() => --Allocated, this.WhenAnyValue(x => x.Allocated, a => a > 0)));
    }
}
