using MemoRandom.Client.Common.Models;
using MemoRandom.Client.ViewModels;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MemoRandom.Client.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ComparedBlockControl.xaml
    /// </summary>
    public partial class ComparedBlockControl : UserControl
    {
        public ComparedBlockControl(ComparedBlockControlViewModel vm, ComparedHuman human, Dispatcher dispatcher)
        {
            InitializeComponent();

            DataContext = vm;
            vm.ProgressDispatcher = dispatcher;
            vm.LeftUpTextBlock = LeftUpTb;
            vm.CenterUpTextBlock = CenterUpTb;
            vm.CurrentProgressBar = CurrentProgressBar;
            vm.ComparedHuman = human;
            this.Loaded += vm.ComparedBlockControl_Loaded;
        }
    }
}
