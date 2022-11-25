using MemoRandom.Client.Common.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MemoRandom.Client.ViewModels
{
    public class ComparedBlockControlViewModel :BindableBase
    {
        public ProgressBar CurrentProgressBar { get; set; }
        public TextBlock LeftUpTextBlock { get; set; }
        public Human Human { get; set; }


        public void ComparedBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            var t = Human;
            LeftUpTextBlock.Text = "Тестовое";
            CurrentProgressBar.Minimum = 0;
            CurrentProgressBar.Maximum = 100;
            CurrentProgressBar.Value = 53;
        }

        #region CTOR
        public ComparedBlockControlViewModel()
        {

        }
        #endregion
    }
}
