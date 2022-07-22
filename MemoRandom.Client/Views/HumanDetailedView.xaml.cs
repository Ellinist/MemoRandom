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

            //Closing += vm.HumanDetailedView_Closing; // Подписка на событие закрытия окна
            if(vm.CloseAction == null)
            {
                vm.CloseAction = new System.Action(this.Close);
            }

            PersonImage.MouseLeftButtonDown += vm.PersonImage_MouseLeftButtonDown;
            PersonImage.MouseLeftButtonUp += vm.PersonImage_MouseLeftButtonUp;
            PersonImage.MouseMove += vm.PersonImage_MouseMove;
            PersonImage.MouseWheel += vm.PersonImage_MouseWheel;

            DataContext = vm;
        }
    }
}
