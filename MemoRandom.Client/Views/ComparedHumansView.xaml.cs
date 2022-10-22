﻿using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;
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
    /// Логика взаимодействия для ComparedHumansView.xaml
    /// </summary>
    public partial class ComparedHumansView : MetroWindow
    {
        public ComparedHumansView(ComparedHumansViewModel vm)
        {
            InitializeComponent();
            this.Loaded += vm.ComparedHumansView_Loaded;
            DataContext = vm;
        }
    }
}
