using GrimBuilding.Common.Support;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimBuilding.ViewModels
{
    public class EditItemWindowViewModel : ReactiveObject
    {
        private Item[] allItems;
        public Item[] AllItems { get => allItems; set => this.RaiseAndSetIfChanged(ref allItems, value); }

        Item item;
        public Item Item { get => item; set => this.RaiseAndSetIfChanged(ref item, value); }

        public MainWindowViewModel MainWindowViewModel { get; init; }
    }
}
