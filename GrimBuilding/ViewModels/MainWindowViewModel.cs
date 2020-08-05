using DynamicData;
using GrimBuilding.Common.Support;
using LiteDB;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace GrimBuilding.ViewModels
{
    class MainWindowViewModel : ReactiveObject
    {
        public LiteDatabase MainDatabase { get; } = new LiteDatabase("data.db");
        public PlayerClass PlayerClass { get; }
        public ObservableCollection<object> ConstellationDisplayObjects { get; } = new ObservableCollection<object>();

        public MainWindowViewModel()
        {
            PlayerClass = MainDatabase.GetCollection<PlayerClass>()
                .Include(x => x.Skills)
                .FindOne(x => x.Name == "Arcanist");

            ConstellationDisplayObjects.AddRange(MainDatabase.GetCollection<PlayerConstellationNebula>().FindAll());
            var constellations = MainDatabase.GetCollection<PlayerConstellation>().Include(x => x.Skills).FindAll();
            ConstellationDisplayObjects.AddRange(constellations);
            ConstellationDisplayObjects.AddRange(constellations.SelectMany(c => c.Skills));
        }
    }
}