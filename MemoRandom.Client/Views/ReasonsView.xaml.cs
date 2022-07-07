using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ReasonsView.xaml
    /// </summary>
    public partial class ReasonsView : MetroWindow
    {
        public ReasonsView(ReasonsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
