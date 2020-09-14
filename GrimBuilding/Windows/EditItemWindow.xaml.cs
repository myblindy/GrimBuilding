using GrimBuilding.Common.Support;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GrimBuilding.Windows
{
    /// <summary>
    /// Interaction logic for EditItemWindow.xaml
    /// </summary>
    public partial class EditItemWindow
    {
        public EditItemWindow()
        {
            DataContext = ViewModel = new();
            InitializeComponent();

            ViewModel.WhenAnyValue(x => x.SuperItemType)
                .Subscribe(_ => ((CollectionViewSource)Resources["AllItemsViewSource"]).View?.Refresh());

            ViewModel.WhenAnyValue(x => x.DialogResult)
                .Subscribe(result => { if (result) { DialogResult = result; Close(); } });
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e) =>
            e.Accepted = ((Item)e.Item).IsOfType(ViewModel.SuperItemType);
    }
}
