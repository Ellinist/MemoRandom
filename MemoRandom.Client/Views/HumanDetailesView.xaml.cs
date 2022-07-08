using System.Windows;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumanDetailesView.xaml
    /// </summary>
    public partial class HumanDetailesView : Window
    {
        public HumanDetailesView(HumanDetailesViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
