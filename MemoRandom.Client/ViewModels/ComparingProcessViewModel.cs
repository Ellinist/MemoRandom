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
using System.Windows.Media.Imaging;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Models;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для хранения UC с прогрессом
    /// </summary>
    public class ComparingProcessViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private readonly ICommonDataController _commonDataController;
        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private CancellationToken token; // Токен для остановки потока
        #endregion

        #region PROPS
        /// <summary>
        /// Графический поток отображаемых элементов
        /// </summary>
        public Dispatcher ProgressDispatcher { get; set; }
        #endregion


        public Window ProgressView { get; set; }

        public StackPanel ProgressStackPanel { get; set; }

        private BitmapSource LeftPicture { get; set; }
        private BitmapSource RightPicture { get; set; }

        private Thread ProgressThread { get; set; }

        

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

            var orderedComparedHumansList =
                CommonDataController.ComparedHumansCollection.Where(x => x.IsComparedHumanConsidered == true);
            // Цикл по всем людям для сравнения
            foreach (var human in orderedComparedHumansList)
            {
                ComparedBlockControl control = new();
                ComparedHumanProgressData comparedHumanData = new()
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
                    ProgressMethod(comparedHumanData);
                }, token); // Передаем токен остановки

                task.Start();
            }
        }

        private void ProgressMethod(object paramsData)
        {
            // Получаем основные данные по человеку для сравнения
            var comparedHumanData = paramsData as ComparedHumanProgressData;
            if (comparedHumanData == null) return; // Если таких данных нет, то выходим - ситуация нереальная

            var control = comparedHumanData.ComparedHumanBar; // Получаем User Control для текущего анализируемого человека для сравнения

            #region Блок вывода данных для анализируемого человека
            // Выводим данные в потоке UI основного окна
            ProgressDispatcher.Invoke(() =>
            {
                control.CurrentHumanTextBlock.Text = comparedHumanData.FullName; // Выводим полное ФИО
                control.CurrentHumanDetailesTextBlock.Text = comparedHumanData.BirthDate.ToLongDateString(); // Выводим дату рождения
                control.CurrentProgressBar.Minimum = 0; // Начальная точка для прогресс-бара
                // Вышеуказанные данные не будут меняться
            });
            #endregion

            // ВНИМАНИЕ! Все остальные данные будут меняться и зависят от текущего времени

            // Но! Прежде вычисляем стартовые позиции
            var startSpan = DateTime.Now - comparedHumanData.BirthDate; // Стартовый диапазон анализируемого человека
            // Запускаем основной цикл отображения изменяющихся данных (зависят от текущего времени)
            while (!token.IsCancellationRequested) // Пока команда для остановки потока не придет, выполняем работу потока
            {
                Thread.Sleep(100); // Для облегчения работы программы замораживаем на 100 мс
                ProgressDispatcher.Invoke(() => // Весь вывод идет в потоке UI пользовательского интерфейса
                {
                    control.CurrentHumanLivedPeriod.Text = GetWastedTime(comparedHumanData); // Выводим бесцельно потраченное время жизни анализируемого человека
                });
            }
            
            //var startSpan = DateTime.Now - data.BirthDate;
            //var orderedList = CommonDataController.HumansList.OrderBy(x => x.FullYearsLived);
            //var earlier = orderedList.LastOrDefault(x => x.DaysLived < startSpan.TotalDays);
            //var later = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.TotalDays);

            //// Вывод на экран параметров при старте окна
            //ProgressDispatcher.Invoke(() =>
            //{
            //    control.CurrentProgressBar.Minimum = 0;

            //    if (earlier != null) // Если предыдущий игрок был найден
            //    {
            //        LeftPicture = _commonDataController.GetHumanImage(earlier);

            //        var before = (DateTime.Now - (data.BirthDate + (earlier.DeathDate - earlier.BirthDate))).Days;
            //        if (later != null)
            //        {
            //            var till = ((data.BirthDate + (later.DeathDate - later.BirthDate)) - DateTime.Now).Days;
            //            control.CurrentProgressBar.Maximum = before + till;
            //            control.CurrentProgressBar.Value = before;
            //        }
            //        else
            //        {
            //            control.CurrentProgressBar.Maximum = before;
            //            control.CurrentProgressBar.Value = before;
            //        }

            //        control.PreviousHumanNameTextBlock.Text = earlier.LastName + " "
            //                                                + (earlier.FirstName  != string.Empty ? (earlier.FirstName[0..1] + ".") : "") + " "
            //                                                + (earlier.Patronymic != string.Empty ? (earlier.Patronymic[0..1] + ".") : "");
            //        control.PreviousImage.Source = LeftPicture;
            //        control.PreviousHumanBirthDateTextBlock.Text = "Рождение: " + earlier.BirthDate.ToLongDateString();

            //        var deathReasonName = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == earlier.DeathReasonId);

            //        control.PreviousHumanDeathDateTextBlock.Text = "Смерть: " +
            //                                                       earlier.DeathDate.ToLongDateString() + " (" +
            //                                                       deathReasonName.ReasonName + ")";
            //        control.PreviousHumanFullYearsTextBlock.Text = "Прожил " + Math.Floor(earlier.FullYearsLived) + " лет";

            //        control.PreviousHumanOverLifeDate.Text = "Пройдено: "
            //                                               + (data.BirthDate + (earlier.DeathDate - earlier.BirthDate)).ToString("dd MMMM yyyy hh: mm");
            //    }
            //    else
            //    {
            //        var before = (DateTime.Now - data.BirthDate).Days;
            //        if (later != null)
            //        {
            //            var till = ((data.BirthDate + (later.DeathDate - later.BirthDate)) - DateTime.Now).Days;
            //            control.CurrentProgressBar.Maximum = before + till;
            //            control.CurrentProgressBar.Value = before;
            //        }
            //        else
            //        {
            //            control.CurrentProgressBar.Maximum = before;
            //            control.CurrentProgressBar.Value = before;
            //        }
            //    }

            //    if (later != null) // Если следующий игрок был найден
            //    {
            //        RightPicture = _commonDataController.GetHumanImage(later);

            //        control.NextHumanNameTextBlock.Text = later.LastName + " "
            //                                            + (later.FirstName  != string.Empty ? (later.FirstName[0..1] + ".")  : "") + " "
            //                                            + (later.Patronymic != string.Empty ? (later.Patronymic[0..1] + ".") : "");
            //        control.NextImage.Source = RightPicture;
            //        control.NextHumanBirthDateTextBlock.Text = "Приход: " + later.BirthDate.ToLongDateString();

            //        var deathReasonName = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == later.DeathReasonId);
            //        control.NextHumanDeathDateTextBlock.Text = "(" + deathReasonName.ReasonName + ") " +
            //                                                   "Уход: " + later.DeathDate.ToLongDateString();
            //        control.NextHumanFullYearsTextBlock.Text = "Прожил: " + Math.Floor(later.FullYearsLived) + " лет";

            //        control.NextHumanOverLifeDate.Text = "Пройдем: "
            //                                           + (data.BirthDate + (later.DeathDate - later.BirthDate)).ToString("dd MMMM yyyy hh: mm");
            //    }
            //});

            //while (!token.IsCancellationRequested)
            //{
            //    Thread.Sleep(1000);
            //    ProgressDispatcher.Invoke(() =>
            //    {
            //        if (earlier != null)
            //        {
            //            // Пройденный период
            //            var spent = DateTime.Now - (data.BirthDate + (earlier.DeathDate - earlier.BirthDate));
            //            var spentDays = Math.Floor(spent.TotalDays);
            //            var spentHours = spent.Hours;
            //            var spentMinutes = spent.Minutes;
            //            var spentSeconds = spent.Seconds;

            //            control.SpentDaysFromPreviousHuman.Text = "Прошло: " +
            //                                                      spentDays.ToString() + " дней" + " " +
            //                                                      spentHours.ToString() + " часов, " +
            //                                                      spentMinutes.ToString() + ":" +
            //                                                      spentSeconds.ToString();
            //        }

            //        if (later != null)
            //        {
            //            // Оставшийся период
            //            var spent = (data.BirthDate + (later.DeathDate - later.BirthDate)) - DateTime.Now;
            //            var spentDays = Math.Floor(spent.TotalDays);
            //            var spentHours = spent.Hours;
            //            var spentMinutes = spent.Minutes;
            //            var spentSeconds = spent.Seconds;

            //            control.RestDaysToNextHuman.Text = "Осталось: " +
            //                                               spentDays.ToString() + " дней" + " " +
            //                                               spentHours.ToString() + " часов, " +
            //                                               spentMinutes.ToString() + ":" +
            //                                               spentSeconds.ToString();
            //        }
            //    });
            //}
        }

        /// <summary>
        /// Метод получения строки впустую потраченного времени своей никчемной жизни
        /// </summary>
        /// <returns></returns>
        private string GetWastedTime(ComparedHumanProgressData comparedHumanData)
        {
            var currentPos = DateTime.Now - comparedHumanData.BirthDate;
            var years = currentPos.Days / 365;
            var days = (int)Math.Floor(currentPos.TotalDays - yy);
            var time = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", currentPos.Hours, currentPos.Minutes, currentPos.Seconds, currentPos.Milliseconds);

            string resultString = "Исчезло: " +
                                  years + " " + SetRightYears(currentPos.Days / 365) + ", " +
                                  days + " " + SetRightDays(days) + ", " +
                                  time;

            return resultString;
        }

        /// <summary>
        /// Формирование текстов для отображения прожитых лет в соответствии с числом
        /// </summary>
        /// <param name="years"></param>
        private string SetRightYears(int years)
        {
            string result = "";
            int t1, t2;
            t1 = years % 10;
            t2 = years % 100;
            if (t1 == 1 && t2 != 11)
            {
                result = "год";
            }
            else if (t1 >= 2 && t1 <= 4 && (t2 < 10 || t2 >= 20))
            {
                result = "года";
            }
            else
            {
                result = "лет";
            }

            return result;
        }

        /// <summary>
        /// Формирование текстов для отображения прожитых лет в соответствии с числом
        /// </summary>
        /// <param name="days"></param>
        private string SetRightDays(int days)
        {
            string result = "";
            int t1, t2;
            t1 = days % 10;
            t2 = days % 100;
            if (t1 == 1 && t2 != 11)
            {
                result = "день";
            }
            else if (t1 >= 2 && t1 <= 4 && (t2 < 10 || t2 >= 20))
            {
                result = "дня";
            }
            else
            {
                result = "дней";
            }

            return result;
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
