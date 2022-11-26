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
using System.Threading.Tasks;

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

        private Thread ProgressThread { get; set; }

        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private CancellationToken token;

        /// <summary>
        /// Метод закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComparingProcessView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Dispose();

            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();

            e.Cancel = false; // Окно закрываем
        }

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
                //ProgressThread = new Thread(ProgressMethod);
                //ProgressThread.Start(data);

                Task task = new Task(() =>
                {
                    ProgressMethod(data);
                }, token);

                task.Start();
                
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

            var startSpan = DateTime.Now - data.BirthDate;
            var orderedList = CommonDataController.HumansList.OrderBy(x => x.DaysLived);
            var earlier = orderedList.LastOrDefault(x => x.DaysLived < startSpan.TotalDays);
            var later = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.TotalDays);

            while (!token.IsCancellationRequested)
            {
                var currentPos = DateTime.Now - data.BirthDate;
                var days = currentPos.Days;
                var years = days / 365;
                var hours = currentPos.Hours;
                var minutes = currentPos.Minutes;
                var seconds = currentPos.Seconds;
                var milliseconds = currentPos.Milliseconds;

                Thread.Sleep(1);
                ProgressDispatcher.Invoke(() =>
                {
                    if (earlier != null) control.LeftDownTb.Text = earlier.LastName;
                    if (later != null) control.RightDownTb.Text = later.LastName;

                    control.CenterDownTb.Text = ("Прожито:" + years + " лет, " + days + " дней, " + hours + ":" + minutes + ":" + seconds + "." + milliseconds).ToString();
                });
            }
        }


        #region CTOR
        public ComparingProcessViewModel()
        {
            token = cancelTokenSource.Token;
            ProgressDispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion
    }
}
