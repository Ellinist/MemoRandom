using MemoRandom.Client.Common.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MemoRandom.Client.ViewModels
{
    public class ComparedBlockControlViewModel :BindableBase
    {
        public Dispatcher ProgressDispatcher { get; set; }
        public ProgressBar CurrentProgressBar { get; set; }
        public TextBlock LeftUpTextBlock { get; set; }
        public TextBlock CenterUpTextBlock { get; set; }
        public ComparedHuman ComparedHuman { get; set; }


        public void ComparedBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            CenterUpTextBlock.Text = ComparedHuman.ComparedHumanFullName;

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                ProgressDispatcher.Invoke(() =>
                {
                    CurrentProgressBar.Value = i;
                    //Thread.Sleep(10);
                    //CurrentProgressBar.Value = i;
                });
                //CurrentProgressBar.Value = i;
            }

            //var t = ComparedHuman;

            //ProgressDispatcher.Invoke(() =>
            //{
            //    LeftUpTextBlock.Text = "Тестовое";
            //    CenterUpTextBlock.Text = ComparedHuman.ComparedHumanFullName;
            //    CurrentProgressBar.Minimum = 0;
            //    CurrentProgressBar.Maximum = 1000;
            //});

            //for (int i = 0; i < 1000; i++)
            //{
            //    Thread.Sleep(500);

            //    ProgressDispatcher.Invoke(() =>
            //    {
            //        CurrentProgressBar.Value = i;
            //    });
            //}
        }

        #region CTOR
        public ComparedBlockControlViewModel()
        {

        }
        #endregion
    }
}
