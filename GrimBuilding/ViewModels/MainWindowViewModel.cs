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
    class MainWindowViewModel : ReactiveObject
    {
        public LiteDatabase MainDatabase { get; } = new LiteDatabase("data.db");
        public IEnumerable<PlayerClass> PlayerClasses { get; }

        public ObservableCollection<ConstellationDisplayObjectModel> ConstellationDisplayObjects { get; } = new ObservableCollection<ConstellationDisplayObjectModel>();

        public MainWindowViewModel()
        {
            var playerClasses = MainDatabase.GetCollection<PlayerClass>().Include(x => x.Skills);
            PlayerClasses = playerClasses.FindAll().ToArray();

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
        }
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