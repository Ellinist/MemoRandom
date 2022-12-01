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
using MahApps.Metro.Controls;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для хранения UC с прогрессом
    /// </summary>
    public class ComparingProcessViewModel : BindableBase
    {
        #region CONSTANTS
        private const int DaysHours      = 24;
        private const int HoursMinutes   = 60;
        private const int MinutesSeconds = 60;
        #endregion

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

        /// <summary>
        /// Метод отработки процедуры сравнения в отдельном потоке
        /// </summary>
        /// <param name="paramsData"></param>
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

            var startSpan = DateTime.Now.Subtract(comparedHumanData.BirthDate); // Стартовый диапазон анализируемого человека

            // Получаем информацию о пережитом (если есть) и не пережитом (если есть) человеке - в процессе работы может поменяться
            var previousActor = orderedList.LastOrDefault(x  => x.DaysLived < startSpan.TotalDays);  // Пережитый
            
            var nextActor     = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.TotalDays); // Не пережитый
            Reason previousReason = null;
            Reason nextReason = null;

            #region Начальный вывод картинок и данных, если они должны быть
            ProgressDispatcher.Invoke(() => // Начальный вывод картинок пережитого и не пережитого игроков
                {
                    if (previousActor != null) // Если пережитый игрок существует - отображаем его картинку
                    {
                        var previousReason = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == previousActor.DeathReasonId);
                        control.PreviousImage.Source = _commonDataController.GetHumanImage(previousActor); // Загружаем картинку пережитого игрока

                        // Отображение данных пережитого игрока, изменяемых только при смене игроков
                        ShowPreviousData(control, previousActor, previousReason);
                    }
                    else
                    {
                        // Отображение данных еще не пережитого игрока, изменяемых только при смене игроков
                        //ShowNextData(control, nextActor, nextReason);
                    }

                    if (nextActor != null)
                    {
                        var nextReason = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == nextActor.DeathReasonId);
                        control.NextImage.Source = _commonDataController.GetHumanImage(nextActor); // Загружаем картинку еще не пережитого игрока

                        // Отображение данных еще не пережитого игрока, изменяемых только при смене игроков
                        ShowNextData(control, nextActor, nextReason);
                    }
                });
            #endregion

            // Запускаем основной цикл отображения изменяющихся данных (зависят от текущего времени)
            while (!token.IsCancellationRequested) // Пока команда для остановки потока не придет, выполняем работу потока
            {
                // Весь бесконечный цикл проходим в потоке UI
                ProgressDispatcher.Invoke(() =>
                {
                    // В метод надо пробрасывать только необходимые аргументы
                    MainProcess(control, comparedHumanData, DateTime.Now, previousActor, nextActor);
                    //MainProcess(control, previousHuman, nextHuman, comparedHumanData, DateTime.Now);

                    var currentTimeLap = DateTime.Now.Subtract(comparedHumanData.BirthDate); // Вычисление разрыва между рождением и текущим временем
                    if (currentTimeLap > (nextActor.DeathDate.Subtract(nextActor.BirthDate)))
                    {
                        // Сдвигаем игроков влево
                        // ВНИМАНИЕ! Здесь поставить проверку, а вдруг следующего игрока нет! Обана!
                        previousActor = nextActor; // Предыдущий игрок становится следующим
                        nextActor = orderedList.FirstOrDefault(x => x.DaysLived > currentTimeLap.TotalDays); // А следующий - вычисляется
                        // И меняем картинки
                        control.PreviousImage.Source = _commonDataController.GetHumanImage(previousActor); // Загружаем картинку пережитого игрока
                        if (nextActor != null)
                        {
                            control.NextImage.Source = _commonDataController.GetHumanImage(nextActor); // Загружаем картинку еще не пережитого игрока

                            // Отображение данных, изменяемых только при смене игроков
                            ShowPreviousData(control, previousActor, previousReason);
                            ShowNextData(control, nextActor, nextReason);
                        }
                        else
                        {
                            control.NextImage.Source = null; // Если следующий игрок не найден - картинка обнуляется

                            //TODO обнулить данные для следующего игрока - его просто нет
                            ShowNextNullData(control);
                        }
                    }
                });

                Thread.Sleep(100); // Остановка потока для уменьшения нагрузки на программу
            }
        }

        /// <summary>
        /// Вывод информации о пережитом игроке, меняющейся только при смене игроков
        /// </summary>
        /// <param name="control"></param>
        /// <param name="previousActor"></param>
        /// <param name="previousReason"></param>
        private void ShowPreviousData(ComparedBlockControl control,
                                      Human previousActor,
                                      Reason previousReason)
        {
            // Выводим информацию (ФИО) о пережитом игроке
            control.PreviousHumanNameTextBlock.Text = previousActor.LastName + " " +
                                                     (previousActor.FirstName != string.Empty ? (previousActor.FirstName[0..1] + ".") : "") + " " +
                                                     (previousActor.Patronymic != string.Empty ? (previousActor.Patronymic[0..1] + ".") : "");

            // Выводим дату рождения пережитого игрока
            control.PreviousHumanBirthDateTextBlock.Text = "Рождение: " + previousActor.BirthDate.ToLongDateString() +
                                                           " " + string.Format($"{previousActor.BirthDate.Hour:D2}") +
                                                           " " + string.Format($"{previousActor.BirthDate.Minute:D2}");

            // Выводим причину смерти и дату смерти еще не пережитого игрока
            if (previousReason != null) // Если не пережитому игроку сопоставлена причина смерти, то выводим ее
            {
                control.PreviousHumanDeathDateTextBlock.Text = "(" + previousReason.ReasonName + ") " + "Уход: " +
                                                               previousActor.DeathDate.ToLongDateString() + " " +
                                                               string.Format($"{previousActor.DeathDate.Hour:D2}") + ":" +
                                                               string.Format($"{previousActor.DeathDate.Minute:D2}") + "";
            }
            else // Если не сопоставлена, то выведем лаконичную строку
            {
                control.PreviousHumanDeathDateTextBlock.Text = "(нет данных) Смерть:";
            }

            // Выводим количество прожитых лет пережитым игроком
            var currentPos = previousActor.DeathDate.Subtract(previousActor.BirthDate);
            var yy = (int)Math.Floor(previousActor.FullYearsLived) * 365.25;
            var previousDaysGone = (int)Math.Floor(currentPos.Days - yy);
            var previousPeriodGone = previousActor.DeathDate.Subtract(previousActor.BirthDate);
            control.PreviousHumanFullYearsTextBlock.Text = "Прожито: " + Math.Floor(previousActor.FullYearsLived) + " " +
                                                           _commonDataController.GetFinalText((int)previousActor.FullYearsLived, PeriodTypes.Years) +
                                                           " " + previousDaysGone + " " +
                                                           _commonDataController.GetFinalText(previousDaysGone, PeriodTypes.Days) +
                                                           " " + string.Format($"{previousPeriodGone.Hours:D2}:" +
                                                                               $"{previousPeriodGone.Minutes:D2}");
        }

        /// <summary>
        /// Вывод информации о еще не пережитом игроке, меняющейся только при смене игроков
        /// </summary>
        /// <param name="control"></param>
        /// <param name="nextActor"></param>
        /// <param name="nextReason"></param>
        private void ShowNextData(ComparedBlockControl control,
                                  Human nextActor,
                                  Reason nextReason)
        {
            // Вывод общей информации о еще не пережитом игроке
            // Выводим информацию (ФИО) о еще не пережитом игроке
            control.NextHumanNameTextBlock.Text = nextActor.LastName + " " +
                                                 (nextActor.FirstName != string.Empty ? (nextActor.FirstName[0..1] + ".") : "") + " " +
                                                 (nextActor.Patronymic != string.Empty ? (nextActor.Patronymic[0..1] + ".") : "");

            // Выводим дату рождения еще не пережитого игрока
            control.NextHumanBirthDateTextBlock.Text = "Рождение: " + nextActor.BirthDate.ToLongDateString() +
                                                       " " + string.Format($"{nextActor.BirthDate.Hour:D2}") +
                                                       " " + string.Format($"{nextActor.BirthDate.Minute:D2}");

            // Выводим причину смерти и дату смерти еще не пережитого игрока
            if (nextReason != null) // Если не пережитому игроку сопоставлена причина смерти, то выводим ее
            {
                control.NextHumanDeathDateTextBlock.Text = "(" + nextReason.ReasonName + ") " + "Уход: " +
                                                           nextActor.DeathDate.ToLongDateString() + " " +
                                                           string.Format($"{nextActor.DeathDate.Hour:D2}") + ":" +
                                                           string.Format($"{nextActor.DeathDate.Minute:D2}") + "";
            }
            else // Если не сопоставлена, то выведем лаконичную строку
            {
                control.NextHumanDeathDateTextBlock.Text = "(нет данных) Смерть:";
            }

            // Выводим количество прожитых лет еще не пережитым игроком
            var currentPos = nextActor.DeathDate.Subtract(nextActor.BirthDate);
            var yy = (int)Math.Floor(nextActor.FullYearsLived) * 365.25;
            var nextDaysLeft = (int)Math.Floor(currentPos.Days - yy);
            var nextPeriodLeft = nextActor.DeathDate.Subtract(nextActor.BirthDate);
            control.NextHumanFullYearsTextBlock.Text = "Прожито: " + Math.Floor(nextActor.FullYearsLived) + " " +
                                                       _commonDataController.GetFinalText((int)nextActor.FullYearsLived, PeriodTypes.Years) +
                                                       " " + nextDaysLeft + " " +
                                                       _commonDataController.GetFinalText(nextDaysLeft, PeriodTypes.Days) +
                                                       " " + string.Format($"{nextPeriodLeft.Hours:D2}:" +
                                                                           $"{nextPeriodLeft.Minutes:D2}");
        }

        /// <summary>
        /// Обнуление всех полей следующего игрока - если анализируемый остался последним в списке
        /// </summary>
        /// <param name="control"></param>
        private void ShowNextNullData(ComparedBlockControl control)
        {
            control.NextHumanNameTextBlock.Text = string.Empty;

            control.NextHumanBirthDateTextBlock.Text = string.Empty;

            control.NextHumanDeathDateTextBlock.Text = string.Empty;

            control.NextHumanFullYearsTextBlock.Text = string.Empty;

            control.NextHumanOverLifeDate.Text = string.Empty;

            control.RestDaysToNextHuman.Text = string.Empty;
        }

        /// <summary>
        /// Временный метод - потом станет основным
        /// </summary>
        private void MainProcess(ComparedBlockControl control,
                           ComparedHumanProgressData comparedHumanData,
                           DateTime currentTime,
                           Human previousActor, Human nextActor)
        {
            // Выводим бесцельно потраченное время жизни анализируемого человека
            control.CurrentHumanLivedPeriod.Text = GetWastedTime(comparedHumanData, currentTime);

            #region Блок существования предыдущего игрока
            if(previousActor != null)
            {
                // Вычисляем время, прошедшее с момента ухода пережитого игрока
                var previousActorDays = (currentTime - (comparedHumanData.BirthDate + (previousActor.DeathDate.Subtract(previousActor.BirthDate)))).TotalDays;
                // Вычисляем количество секунд, прошедшее с момента ухода пережитого игрока
                var previousActorSeconds = Math.Floor(previousActorDays * DaysHours * HoursMinutes * MinutesSeconds);

                // Проверка на наличие не пережитого игрока
                if(nextActor != null)
                {
                    // Выводим дату, когда еще не пережитый игрок будет пройден
                    control.NextHumanOverLifeDate.Text = "Преодоление: " +
                                                         (comparedHumanData.BirthDate +
                                                         (nextActor.DeathDate - nextActor.BirthDate)).ToString("dd MMMM yyyy HH:mm");

                    // Оставшийся до не пережитого игрока период времени
                    var nextPeriodLeft = comparedHumanData.BirthDate + nextActor.DeathDate.Subtract(nextActor.BirthDate) - currentTime;
                    var nextDaysLeft = nextPeriodLeft.Days;                                // Оставшиеся целые дни
                    var nextTimeLeft = string.Format($"{nextPeriodLeft.Hours:D2}:" +       // Часы
                                                     $"{nextPeriodLeft.Minutes:D2}:" +     // Минуты
                                                     $"{nextPeriodLeft.Seconds:D2}." +     // Секунды
                                                     $"{nextPeriodLeft.Milliseconds:D3}"); // Миллисекунды

                    control.RestDaysToNextHuman.Text = "Осталось: " + nextDaysLeft + " " +
                                                       _commonDataController.GetFinalText(nextDaysLeft, PeriodTypes.Days) +
                                                       " " + nextTimeLeft;

                    // Вычисляем время до момента его ухода
                    var nextActorDays = ((comparedHumanData.BirthDate + nextActor.DeathDate.Subtract(nextActor.BirthDate)) - currentTime).TotalDays;
                    // Вычисляем количество секунд, которое должно пройти до достижения возраста пережитого игрока
                    var nextActorSeconds = Math.Floor(nextActorDays * DaysHours * HoursMinutes * MinutesSeconds);

                    #region Управление прогресс-индикатором
                    control.CurrentProgressBar.Maximum = previousActorSeconds + nextActorSeconds; // Значение максимума прогресс-индикатора
                    control.CurrentProgressBar.Value   = previousActorSeconds; // Значение текущей позиции прогресс-индикатора
                    #endregion
                }
                else
                {
                    // Сюда попадаем только тогда, когда анализируемый человек самый последний в списке - малопонятная схема
                    control.CurrentProgressBar.Maximum = previousActorSeconds; // Максимум прогресс-индикатора
                    control.CurrentProgressBar.Value   = previousActorSeconds; // Текущая позиция прогресс-индикатора
                }

                // Выводим дату, когда еще не пережитый игрок будет пройден
                control.PreviousHumanOverLifeDate.Text = "Преодоление: " +
                                                         (comparedHumanData.BirthDate +
                                                         previousActor.DeathDate.Subtract(previousActor.BirthDate).ToString("dd MMMM yyyy HH:mm"));


                // Выводим время, прошедшее с момента прохода пережитого игрока
                var previousPeriodGone = currentTime - (comparedHumanData.BirthDate + previousActor.DeathDate.Subtract(previousActor.BirthDate));
                var previousDaysGone = previousPeriodGone.Days;
                var previousTimeGone = string.Format($"{previousPeriodGone.Hours:D2}:" +
                                                     $"{previousPeriodGone.Minutes:D2}:" +
                                                     $"{previousPeriodGone.Seconds:D2}." +
                                                     $"{previousPeriodGone.Milliseconds:D3}");
                control.SpentDaysFromPreviousHuman.Text = "Прошло: " +
                                                          previousDaysGone.ToString() + " " +
                                                          _commonDataController.GetFinalText(previousDaysGone, PeriodTypes.Days) +
                                                          " " + previousTimeGone;
            }
            #endregion
            #region Блок отсутствия предыдущего игрока
            else
            {
                if (nextActor != null)
                {
                    // Выводим дату, когда еще не пережитый игрок будет пройден
                    control.NextHumanOverLifeDate.Text = "Преодоление: " +
                                                         (comparedHumanData.BirthDate +
                                                         nextActor.DeathDate.Subtract(nextActor.BirthDate).ToString("dd MMMM yyyy HH:mm"));

                    // Оставшийся до не пережитого игрока период времени
                    var nextPeriodLeft = comparedHumanData.BirthDate + nextActor.DeathDate.Subtract(nextActor.BirthDate) - currentTime;
                    var nextDaysLeft = nextPeriodLeft.Days;                                // Оставшиеся целые дни
                    var nextTimeLeft = string.Format($"{nextPeriodLeft.Hours:D2}:" +       // Часы
                                                     $"{nextPeriodLeft.Minutes:D2}:" +     // Минуты
                                                     $"{nextPeriodLeft.Seconds:D2}." +     // Секунды
                                                     $"{nextPeriodLeft.Milliseconds:D3}"); // Миллисекунды

                    control.RestDaysToNextHuman.Text = "Осталось: " + nextDaysLeft + " " +
                                                       _commonDataController.GetFinalText(nextDaysLeft, PeriodTypes.Days) +
                                                       " " + nextTimeLeft;

                    // Вычисляем время до момента его ухода
                    var nextActorDays = ((comparedHumanData.BirthDate + nextActor.DeathDate.Subtract(nextActor.BirthDate)) - currentTime).TotalDays;
                    // Вычисляем количество секунд, которое должно пройти до достижения возраста пережитого игрока
                    var nextActorSeconds = Math.Floor(nextActorDays * DaysHours * HoursMinutes * MinutesSeconds);
                    var currentHumanDays = (currentTime - comparedHumanData.BirthDate).TotalDays;
                    var currentHumanSeconds = Math.Floor(currentHumanDays * DaysHours * HoursMinutes * MinutesSeconds);

                    #region Управление прогресс-индикатором
                    control.CurrentProgressBar.Maximum = nextActorSeconds + currentHumanSeconds; // Значение максимума прогресс-индикатора
                    control.CurrentProgressBar.Value = currentHumanSeconds; // Значение текущей позиции прогресс-индикатора
                    #endregion
                }
                else
                {
                    //TODO Ситуация странная - не ни до ни после
                }
            }
            #endregion
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
            var time = string.Format($"{currentPos.Hours:D2}:" +
                                     $"{currentPos.Minutes:D2}:" +
                                     $"{currentPos.Seconds:D2}." +
                                     $"{currentPos.Milliseconds:D3}");

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


#region Расчет количества високосных дней в периоде
//private static int NumberOfLeapYears(int startYear, int endYear)
//{
//    int counter = 0;
//    for (int year = startYear; year <= endYear; year++)
//        counter += DateTime.IsLeapYear(year) ? 1 : 0;
//    return counter;
//}
#endregion


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
