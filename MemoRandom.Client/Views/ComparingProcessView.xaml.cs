using MemoRandom.Client.ViewModels;
using System.Windows;
using MahApps.Metro.Controls;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ComparingProcessView.xaml
    /// </summary>
    public partial class ComparingProcessView : MetroWindow
    {
        private ComparingProcessViewModel _vm;

        private void ComparingProcessView_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.SetStackPanel(ProgressStackPanel);
        }






        #region CTOR
        public ComparingProcessView(ComparingProcessViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            _vm = vm;
            this.Loaded += ComparingProcessView_Loaded;
            this.Closing += vm.ComparingProcessView_Closing;
        }
        #endregion
    }
}
