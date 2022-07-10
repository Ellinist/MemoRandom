using MemoRandom.Client.ViewModels;
using System.Windows;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {
        public AboutView(AboutViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
