using System.Windows;
using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumanDetailesView.xaml
    /// </summary>
    public partial class HumanDetailedView /*: MetroWindow*/
    {
        public HumanDetailedView(HumanDetailedViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
