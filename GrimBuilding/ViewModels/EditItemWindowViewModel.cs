using GrimBuilding.Common.Support;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrimBuilding.ViewModels
{
    public class EditItemWindowViewModel : ReactiveObject
    {
        private Item[] allItems;
        public Item[] AllItems { get => allItems; set => this.RaiseAndSetIfChanged(ref allItems, value); }

        Item item;
        public Item Item { get => item; set => this.RaiseAndSetIfChanged(ref item, value); }

        public MainWindowViewModel MainWindowViewModel { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "helper binding class")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "helper binding class")]
        public struct DisplayTextValuePair
        {
            public DisplayTextValuePair(string name, ItemType value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; init; }
            public ItemType Value { get; init; }
        }

        static string RemoveSuperFromDisplayText(string text) => text.StartsWith("Super") ? text[5..] : text;

        private static readonly DisplayTextValuePair[] superItemTypes = Enum.GetValues(typeof(ItemType)).Cast<ItemType>()
            .Select(v => new DisplayTextValuePair(RemoveSuperFromDisplayText(Enum.GetName(typeof(ItemType), v)), v))
            .ToArray();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "binding helper")]
        public DisplayTextValuePair[] SuperItemTypes => superItemTypes;

        ItemType superItemType = ItemType.SuperWeapon;
        public ItemType SuperItemType { get => superItemType; set => this.RaiseAndSetIfChanged(ref superItemType, value); }

        bool dialogResult;
        public bool DialogResult { get => dialogResult; set => this.RaiseAndSetIfChanged(ref dialogResult, value); }

        public ICommand OKCommand { get; }

        public EditItemWindowViewModel()
        {
            OKCommand = ReactiveCommand.Create(() => DialogResult = true);
        }
    }
}
