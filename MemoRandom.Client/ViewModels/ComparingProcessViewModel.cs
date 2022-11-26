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
using MemoRandom.Client.Common.Interfaces;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для хранения UC с прогрессом
    /// </summary>
    public class ComparingProcessViewModel : BindableBase
    {
        private readonly ICommonDataController _commonDataController;

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

            var startSpan = DateTime.Now - data.BirthDate;
            var orderedList = CommonDataController.HumansList.OrderBy(x => x.DaysLived);
            var earlier = orderedList.LastOrDefault(x => x.DaysLived < startSpan.Days);
            var later = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.Days);

            ProgressDispatcher.Invoke(() =>
            {
                control.CurrentProgressBar.Minimum = 0;
                control.CurrentProgressBar.Maximum = 1000;
                control.CurrentProgressBar.Value = 0;

                if (earlier != null) // Если предыдущий игрок был найден
                {
                    control.PreviousHumanNameTextBlock.Text = earlier.LastName + " "
                                                            + (earlier.FirstName  != string.Empty ? (earlier.FirstName[0..1] + ".") : "") + " "
                                                            + (earlier.Patronymic != string.Empty ? (earlier.Patronymic[0..1] + ".") : "");
                    control.PreviousImage.Source = _commonDataController.GetHumanImage(earlier);
                    control.PreviousHumanBirthDateTextBlock.Text = "Родился: " + earlier.BirthDate.ToLongDateString();
                    control.PreviousHumanDeathDateTextBlock.Text = "Умер: " + earlier.DeathDate.ToLongDateString();
                    control.PreviousHumanFullYearsTextBlock.Text = "Прожил " + Math.Floor(earlier.FullYearsLived) + " лет";

                    var resulto = data.BirthDate + (earlier.DeathDate - earlier.BirthDate);
                    control.PreviousHumanOverLifeDate.Text = "Пройдено: " + resulto.ToString();
                }
                if (later != null) // Если следующий игрок был найден
                {
                    control.NextHumanNameTextBlock.Text = later.LastName + " "
                                                        + (later.FirstName  != string.Empty ? (later.FirstName[0..1] + ".")  : "") + " "
                                                        + (later.Patronymic != string.Empty ? (later.Patronymic[0..1] + ".") : "");
                    control.NextImage.Source = _commonDataController.GetHumanImage(later);
                    control.NextHumanBirthDateTextBlock.Text = "Родился: " + later.BirthDate.ToLongDateString();
                    control.NextHumanDeathDateTextBlock.Text = "Умер: " + later.DeathDate.ToLongDateString();
                    control.NextHumanFullYearsTextBlock.Text = "Прожил: " + Math.Floor(later.FullYearsLived) + " лет";

                    var resulto = data.BirthDate + (later.DeathDate - later.BirthDate);
                    control.NextHumanOverLifeDate.Text = "Пройдем: " + resulto.ToString();
                }

                control.CurrentHumanTextBlock.Text = data.FullName;
                control.CurrentHumanDetailesTextBlock.Text = data.BirthDate.ToLongDateString();
            });

            while (!token.IsCancellationRequested)
            {
                var currentPos = DateTime.Now - data.BirthDate;
                var days = currentPos.Days;
                var years = days / 365;
                var hours = currentPos.Hours;
                var minutes = currentPos.Minutes;
                var seconds = currentPos.Seconds;
                //var milliseconds = currentPos.Milliseconds;

                Thread.Sleep(10);
                ProgressDispatcher.Invoke(() =>
                {
                    control.CurrentHumanLivedPeriod.Text = ("Прожито: " + years + " лет, " + days + " дней, " + hours + ":" + minutes + ":" + seconds/* + "." + milliseconds*/).ToString();
                });
            }
        }


        #region CTOR
        public ComparingProcessViewModel(ICommonDataController commonDataController)
        {
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(Common));

            token = cancelTokenSource.Token;
            ProgressDispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion
    }
}
