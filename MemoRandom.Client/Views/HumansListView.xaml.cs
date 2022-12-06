using MahApps.Metro.Controls;
using MemoRandom.Client.Common.Models;
using MemoRandom.Client.ViewModels;
using ScottPlot;
using System;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumansView.xaml
    /// </summary>
    public partial class HumansListView : MetroWindow
    {
        private readonly HumansListViewModel _vm;

        /// <summary>
        /// Метод позиционирования на добавленной или отредактированной записи в DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgHumans_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(DgHumans.SelectedItem != null)
            {
                DgHumans.UpdateLayout();
                DgHumans.ScrollIntoView(DgHumans.SelectedItem, null);
                DgHumans.Focus();
            }
        }

        /// <summary>
        /// Позиционирование фокуса на требуемой записи
        /// </summary>
        /// <param name="human"></param>
        private void SetCurrentRecord(Human human)
        {
            DgHumans.SelectedItem = human;
        }

        /// <summary>
        /// Метод отработки при загрузке окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HumansListView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.HumansListView_Loaded(HumansChart);
        }



        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="vm"></param>
        public HumansListView(HumansListViewModel vm)
        {
            _vm = vm;

            InitializeComponent();

            DataContext = vm;

            this.Loaded += HumansListView_Loaded;
            this.Closed += _vm.HumansListView_Closed; // Событие закрытия окна
            DgHumans.Sorting += _vm.DgHumans_Sorting; // Событие сортировки по столбцу
            DgHumans.SelectionChanged += DgHumans_SelectionChanged;
            DgHumans.MouseDoubleClick += _vm.DgHumans_MouseDoubleClick;

            _vm.SetCurrentRecordEvent += SetCurrentRecord;
        }
        #endregion
    }
}