using MemoRandom.Client.ViewModels;
using System;
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
        /// Событие загрузки стартового окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartMemoRandomView_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.StartView_Loaded(sender, e);
        }

        ///// <summary>
        ///// Нажатие на кнопку вызова справочника причин смерти
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        ///// <exception cref="System.NotImplementedException"></exception>
        //private void ReasonsButton_Click1(object sender, RoutedEventArgs e)
        //{
        //    _vm.ReasonsButton_Click(sender, e);
        //}

        ///// <summary>
        ///// Нажатие на кнопку вызова основного окна работы
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void HumansButton_Click(object sender, RoutedEventArgs e)
        //{
        //    _vm.HumansButton_Click(sender, e);
        //}





        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="vm"></param>
        public StartMemoRandomView(StartMemoRandomViewModel vm)
        {
            _vm = vm;

            InitializeComponent();

            DataContext = vm;

            this.Loaded         += StartMemoRandomView_Loaded; // Загрузка стартового окна
            ReasonsButton.Click += _vm.ReasonsButton_Open; // Вызов окна справочника причин смерти
            HumansButton.Click  += _vm.HumansButton_Open; // Вызов основного окна работы

            if (vm.ButtonsVisibility == null)
            {
                // Делегат установки видимости кнопок после чтения справочника причин смерти
                vm.ButtonsVisibility = new Action(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ReasonsButton.Visibility = Visibility.Visible;
                        HumansButton.Visibility  = Visibility.Visible;
                    });
                });
            }
        }
        #endregion
    }
}
