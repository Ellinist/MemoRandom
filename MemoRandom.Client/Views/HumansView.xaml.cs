using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumansView.xaml
    /// </summary>
    public partial class HumansView : MetroWindow
    {
        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Activate();
        }

        #region CTOR
        public HumansView(HumansViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
        }
        #endregion
    }
}
