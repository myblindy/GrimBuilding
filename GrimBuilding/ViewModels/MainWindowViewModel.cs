using DynamicData;
using GrimBuilding.Common.Support;
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
        public EquipSlotWithItem[] EquipSlotWithItems { get; }
        public Dictionary<ItemRarity, ItemRarityTextStyle> ItemRarityTextStyles { get; } = new();

        public MainWindowViewModel()
        {
            PlayerClasses = MainDatabase.GetCollection<PlayerClass>().Include(x => x.Skills).FindAll().ToArray();

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

            EquipSlotWithItems = MainDatabase.GetCollection<EquipSlot>().FindAll()
                .Select(es => new EquipSlotWithItem { EquipSlot = es })
                .ToArray();
            EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Feet).Item =
                MainDatabase.GetCollection<Item>().Find(i => i.Type == ItemType.Feet && i.Name == "Dreadnought Footpads").Last();
            EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Shoulders).Item =
                MainDatabase.GetCollection<Item>().Find(i => i.Type == ItemType.Shoulders && i.Name.StartsWith("Rah'Zin")).Last();
            EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Chest).Item =
                MainDatabase.GetCollection<Item>().Find(i => i.Type == ItemType.Chest && i.Name.StartsWith("Gildor's Guard")).Last();
            EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Finger1).Item =
                MainDatabase.GetCollection<Item>().Find(i => i.Type == ItemType.Ring && i.Name.StartsWith("Aetherlord's Signet")).Last();
            EquipSlotWithItems.First(es => es.EquipSlot.Type == EquipSlotType.Finger2).Item =
                MainDatabase.GetCollection<Item>().Find(i => i.Type == ItemType.Ring && i.Name.Contains("Open Hand")).Last();
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