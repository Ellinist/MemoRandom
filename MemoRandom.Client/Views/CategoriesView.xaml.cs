using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для CategoriesView.xaml
    /// </summary>
    public partial class CategoriesView : MetroWindow
    {
        public CategoriesView(CategoriesViewModel vm)
        {
            InitializeComponent();

            this.Loaded += vm.CategoriesView_Loaded;
            DataContext = vm;
        }
    }
}
