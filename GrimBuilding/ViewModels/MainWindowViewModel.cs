using GrimBuilding.Common.Support;
using LiteDB;
using ReactiveUI;

namespace GrimBuilding.ViewModels
{
    class MainWindowViewModel : ReactiveObject
    {
        public LiteDatabase MainDatabase { get; } = new LiteDatabase("data.db");
        public PlayerClass PlayerClass { get; }

        public MainWindowViewModel()
        {
            PlayerClass = MainDatabase.GetCollection<PlayerClass>().FindOne(x => true);
        }
    }
}