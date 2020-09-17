using GrimBuilding.Common;
using GrimBuilding.Common.Support;
using System.Windows;
using System.Windows.Controls;

namespace GrimBuilding.Controls
{
    public partial class MasteryViewerControl : UserControl
    {
        public PlayerClass PlayerClass
        {
            get { return (PlayerClass)GetValue(PlayerClassProperty); }
            set { SetValue(PlayerClassProperty, value); }
        }

        public static readonly DependencyProperty PlayerClassProperty =
            DependencyProperty.Register(nameof(PlayerClass), typeof(PlayerClass), typeof(MasteryViewerControl));

        public PlayerMasterySkillWithCountModel[] PlayerSkillsWithCount
        {
            get { return (PlayerMasterySkillWithCountModel[])GetValue(PlayerSkillsWithCountProperty); }
            set { SetValue(PlayerSkillsWithCountProperty, value); }
        }

        public static readonly DependencyProperty PlayerSkillsWithCountProperty =
            DependencyProperty.Register(nameof(PlayerSkillsWithCount), typeof(PlayerMasterySkillWithCountModel[]), typeof(MasteryViewerControl));

        public MasteryViewerControl()
        {
            InitializeComponent();
        }
    }
}
