using GrimBuilding.ViewModels;
using MoreLinq;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace GrimBuilding.Windows
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
                        .ForEach(w => w.ObservableForProperty(m => m.Allocated).Select(_ => System.Reactive.Unit.Default)
                            .InvokeCommand(ViewModel.RecalculateSolverCommand)
                            .DisposeWith(dc)))
                    .DisposeWith(dc);
                ViewModel.FullBuild.WhenAnyValue(x => x.SkillsWithCount2)
                    .Subscribe(skillsWithCount => skillsWithCount
                        .ForEach(w => w.ObservableForProperty(m => m.Allocated).Select(_ => System.Reactive.Unit.Default)
                            .InvokeCommand(ViewModel.RecalculateSolverCommand)
                            .DisposeWith(dc)))
                    .DisposeWith(dc);
                ViewModel.EditItemInteraction.RegisterHandler(ctx =>
                {
                    var dlg = new EditItemWindow { ViewModel = { AllItems = ViewModel.AllItems, Item = ctx.Input ?? new(), MainWindowViewModel = ViewModel }, Owner = this };
                    ctx.SetOutput(dlg.ShowDialog() ?? false ? dlg.ViewModel.Item : null);
                }).DisposeWith(dc);
            });
        }
    }
}
