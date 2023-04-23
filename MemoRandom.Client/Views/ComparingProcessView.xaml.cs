using MemoRandom.Client.ViewModels;
using MahApps.Metro.Controls;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ComparingProcessView.xaml
    /// </summary>
    public partial class ComparingProcessView : MetroWindow
    {
        #region CTOR
        public ComparingProcessView(ComparingProcessViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            this.Loaded += vm.ComparingProcessView_Loaded;
            this.Closing += vm.ComparingProcessView_Closing;
        }
        #endregion
    }
}
