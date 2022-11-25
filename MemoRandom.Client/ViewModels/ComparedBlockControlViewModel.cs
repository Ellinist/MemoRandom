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
        public Human Human { get; set; }


        public void ComparedBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            var t = Human;

            LeftUpTextBlock.Text = "Тестовое";
            CurrentProgressBar.Minimum = 0;
            CurrentProgressBar.Maximum = 1000;

            Thread thread = new Thread(ProgressMethod);
            thread.Start();

            //ProgressDispatcher.BeginInvoke(() =>
            //{
            //    LeftUpTextBlock.Text = "Тестовое";
            //    CurrentProgressBar.Minimum = 0;
            //    CurrentProgressBar.Maximum = 1000;
            //    CurrentProgressBar.Value = 53;
            //});
            

            
            //ProgressDispatcher.Invoke(() =>
            //{

            //});

            //for(int i = 0; i < 1000; i++)
            //{
            //    Thread.Sleep(100);
            //    ProgressDispatcher.Invoke(() =>
            //    {
            //        CurrentProgressBar.Value = i;
            //    });
            //}
        }

        private void ProgressMethod()
        {
            for (int i = 0; i < 1000; i++)
            {
                //for (var j = 0; j < 1_000_000; j++)
                //{
                //    Math.Sqrt(15489.12);
                //}

                Thread.Sleep(1000);

                ProgressDispatcher.Invoke(() =>
                {
                    CurrentProgressBar.Value = i;
                });
            }
        }

        #region CTOR
        public ComparedBlockControlViewModel()
        {

        }
        #endregion
    }
}
