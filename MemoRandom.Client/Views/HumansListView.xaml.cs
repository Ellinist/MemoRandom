﻿using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumansView.xaml
    /// </summary>
    public partial class HumansListView : MetroWindow
    {
        private readonly HumansListViewModel _vm;

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

            this.Loaded += _vm.HumansListView_Loaded; // Событие открытия окна
            this.Closed += _vm.HumansListView_Closed; // Событие закрытия окна
            DgHumans.Sorting += _vm.DgHumans_Sorting; // Событие сортировки по столбцу
        }
        #endregion
    }
}