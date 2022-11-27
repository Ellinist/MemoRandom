using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Views.UserControls;
using Prism.Mvvm;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MemoRandom.Client.Models;
using System;
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
            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();

            e.Cancel = false; // Окно закрываем
        }

        public void GetStackPanel(StackPanel panel)
        {
            ProgressStackPanel = panel;

            // Цикл по всем людям для сравнения
            foreach(var human in CommonDataController.ComparedHumansCollection.Where(x => x.IsComparedHumanConsidered == true))
            {
                ComparedBlockControl control = new();
                ComparedHumanProgressData data = new()
                {
                    ComparedHumanBar = control,
                    FullName = human.ComparedHumanFullName,
                    BirthDate = human.ComparedHumanBirthDate,
                    FullYearsLived = Math.Floor((DateTime.Now - human.ComparedHumanBirthDate).Days / 365.25)
                };

                ProgressStackPanel.Children.Add(control);

                // Каждый человек для сравнения в своем потоке
                Task task = new Task(() =>
                {
                    ProgressMethod(data);
                }, token); // Передаем токен остановки

                task.Start();
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

                if (earlier != null) // Если предыдущий игрок был найден
                {
                    var before = (DateTime.Now - (data.BirthDate + (earlier.DeathDate - earlier.BirthDate))).Days;
                    if (later != null)
                    {
                        var till = ((data.BirthDate + (later.DeathDate - later.BirthDate)) - DateTime.Now).Days;
                        control.CurrentProgressBar.Maximum = before + till;
                        control.CurrentProgressBar.Value = before;
                    }
                    else
                    {
                        control.CurrentProgressBar.Maximum = before;
                        control.CurrentProgressBar.Value = before;
                    }

                    control.PreviousHumanNameTextBlock.Text = earlier.LastName + " "
                                                            + (earlier.FirstName  != string.Empty ? (earlier.FirstName[0..1] + ".") : "") + " "
                                                            + (earlier.Patronymic != string.Empty ? (earlier.Patronymic[0..1] + ".") : "");
                    control.PreviousImage.Source = _commonDataController.GetHumanImage(earlier);
                    control.PreviousHumanBirthDateTextBlock.Text = "Рождение: " + earlier.BirthDate.ToLongDateString();

                    var deathReasonName = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == earlier.DeathReasonId);

                    control.PreviousHumanDeathDateTextBlock.Text = "Смерть: " +
                                                                   earlier.DeathDate.ToLongDateString() + " (" +
                                                                   deathReasonName.ReasonName + ")";
                    control.PreviousHumanFullYearsTextBlock.Text = "Прожил " + Math.Floor(earlier.FullYearsLived) + " лет";

                    control.PreviousHumanOverLifeDate.Text = "Пройдено: "
                                                           + (data.BirthDate + (earlier.DeathDate - earlier.BirthDate)).ToString("dd MMMM yyyy hh: mm");
                }
                else
                {
                    var before = (DateTime.Now - data.BirthDate).Days;
                    if (later != null)
                    {
                        var till = ((data.BirthDate + (later.DeathDate - later.BirthDate)) - DateTime.Now).Days;
                        control.CurrentProgressBar.Maximum = before + till;
                        control.CurrentProgressBar.Value = before;
                    }
                    else
                    {
                        control.CurrentProgressBar.Maximum = before;
                        control.CurrentProgressBar.Value = before;
                    }
                }

                if (later != null) // Если следующий игрок был найден
                {
                    control.NextHumanNameTextBlock.Text = later.LastName + " "
                                                        + (later.FirstName  != string.Empty ? (later.FirstName[0..1] + ".")  : "") + " "
                                                        + (later.Patronymic != string.Empty ? (later.Patronymic[0..1] + ".") : "");
                    control.NextImage.Source = _commonDataController.GetHumanImage(later);
                    control.NextHumanBirthDateTextBlock.Text = "Приход: " + later.BirthDate.ToLongDateString();

                    var deathReasonName = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == later.DeathReasonId);
                    control.NextHumanDeathDateTextBlock.Text = "(" + deathReasonName.ReasonName + ") " +
                                                               "Уход: " + later.DeathDate.ToLongDateString();
                    control.NextHumanFullYearsTextBlock.Text = "Прожил: " + Math.Floor(later.FullYearsLived) + " лет";

                    control.NextHumanOverLifeDate.Text = "Пройдем: "
                                                       + (data.BirthDate + (later.DeathDate - later.BirthDate)).ToString("dd MMMM yyyy hh: mm");
                }

                control.CurrentHumanTextBlock.Text = data.FullName;
                control.CurrentHumanDetailesTextBlock.Text = data.BirthDate.ToLongDateString();
            });

            while (!token.IsCancellationRequested)
            {
                var currentPos = DateTime.Now - data.BirthDate;
                var yy = data.FullYearsLived * 365.25;
                var years = currentPos.Days / 365 ;
                var days = Math.Floor(currentPos.TotalDays - yy);
                var hours = currentPos.Hours;
                var minutes = currentPos.Minutes;
                var seconds = currentPos.Seconds;

                Thread.Sleep(1000);
                ProgressDispatcher.Invoke(() =>
                {
                    if (earlier != null)
                    {
                        // Пройденный период
                        var spent = DateTime.Now - (data.BirthDate + (earlier.DeathDate - earlier.BirthDate));
                        var spentDays = Math.Floor(spent.TotalDays);
                        var spentHours = spent.Hours;
                        var spentMinutes = spent.Minutes;
                        var spentSeconds = spent.Seconds;

                        control.SpentDaysFromPreviousHuman.Text = "Прошло: " +
                                                                  spentDays.ToString() + " дней" + " " +
                                                                  spentHours.ToString() + " часов, " +
                                                                  spentMinutes.ToString() + ":" +
                                                                  spentSeconds.ToString();
                    }

                    if (later != null)
                    {
                        // Оставшийся период
                        var spent = (data.BirthDate + (later.DeathDate - later.BirthDate)) - DateTime.Now;
                        var spentDays = Math.Floor(spent.TotalDays);
                        var spentHours = spent.Hours;
                        var spentMinutes = spent.Minutes;
                        var spentSeconds = spent.Seconds;

                        control.RestDaysToNextHuman.Text = "Осталось: " +
                                                           spentDays.ToString() + " дней" + " " +
                                                           spentHours.ToString() + " часов, " +
                                                           spentMinutes.ToString() + ":" +
                                                           spentSeconds.ToString();
                    }
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
