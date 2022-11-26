using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Views.UserControls;
using Prism.Mvvm;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MemoRandom.Client.Models;
using System;
using ImTools;
using System.Linq;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для хранения UC с прогрессом
    /// </summary>
    public class ComparingProcessViewModel : BindableBase
    {
        public Dispatcher ProgressDispatcher { get; set; }

        public Window ProgressView { get; set; }

        public StackPanel ProgressStackPanel { get; set; }

        public void GetStackPanel(StackPanel panel)
        {
            ProgressStackPanel = panel;

            // Цикл по всем людям для сравнения
            foreach(var human in CommonDataController.ComparedHumansCollection)
            {
                ComparedBlockControl control = new();
                ComparedHumanProgressData data = new()
                {
                    ComparedHumanBar = control,
                    FullName = human.ComparedHumanFullName,
                    BirthDate = human.ComparedHumanBirthDate
                };

                ProgressStackPanel.Children.Add(control);

                // Каждый человек для сравнения в своем потоке
                new Thread(ProgressMethod).Start(data);

                //Thread thread = new Thread(ProgressMethod);
                //thread.Start(control);
                ////*Thread thread = */new Thread(() => PrMethod(human.ComparedHumanFullName, control)).Start();
            }
        }

        private void ProgressMethod(object paramsData)
        {
            ComparedHumanProgressData data = paramsData as ComparedHumanProgressData;
            if (data == null) return;

            var control = data.ComparedHumanBar;

            ProgressDispatcher.Invoke(() =>
            {
                control.CurrentProgressBar.Minimum = 0;
                control.CurrentProgressBar.Maximum = 1000;
                control.CurrentProgressBar.Value = 0;

                control.CenterUpTb.Text = data.FullName;
                control.LeftUpTb.Text = data.BirthDate.ToLongDateString();
            });

            //for (var i = 0; i < 1000; i++)
            //{
            //    Thread.Sleep(3);
            //    ProgressDispatcher.Invoke(() =>
            //    {
            //        control.CurrentProgressBar.Value = i;
            //    });
            //}

            var startSpan = DateTime.Now - data.BirthDate;
            var orderedList = CommonDataController.HumansList.OrderBy(x => x.DaysLived);
            var earlier = orderedList.LastOrDefault(x => x.DaysLived < startSpan.TotalDays);
            var later = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.TotalDays);

            while (true)
            {
                var currentPos = DateTime.Now - data.BirthDate;
                var days = currentPos.Days;
                var years = days / 365;
                var hours = currentPos.Hours;
                var minutes = currentPos.Minutes;
                var seconds = currentPos.Seconds;
                var milliseconds = currentPos.Milliseconds;
                //var currentPos = DateTime.Now - data.BirthDate;
                //var years = currentPos.TotalDays / 365;
                //var days = currentPos.TotalDays;
                //var hours = currentPos.TotalHours;
                Thread.Sleep(1);
                ProgressDispatcher.Invoke(() =>
                {
                    if(earlier != null) control.LeftDownTb.Text = earlier.LastName;
                    if(later != null) control.RightDownTb.Text = later.LastName;

                    control.CenterDownTb.Text = ("Прожито:" + years + " лет, " + days + " дней, " + hours + ":" + minutes + ":" + seconds + "." + milliseconds).ToString();
                });
            }
        }


        #region CTOR
        public ComparingProcessViewModel()
        {
            ProgressDispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion
    }
}
