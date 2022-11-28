using MemoRandom.Client.ViewModels;
using System.Windows;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ComparingProcessView.xaml
    /// </summary>
    public partial class ComparingProcessView : Window
    {
        private ComparingProcessViewModel _vm;

        private void ComparingProcessView_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.GetStackPanel(ProgressStackPanel);
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
