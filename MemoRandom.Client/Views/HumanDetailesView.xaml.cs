using System.Windows;
using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumanDetailesView.xaml
    /// </summary>
    public partial class HumanDetailesView /*: MetroWindow*/
    {
        public HumanDetailesView(HumanDetailesViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
