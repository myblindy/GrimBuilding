using DynamicData;
using GrimBuilding.Common.Support;
using GrimBuilding.Solvers;
using LiteDB;
using MoreLinq;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace GrimBuilding.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public LiteDatabase MainDatabase { get; } = new LiteDatabase("data.db");
        public PlayerClass[] PlayerClasses { get; }
        public Dictionary<(string c1, string c2), string> PlayerClassCombinations { get; } = new();
        public List<ConstellationDisplayObjectModel> ConstellationDisplayObjects { get; } = new();
        public Dictionary<ItemRarity, ItemRarityTextStyle> ItemRarityTextStyles { get; } = new();

        public FullBuildModel FullBuild { get; } = new FullBuildModel();
        public ObservableCollection<SolverResult> CurrentSolverResults { get; } = new ObservableCollection<SolverResult>();
        public ICommand RecalculateSolverCommand { get; }

        public MainWindowViewModel()
        {
            RecalculateSolverCommand = ReactiveCommand.Create(() =>
              {
                  CurrentSolverResults.Clear();
                  CurrentSolverResults.AddRange(FullBuild.GetSolverResults());
              });

            PlayerClasses = MainDatabase.GetCollection<PlayerClass>().Include(x => x.Skills).FindAll().ToArray();
            FullBuild.Class1 = PlayerClasses[0];
            FullBuild.Class2 = PlayerClasses[4];

            ConstellationDisplayObjects.AddRange(MainDatabase.GetCollection<PlayerConstellationNebula>().FindAll().Select(obj => new ConstellationNebulaDisplayObjectModel { Object = obj }));
            var constellations = MainDatabase.GetCollection<PlayerConstellation>().Include(x => x.Skills).FindAll();
            ConstellationDisplayObjects.AddRange(constellations.Select(obj => new PlayerConstellationDisplayObjectModel { Object = obj }));

            foreach (var constellation in constellations)
            {
                int skillIdx = 0;
                foreach (var skill in constellation.Skills)
                {
                    var requirement = constellation.SkillRequirements.Length > skillIdx ? constellation.Skills[constellation.SkillRequirements[skillIdx]] : null;

                    ConstellationDisplayObjects.Add(new PlayerSkillConstellationDisplayObjectModel
                    {
                        Object = skill,
                        DependencyPositionX = (requirement?.PositionX ?? skill.PositionX) - skill.PositionX,
                        DependencyPositionY = (requirement?.PositionY ?? skill.PositionY) - skill.PositionY,
                    });
                    ++skillIdx;
                }
            }

            foreach (var combination in MainDatabase.GetCollection<PlayerClassCombination>().FindAll())
                PlayerClassCombinations.Add((combination.ClassName1, combination.ClassName2), combination.Name);

            foreach (var style in MainDatabase.GetCollection<ItemRarityTextStyle>().FindAll())
                ItemRarityTextStyles.Add(style.Rarity, style);

            FullBuild.EquipSlotWithItems = MainDatabase.GetCollection<EquipSlot>().FindAll()
                .Select(es => new EquipSlotWithItem { EquipSlot = es })
                .ToArray();

            var items = MainDatabase.GetCollection<Item>()
                .Include(BsonExpression.Create(@"$.SkillsWithQuantity[*]"))
                .Include(BsonExpression.Create(@"$.SkillsWithQuantity[*].Skill"));
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Feet).Item =
                items.Find(i => i.Type == ItemType.Feet && i.Name == "Dreadnought Footpads").Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Shoulders).Item =
                items.Find(i => i.Type == ItemType.Shoulders && i.Name.StartsWith("Rah'Zin")).Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Chest).Item =
                items.Find(i => i.Type == ItemType.Chest && i.Name.StartsWith("Gildor's Guard")).Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Finger1).Item =
                items.Find(i => i.Type == ItemType.Ring && i.Name.StartsWith("Aetherlord's Signet")).Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Finger2).Item =
                items.Find(i => i.Type == ItemType.Ring && i.Name.Contains("Open Hand")).Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.HandRight).Item =
                items.Find(i => i.Name.Contains("Scion of Crimson") && i.ItemStyleText == "Mythical").Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Neck).Item =
                items.Find(i => i.Name.Contains("Ultos' Gem")).Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.HandLeft).Item =
                items.Find(i => i.Name.Contains("Will of the Living")).Last();
            FullBuild.EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Medal).Item =
                items.Find(i => i.Name.Contains("Markovian's Stratagem") && i.ItemStyleText == "Mythical").Last();
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