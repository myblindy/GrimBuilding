using DynamicData;
using GrimBuilding.Common;
using GrimBuilding.Common.Support;
using GrimBuilding.Solvers;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Windows.Input;

namespace GrimBuilding.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public Interaction<Item, Item> EditItemInteraction { get; } = new();

        public Item[] AllItems { get; }
        public PlayerClass[] PlayerClasses { get; }
        public Dictionary<(string c1, string c2), string> PlayerClassCombinations { get; } = new();
        public List<ConstellationDisplayObjectModel> ConstellationDisplayObjects { get; } = new();
        public Dictionary<ItemRarity, ItemRarityTextStyle> ItemRarityTextStyles { get; } = new();

        public FullBuildModel FullBuild { get; }
        public ObservableCollection<SolverResult> CurrentSolverResults { get; } = new();
        public ObservableCollection<SolverResult> OtherSolverResults { get; } = new()
        {
            new($"{60.0:0} Total Cunning"),
            new($"{280}-{720} Weapon Hit"),
        };
        public ICommand RecalculateSolverCommand { get; }

        public MainWindowViewModel()
        {
            FullBuild = new(this);

            RecalculateSolverCommand = ReactiveCommand.Create(() =>
                {
                    CurrentSolverResults.Clear();
                    CurrentSolverResults.AddRange(FullBuild.GetSolverResults());
                });

            using var db = new GdDbContext();

            PlayerClasses = db.PlayerClasses.Include(x => x.Skills).ThenInclude(x => x.BaseStatLevels).OrderBy(pc => pc.Name).ToArray();

            // sort player class skills by their x coordinate, to simplify displaying
            PlayerClasses.ForEach(pc => pc.Skills.Sort((a, b) => a.PositionX.CompareTo(b.PositionX)));

            // sort skill levels by their ID
            PlayerClasses.ForEach(pc => pc.Skills.ForEach(s => s.BaseStatLevels.Sort((a, b) => a.LevelIndex.CompareTo(b.LevelIndex))));

            FullBuild.Class1 = PlayerClasses[0];
            FullBuild.Class2 = PlayerClasses[4];

            ConstellationDisplayObjects.AddRange(db.PlayerConstellationNebulas.Select(obj => new ConstellationNebulaDisplayObjectModel { Object = obj }));
            var constellations = db.PlayerConstellations.Include(x => x.Skills).ToList();
            ConstellationDisplayObjects.AddRange(constellations.Select(obj => new PlayerConstellationDisplayObjectModel { Object = obj }));

            foreach (var constellation in constellations)
            {
                int skillIdx = 0;
                foreach (var skill in constellation.Skills)
                {
                    var requirement = constellation.SkillRequirements.Count > skillIdx ? constellation.Skills[constellation.SkillRequirements[skillIdx]] : null;

                    ConstellationDisplayObjects.Add(new PlayerSkillConstellationDisplayObjectModel
                    {
                        Object = skill,
                        DependencyPositionX = (requirement?.PositionX ?? skill.PositionX) - skill.PositionX,
                        DependencyPositionY = (requirement?.PositionY ?? skill.PositionY) - skill.PositionY,
                    });
                    ++skillIdx;
                }
            }

            foreach (var combination in db.PlayerClassCombinations)
                PlayerClassCombinations.Add((combination.ClassName1, combination.ClassName2), combination.Name);

            foreach (var style in db.ItemRarityTextStyles)
                ItemRarityTextStyles.Add(style.Rarity, style);

            FullBuild.EquipSlotWithItems = db.EquipSlots
                .Select(es => new EquipSlotWithItem { EquipSlot = es })
                .ToArray();

            var sw = Stopwatch.StartNew();
            AllItems = db.Items
                .Include(w => w.SkillsWithQuantity).ThenInclude(w => w.Skill)
                .ToArray();
            Debug.WriteLine(sw.Elapsed);

            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Feet).Item =
            //    items.Find(i => i.Type == ItemType.Feet && i.Name == "Dreadnought Footpads").Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Shoulders).Item =
            //    items.Find(i => i.Type == ItemType.Shoulders && i.Name.StartsWith("Rah'Zin")).Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Chest).Item =
            //    items.Find(i => i.Type == ItemType.Chest && i.Name.StartsWith("Gildor's Guard")).Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Finger1).Item =
            //    items.Find(i => i.Type == ItemType.Ring && i.Name.StartsWith("Aetherlord's Signet")).Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Finger2).Item =
            //    items.Find(i => i.Type == ItemType.Ring && i.Name.Contains("Open Hand")).Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.HandRight).Item =
            //    items.Find(i => i.Name.Contains("Scion of Crimson") && i.ItemStyleText == "Mythical").Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Neck).Item =
            //    items.Find(i => i.Name.Contains("Ultos' Gem")).Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.HandLeft).Item =
            //    items.Find(i => i.Name.Contains("Will of the Living")).Last();
            //FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Medal).Item =
            //    items.Find(i => i.Name.Contains("Markovian's Stratagem") && i.ItemStyleText == "Mythical").Last();

            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.HandRight).Item =
                AllItems.First(i => i.Name == "Wendigo Cleaver" && string.IsNullOrWhiteSpace(i.ItemStyleText));
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Finger1).Item =
                AllItems.First(i => i.Type == ItemType.Ring && i.Name.StartsWith("Skinner") && i.ItemLevel == 20);
        }
    }

    public class EquipSlotWithItem : ReactiveObject
    {
        EquipSlot equipSlot;
        public EquipSlot EquipSlot { get => equipSlot; set => this.RaiseAndSetIfChanged(ref equipSlot, value); }

        Item item;
        public Item Item { get => item; set => this.RaiseAndSetIfChanged(ref item, value); }
    }

    public class ConstellationDisplayObjectModel : ReactiveObject
    {
        bool selected;
        public bool Selected { get => selected; set => this.RaiseAndSetIfChanged(ref selected, value); }
    }

    public class ConstellationNebulaDisplayObjectModel : ConstellationDisplayObjectModel
    {
        public PlayerConstellationNebula Object { get; set; }
    }

    public class PlayerConstellationDisplayObjectModel : ConstellationDisplayObjectModel
    {
        public PlayerConstellation Object { get; set; }
    }

    public class PlayerSkillConstellationDisplayObjectModel : ConstellationDisplayObjectModel
    {
        public PlayerSkill Object { get; set; }
        public int DependencyPositionX { get; set; }
        public int DependencyPositionY { get; set; }
    }
}