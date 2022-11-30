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
using MemoRandom.Client.Common.Enums;
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

        /// <summary>
        /// Формирование прогресс-индикаторов и потоков внутри
        /// </summary>
        /// <param name="panel"></param>
        public void SetStackPanel(StackPanel panel)
        {
            var orderedComparedHumansList = CommonDataController.ComparedHumansCollection.Where(x => x.IsComparedHumanConsidered == true);
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

                panel.Children.Add(control);

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
            // Объявляем переменные, ссылающиеся на картинки
            // Недопустимо выносить их в свойства класса, так как они обязаны существовать в рамках своего потока
            BitmapSource leftPicture  = null;
            BitmapSource rightPicture = null;

            // Получаем основные данные по человеку для сравнения
            if (paramsData is not ComparedHumanProgressData comparedHumanData) return; // Если таких данных нет, то выходим - ситуация нереальная

            var control = comparedHumanData.ComparedHumanBar; // Получаем User Control для текущего анализируемого человека для сравнения

            #region Блок вывода данных для анализируемого человека
            // Выводим данные в потоке UI основного окна
            ProgressDispatcher.Invoke(() =>
            {
                control.CurrentHumanTextBlock.Text = comparedHumanData.FullName; // Выводим полное ФИО
                control.CurrentHumanDetailesTextBlock.Text = "Родился: " + comparedHumanData.BirthDate.ToLongDateString(); // Выводим дату рождения
                control.CurrentProgressBar.Minimum = 0; // Начальная точка для прогресс-индикатора
                // Вышеуказанные данные не будут меняться
            });
            #endregion

            // ВНИМАНИЕ! Все остальные данные будут меняться и зависят от текущего времени
            // Но! Прежде вычисляем стартовые позиции и вспомогательные данные
            var orderedList = CommonDataController.HumansList.OrderBy(x => x.FullYearsLived).ToList(); // Упорядоченный по возрасту список людей

            var startSpan = DateTime.Now - comparedHumanData.BirthDate; // Стартовый диапазон анализируемого человека

            // Получаем информацию о пережитом (если есть) и не пережитом (если есть) человеке - в процессе работы может поменяться
            var previousHuman = orderedList.LastOrDefault(x => x.DaysLived < startSpan.TotalDays);  // Пережитый
            var nextHuman     = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.TotalDays); // Не пережитый

            ProgressDispatcher.Invoke(() => // Начальный вывод картинок пережитого и не пережитого игроков
            {
                if (previousHuman != null) // Если пережитый игрок существует - отображаем его картинку
                {
                    control.PreviousImage.Source = _commonDataController.GetHumanImage(previousHuman); // Загружаем картинку пережитого игрока
                }
                if (nextHuman != null)
                {
                    control.NextImage.Source = _commonDataController.GetHumanImage(nextHuman); // Загружаем картинку еще не пережитого игрока
                }
            });

            // Запускаем основной цикл отображения изменяющихся данных (зависят от текущего времени)
            while (!token.IsCancellationRequested) // Пока команда для остановки потока не придет, выполняем работу потока
            {
                // Весь бесконечный цикл проходим в потоке UI
                ProgressDispatcher.Invoke(() =>
                {
                    //if (previousHuman != null) // Если пережитый игрок существует - отображаем его картинку
                    //{
                    //    control.PreviousImage.Source = _commonDataController.GetHumanImage(previousHuman); // Загружаем картинку в правильном потоке
                    //}

                    //if (nextHuman != null)
                    //{
                    //    control.NextImage.Source = _commonDataController.GetHumanImage(nextHuman);
                    //}

                    MainProcess(control, previousHuman, nextHuman, comparedHumanData, /*startSpan, orderedList, */leftPicture, rightPicture, DateTime.Now);
                });

                Thread.Sleep(100); // Остановка потока для уменьшения нагрузки на программу
            }
        }

        /// <summary>
        /// Основной метод вывода в асинхронном режиме информации для сравнения
        /// </summary>
        /// <param name="control"></param>
        /// <param name="earlier"></param>
        /// <param name="later"></param>
        /// <param name="comparedHumanData"></param>
        /// <param name="startSpan"></param>
        /// <param name="orderedList"></param>
        /// <param name="LeftPicture"></param>
        /// <param name="RightPicture"></param>
        /// <param name="currentDateTime"></param>
        private void MainProcess(ComparedBlockControl control,
                                 Human earlier, Human later,
                                 ComparedHumanProgressData comparedHumanData,
                                 //TimeSpan startSpan,
                                 //IOrderedEnumerable<Human> orderedList,
                                 BitmapSource LeftPicture, BitmapSource RightPicture,
                                 DateTime currentDateTime)
        {
            //startSpan = currentDateTime - comparedHumanData.BirthDate; // Обновляемый диапазон анализируемого человека

            ////Получаем информацию о пережитом (если есть) и не пережитом (если есть) человеке - в процессе работы может поменяться
            //earlier = orderedList.LastOrDefault(x => x.DaysLived < startSpan.TotalDays);  // Пережитый
            //later = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.TotalDays); // Не пережитый

            //ProgressDispatcher.Invoke(() => // Весь вывод идет в потоке UI пользовательского интерфейса
            //{
                control.CurrentHumanLivedPeriod.Text = GetWastedTime(comparedHumanData, currentDateTime); // Выводим бесцельно потраченное время жизни анализируемого человека

                control.CurrentProgressBar.Minimum = 0; // Задаем стартовое значение прогресс-индикатора (всегда с нуля)

                if (earlier != null) // Если предыдущий игрок был найден
                {
                //// Если левая картинка нулевая - не выводим, если левая картинка не меняется - не выводим
                //if (control.PreviousImage.Source != LeftPicture && LeftPicture != null) control.PreviousImage.Source = _commonDataController.GetHumanImage(earlier);

                // Вычисляем время, прошедшее с момента ухода пережитого
                var before = (currentDateTime - (comparedHumanData.BirthDate + (earlier.DeathDate - earlier.BirthDate))).Days;

                    if (later != null) // Если не пережитый игрок существует
                    {
                        //// Если правая картинка нулевая - не выводим, если правая картинка не меняется - не выводим
                        //if (control.NextImage.Source != RightPicture && RightPicture != null) control.NextImage.Source = _commonDataController.GetHumanImage(later);

                        // Вычисляем время до момента его ухода
                        var till = ((comparedHumanData.BirthDate + (later.DeathDate - later.BirthDate)) - currentDateTime).Days;

                        // Выводим информацию (ФИО) о еще не пережитом игроке
                        control.NextHumanNameTextBlock.Text = later.LastName + " "
                                                            + (later.FirstName != string.Empty ? (later.FirstName[0..1] + ".") : "") + " "
                                                            + (later.Patronymic != string.Empty ? (later.Patronymic[0..1] + ".") : "");

                        // Выводим дату рождения еще не пережитого игрока
                        control.NextHumanBirthDateTextBlock.Text = "Рождение: " + later.BirthDate.ToLongDateString();

                        // Выводим причину смерти еще не пережитого игрока
                        var laterDeathReasonName = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == later.DeathReasonId);
                        if (laterDeathReasonName != null) // Если пережитому игроку сопоставлена причина смерти, то выводим ее
                        {
                            control.NextHumanDeathDateTextBlock.Text = "(" + laterDeathReasonName.ReasonName + ") " +
                                                                       "Уход: " + later.DeathDate.ToLongDateString();
                        }
                        else // Если не сопоставлена, то выведем лаконичную строку
                        {
                            control.NextHumanDeathDateTextBlock.Text = "Смерть: (нет данных)";
                        }

                        // Выводим дату, когда еще не пережитый игрок будет пройден
                        control.NextHumanOverLifeDate.Text = "Пройдем: "
                                                           + (comparedHumanData.BirthDate + (later.DeathDate - later.BirthDate)).ToString("dd MMMM yyyy hh: mm");

                        // Выводим количество прожитых лет еще не пережитым игроком
                        control.NextHumanFullYearsTextBlock.Text = "Прожил: " + Math.Floor(later.FullYearsLived) + " лет";

                        control.CurrentProgressBar.Maximum = before + till; // Значение максимума прогресс-индикатора
                        control.CurrentProgressBar.Value = before; // Значение текущей позиции прогресс-индикатора

                        // Оставшийся до не пережитого игрока период времени
                        var laterSpent = comparedHumanData.BirthDate + (later.DeathDate - later.BirthDate) - currentDateTime;
                        var laterSpentDays = laterSpent.Days;
                        var laterSpentTime = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", laterSpent.Hours, laterSpent.Minutes, laterSpent.Seconds, laterSpent.Milliseconds);

                        control.RestDaysToNextHuman.Text = "Осталось: " +
                                                           laterSpentDays + " " +
                                                           _commonDataController.GetFinalText(laterSpentDays, PeriodTypes.Days) + " " + laterSpentTime;
                    }
                    else // Если же не пережитого не существует, то максимум прогресс-нидикатора и текущая позиция совпадают
                    {
                        // Сюда попадаем только тогда, когда анализируемый человек самый последний в списке - малопонятная схема
                        control.CurrentProgressBar.Maximum = before; // Максимум прогресс-индикатора
                        control.CurrentProgressBar.Value = before; // Текущая позиция прогресс-индикатора
                    }

                    // Выводим информацию (ФИО) о пережитом игроке
                    control.PreviousHumanNameTextBlock.Text = earlier.LastName + " "
                                                            + (earlier.FirstName != string.Empty ? (earlier.FirstName[0..1] + ".") : "") + " "
                                                            + (earlier.Patronymic != string.Empty ? (earlier.Patronymic[0..1] + ".") : "");

                    // Выводим дату рождения пережитого игрока
                    control.PreviousHumanBirthDateTextBlock.Text = "Рождение: " + earlier.BirthDate.ToLongDateString();

                    // Выводим причину смерти пережитого игрока
                    var earlierDeathReasonName = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == earlier.DeathReasonId);
                    if (earlierDeathReasonName != null) // Если пережитому игроку сопоставлена причина смерти, то выводим ее
                    {
                        control.PreviousHumanDeathDateTextBlock.Text = "Смерть: " +
                                                                   earlier.DeathDate.ToLongDateString() + " (" +
                                                                   earlierDeathReasonName.ReasonName + ")";
                    }
                    else // Если не сопоставлена, то выведем лаконичную строку
                    {
                        control.PreviousHumanDeathDateTextBlock.Text = "Смерть: (нет данных)";
                    }

                    // Выводим дату, когда пережитый игрок был пройден
                    control.PreviousHumanOverLifeDate.Text = "Пройдено: "
                                                           + (comparedHumanData.BirthDate + (earlier.DeathDate - earlier.BirthDate)).ToString("dd MMMM yyyy hh: mm");

                    // Выводим количество прожитых лет пережитым игроком
                    control.PreviousHumanFullYearsTextBlock.Text = "Прожил " + Math.Floor(earlier.FullYearsLived) + " лет";

                    // Выводим время, прошедшее с момента прохода пережитого игрока
                    var earlierSpent = currentDateTime - (comparedHumanData.BirthDate + (earlier.DeathDate - earlier.BirthDate));
                    var earlierSpentDays = earlierSpent.Days;
                    var earlierSpentTime = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", earlierSpent.Hours, earlierSpent.Minutes, earlierSpent.Seconds, earlierSpent.Milliseconds);
                    control.SpentDaysFromPreviousHuman.Text = "Прошло: " +
                                                              earlierSpentDays.ToString() + " " +
                                                              _commonDataController.GetFinalText(earlierSpentDays, PeriodTypes.Days) + " " + earlierSpentTime;
                }
                else // Если пережитый игрок не был найден (не существует) - анализируемый человек первый по возрасту
                {
                    // Вычисляем время, прошедшее с момента рождения анализируемого человека
                    var before = (currentDateTime - comparedHumanData.BirthDate).Days;

                    if (later != null) // Если не пережитый игрок существует
                    {
                        //// Если правая картинка нулевая - не выводим, если правая картинка не меняется - не выводим
                        //if (control.NextImage.Source != RightPicture && RightPicture != null) control.NextImage.Source = _commonDataController.GetHumanImage(later);

                        // Вычисляем время до момента его ухода
                        var till = ((comparedHumanData.BirthDate + (later.DeathDate - later.BirthDate)) - currentDateTime).Days;

                        // Выводим информацию (ФИО) о еще не пережитом игроке
                        control.NextHumanNameTextBlock.Text = later.LastName + " "
                                                            + (later.FirstName != string.Empty ? (later.FirstName[0..1] + ".") : "") + " "
                                                            + (later.Patronymic != string.Empty ? (later.Patronymic[0..1] + ".") : "");

                        // Выводим дату рождения еще не пережитого игрока
                        control.NextHumanBirthDateTextBlock.Text = "Рождение: " + later.BirthDate.ToLongDateString();

                        // Выводим причину смерти еще не пережитого игрока
                        var laterDeathReasonName = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == later.DeathReasonId);
                        if (laterDeathReasonName != null) // Если пережитому игроку сопоставлена причина смерти, то выводим ее
                        {
                            control.NextHumanDeathDateTextBlock.Text = "(" + laterDeathReasonName.ReasonName + ") " +
                                                                       "Смерть: " + later.DeathDate.ToLongDateString();
                        }
                        else
                        {
                            control.NextHumanDeathDateTextBlock.Text = "Смерть: (нет данных)";
                        }

                        // Выводим дату, когда еще не пережитый игрок будет пройден
                        control.NextHumanOverLifeDate.Text = "Пройдем: "
                                                           + (comparedHumanData.BirthDate + (later.DeathDate - later.BirthDate)).ToString("dd MMMM yyyy hh: mm");

                        // Выводим количество прожитых лет еще не пережитым игроком
                        control.NextHumanFullYearsTextBlock.Text = "Прожил " + Math.Floor(later.FullYearsLived) + " лет";

                        // Оставшийся до не пережитого игрока период времени
                        var laterSpent = /*comparedHumanData.BirthDate + */(later.DeathDate - later.BirthDate) - (currentDateTime - comparedHumanData.BirthDate);
                        var laterSpentDays = laterSpent.Days;
                        var laterSpentTime = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", laterSpent.Hours, laterSpent.Minutes, laterSpent.Seconds, laterSpent.Milliseconds);

                        control.RestDaysToNextHuman.Text = "Осталось: " +
                                                           laterSpentDays.ToString() + " " +
                                                           _commonDataController.GetFinalText(laterSpentDays, PeriodTypes.Days) + " " + laterSpentTime;

                        // Вычисляем время, прошедшее с момента рождения анализируемого
                        var before2 = (currentDateTime - comparedHumanData.BirthDate).Days;
                        // Вычисляем время до момента его ухода
                        var till2 = ((comparedHumanData.BirthDate + (later.DeathDate - later.BirthDate)) - currentDateTime).Days;

                        control.CurrentProgressBar.Maximum = before2 + till2; // Максимум прогресс-индикатора
                        control.CurrentProgressBar.Value = before2; // Текущая позиция прогресс-индикатора
                    }
                    else // Не пережитый игрок не найден - странная ситуация - ничегошеньки нет (ни до, ни после)
                    {
                        // В душе не чаю, что тут писать
                    }
                }
            //});
        }

        /// <summary>
        /// Метод получения строки впустую потраченного времени своей никчемной жизни
        /// </summary>
        /// <returns></returns>
        private string GetWastedTime(ComparedHumanProgressData comparedHumanData, DateTime currentDateTime)
        {
            var currentPos = currentDateTime - comparedHumanData.BirthDate;
            var yy = comparedHumanData.FullYearsLived * 365.25;
            var years = currentPos.Days / 365;
            var days = (int)Math.Floor(currentPos.TotalDays - yy);
            var time = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", currentPos.Hours, currentPos.Minutes, currentPos.Seconds, currentPos.Milliseconds);

            string resultString = "Прошло: " +
                                  years + " " + _commonDataController.GetFinalText(currentPos.Days / 365, PeriodTypes.Years) + ", " +
                                  days  + " " + _commonDataController.GetFinalText(days, PeriodTypes.Days) + ", " +
                                  time;

            return resultString;
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


#region MyRegion
//var period = DateTime.Now - comparedHumanData.BirthDate; // Прожитый период в днях
//for (var j = 0; j < period.Days; j += 10)
//{
//    var earlier1 = orderedList.LastOrDefault(x => x.DaysLived < j);  // Пережитый
//    var later1 = orderedList.FirstOrDefault(x => x.DaysLived > j); // Не пережитый

//    if (earlier1 != null) // Если пережитый игрок существует
//    {
//        ProgressDispatcher.Invoke(() =>
//        {
//            LeftPicture = _commonDataController.GetHumanImage(earlier1); // Загружаем картинку в правильном потоке
//        });
//    }

//    if (later1 != null) // Если еще не пережитый игрок существует
//    {
//        ProgressDispatcher.Invoke(() =>
//        {
//            RightPicture = _commonDataController.GetHumanImage(later1); // Загружаем картинку в правильном потоке
//        });
//    }
//    //Thread.Sleep(10);

//    var span = comparedHumanData.BirthDate.AddDays(j);
//    MainProcess(control, earlier1, later1, comparedHumanData, period, orderedList, LeftPicture, RightPicture, span);

//    if (token.IsCancellationRequested) return; // При прерывании процесса вываливаемся вообще
//} 
#endregion
