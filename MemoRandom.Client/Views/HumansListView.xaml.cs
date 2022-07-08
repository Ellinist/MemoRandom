using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumansView.xaml
    /// </summary>
    public partial class HumansListView : MetroWindow
    {
        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Activate();
        }

        #region CTOR
        public HumansListView(HumansListViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
        }
        #endregion
    }
}
