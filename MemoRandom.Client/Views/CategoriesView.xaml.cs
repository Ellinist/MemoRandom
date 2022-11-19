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

            DataContext = vm;
            this.Loaded += vm.CategoriesView_Loaded;
            DgCategories.SelectionChanged += DgCategories_SelectionChanged;
        }

        private void DgCategories_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DgCategories.Focus();
        }
    }
}
