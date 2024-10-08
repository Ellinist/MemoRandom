﻿using System;
using System.Windows;
using MahApps.Metro.Controls;
using MemoRandom.Client.Common.Enums;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumanDetailedView.xaml
    /// </summary>
    public partial class HumanDetailedView /*: MetroWindow*/
    {
        private readonly HumanDetailedViewModel _vm; // Модель представления окна редактирования/добавления человека

        /// <summary>
        /// Загрузка окна редактирования/добавления человека
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailedView_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.DetailedView_Loaded(sender, e);
        }


        #region CTOR
        public HumanDetailedView(HumanDetailedViewModel vm)
        {
            _vm = vm;

            InitializeComponent();

            if (vm.CloseAction == null)
            {
                vm.CloseAction = new System.Action(this.Close);
            }

            PersonImage.MouseLeftButtonDown += vm.PersonImage_MouseLeftButtonDown;
            PersonImage.MouseMove += vm.PersonImage_MouseMove;
            SourceCanvas.MouseWheel += vm.SourceCanvas_MouseWheel;

            DataContext = vm;

            HumanSexCombo.ItemsSource = Enum.GetValues(typeof(SexEnum));
        }
        #endregion
    }
}
