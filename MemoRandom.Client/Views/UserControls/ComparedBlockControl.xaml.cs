using MemoRandom.Client.Common.Models;
using MemoRandom.Client.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MemoRandom.Client.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ComparedBlockControl.xaml
    /// </summary>
    public partial class ComparedBlockControl : UserControl
    {
        public static DependencyProperty ComparedHumanFullNameProperty =
            DependencyProperty.Register("ComparedHuman", typeof(string), typeof(ComparedBlockControl), null);

        public string ComparedHumanFullName
        {
            get { return (string)GetValue(ComparedHumanFullNameProperty); }
            set { SetValue(ComparedHumanFullNameProperty, value); }
        }

        public ComparedBlockControl(/*ComparedBlockControlViewModel vm, ComparedHuman human, Dispatcher dispatcher*/)
        {
            InitializeComponent();
        }
    }
}
