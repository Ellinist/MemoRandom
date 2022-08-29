﻿using MemoRandom.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для CategoriesView.xaml
    /// </summary>
    public partial class CategoriesView : Window
    {
        public CategoriesView(CategoriesViewModel vm)
        {
            InitializeComponent();

            this.Loaded += vm.CategoriesView_Loaded;

            CategoriesCombo.ItemsSource = typeof(Colors).GetProperties();
            DataContext = vm;
        }
    }
}
