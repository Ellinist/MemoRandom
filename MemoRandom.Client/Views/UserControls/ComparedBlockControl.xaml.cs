using MemoRandom.Client.Common.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MemoRandom.Client.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ComparedBlockControl.xaml
    /// </summary>
    public partial class ComparedBlockControl : UserControl
    {
        public ComparedBlockControl(ComparedBlockControlViewModel vm, Human human)
        {
            InitializeComponent();

            DataContext = vm;
            vm.LeftUpTextBlock = LeftUpTb;
            vm.CurrentProgressBar = CurrentProgressBar;
            vm.Human = human;
            this.Loaded += vm.ComparedBlockControl_Loaded;
        }
    }
}
