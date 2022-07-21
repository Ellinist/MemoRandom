using MemoRandom.Client.ViewModels;
using System.Windows;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для StartMemoRandom.xaml
    /// </summary>
    public partial class StartMemoRandomView
    {
        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Activate();
        }

        public StartMemoRandomView(StartMemoRandomViewModel vm)
        {
            InitializeComponent();

            if(vm.ButtonsVisibility == null)
            {
                // Делегат установки видимости кнопок после чтения справочника причин смерти
                vm.ButtonsVisibility = new System.Action(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ReasonsButton.Visibility = Visibility.Visible;
                        HumansButton.Visibility  = Visibility.Visible;
                    });
                });
            }

            DataContext = vm;
        }
    }
}
