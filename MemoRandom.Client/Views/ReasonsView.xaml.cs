using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ReasonsView.xaml
    /// </summary>
    public partial class ReasonsView : MetroWindow
    {
        private readonly ReasonsViewModel _vm; // Модель представления справочника причин смерти

        /// <summary>
        /// Загрузка окна справочника причин смерти
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReasonsDictionary_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.ReasonsDictionary_Loaded(sender, e);
        }



        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="vm"></param>
        public ReasonsView(ReasonsViewModel vm)
        {
            _vm = vm;

            InitializeComponent();
            DataContext = vm;
        }
        #endregion
    }
}
