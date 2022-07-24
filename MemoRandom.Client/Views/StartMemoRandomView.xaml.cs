using MemoRandom.Client.ViewModels;
using System.Windows;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для StartMemoRandom.xaml
    /// </summary>
    public partial class StartMemoRandomView
    {
        private readonly StartMemoRandomViewModel _vm; // Модель представления стартового окна

        /// <summary>
        /// Событие загрузки окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartView_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.StartView_Loaded(sender, e);
        }

        /// <summary>
        /// Событие нажатия на кнопку открытия окна справочника причин смерти
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReasonsButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.ReasonsButton_Click(sender, e);
        }

        /// <summary>
        /// Событие нажатия на кнопку открытия окна со списком людей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HumansButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.HumansButton_Click(sender, e);
        }





        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="vm"></param>
        public StartMemoRandomView(StartMemoRandomViewModel vm)
        {
            _vm = vm;

            InitializeComponent();

            if (vm.ButtonsVisibility == null)
            {
                // Делегат установки видимости кнопок после чтения справочника причин смерти
                vm.ButtonsVisibility = new System.Action(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ReasonsButton.Visibility = Visibility.Visible;
                        HumansButton.Visibility = Visibility.Visible;
                    });
                });
            }

            DataContext = vm;
        }
        #endregion
    }
}
