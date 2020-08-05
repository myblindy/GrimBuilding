using LiteDB;
using System.Windows;
using System.Windows.Controls;

namespace GrimBuilding.Controls
{
    public partial class MasteryViewerControl : UserControl
    {
        public LiteDatabase MainDatabase
        {
            get { return (LiteDatabase)GetValue(MainDatabaseProperty); }
            set { SetValue(MainDatabaseProperty, value); }
        }

        public static readonly DependencyProperty MainDatabaseProperty =
            DependencyProperty.Register(nameof(MainDatabase), typeof(LiteDatabase), typeof(MasteryViewerControl));

        public MasteryViewerControl()
        {
            InitializeComponent();
        }
    }
}
