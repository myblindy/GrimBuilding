using GrimBuilding.Solvers;
using GrimBuilding.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GrimBuilding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            DataContext = ViewModel = new();
            InitializeComponent();

            this.WhenActivated(dc =>
            {
                ViewModel.FullBuild.WhenAnyValue(w => w.Class1, w => w.Class2, w => w.Physique, w => w.Cunning, w => w.Spirit, (c1, c2, p, c, s) => System.Reactive.Unit.Default)
                    .InvokeCommand(ViewModel.RecalculateSolverCommand)
                    .DisposeWith(dc);
                ViewModel.FullBuild.WhenAnyValue(x => x.SkillsWithCount1)
                    .Subscribe(skillsWithCount => skillsWithCount
                        .Select(w => w.ObservableForProperty(m => m.Allocated).Select(_=>System.Reactive.Unit.Default)).Amb()
                        .InvokeCommand(ViewModel.RecalculateSolverCommand)
                        .DisposeWith(dc))
                    .DisposeWith(dc);
            });
        }
    }
}
