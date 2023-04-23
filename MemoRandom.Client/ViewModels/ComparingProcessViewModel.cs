using MemoRandom.Client.Common.Implementations;
using Prism.Mvvm;
using System.Threading;
using System.Windows.Threading;
using MemoRandom.Client.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Enums;
using MemoRandom.Client.Common.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для хранения UC с прогрессом
    /// </summary>
    public class ComparingProcessViewModel : BindableBase
    {
        #region CONSTANTS
        private const int DAYS_HOURS      = 24;
        private const int HOURS_MINUTES   = 60;
        private const int MINUTES_SECONDS = 60;
        #endregion

        #region PRIVATE FIELDS
        private readonly ICommonDataController _commonDataController;
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _token; // Токен для остановки потока
        private readonly Dispatcher _progressDispatcher;

        private string _comparingTitle = "Кого мы пережили и... как много еще предстоит сделать!";
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна сравнения
        /// </summary>
        public string ComparingTitle
        {
            get => _comparingTitle;
            set
            {
                _comparingTitle = value;
                RaisePropertyChanged(nameof(ComparingTitle));
            }
        }

        public ObservableCollection<ComparedHumanProgressData> ProgressCollection { get; set; } = new();
        #endregion

        /// <summary>
        /// Метод закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComparingProcessView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _cancelTokenSource.Cancel();
            _cancelTokenSource.Dispose();

            e.Cancel = false; // Окно закрываем
        }

        /// <summary>
        /// Формирование прогресс-индикаторов и потоков внутри
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComparingProcessView_Loaded(object sender, RoutedEventArgs e)
        {
            var orderedComparedHumansList = CommonDataController.ComparedHumansCollection
                .Where(x => x.IsComparedHumanConsidered == true);

            foreach (var human in orderedComparedHumansList)
            {
                var period = Math.Floor((DateTime.Now - human.BirthDate).Days / 365.25);
                ComparedHumanProgressData progressData = new ComparedHumanProgressData()
                {
                    // Эти параметры не меняются
                    CurrentHumanFullName = human.ComparedHumanFullName,
                    CurrentHumanBirthDate = "Рождение: " + human.BirthDate.ToLongDateString() +
                                            " " + string.Format($"{human.BirthDate.Hour:D2}") +
                                            ":" + string.Format($"{human.BirthDate.Minute:D2}"),
                    StartValue = 0
                };
                ProgressCollection.Add(progressData);

                Task.Run(() =>
                {
                    ProgressMethod(human, progressData);
                }, _token);
            }
        }

        /// <summary>
        /// Метод отработки процедуры сравнения в отдельном потоке
        /// </summary>
        /// <param name="human"></param>
        /// <param name="progressData"></param>
        private void ProgressMethod(ComparedHuman human, ComparedHumanProgressData progressData)
        {
            // Но! Прежде вычисляем стартовые позиции и вспомогательные данные
            var orderedList = CommonDataController.HumansList.OrderBy(x => x.DaysLived).ToList(); // Упорядоченный по возрасту список людей

            var startSpan = DateTime.Now.Subtract(human.BirthDate); // Стартовый диапазон анализируемого человека

            // Получаем информацию о пережитом (если есть) и не пережитом (если есть) человеке - в процессе работы может поменяться
            var previousActor = orderedList.LastOrDefault(x => x.DaysLived < startSpan.TotalDays);  // Пережитый
            var nextActor = orderedList.FirstOrDefault(x => x.DaysLived > startSpan.TotalDays); // Не пережитый

            #region Начальный вывод картинок и данных, если они должны быть
            _progressDispatcher.Invoke(() =>
            {
                if (previousActor != null) // Если пережитый игрок существует - отображаем его картинку
                {
                    var previousReason = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == previousActor.DeathReasonId);
                    progressData.PreviousImage = _commonDataController.GetPersonImage(previousActor.ImageFile);

                    // Отображение данных пережитого игрока, изменяемых только при смене игроков
                    ShowPreviousData(progressData, previousActor, previousReason);
                }

                if (nextActor != null)
                {
                    var nextReason = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == nextActor.DeathReasonId);
                    progressData.NextImage = _commonDataController.GetPersonImage(nextActor.ImageFile); // Загружаем картинку еще не пережитого игрока

                    // Отображение данных еще не пережитого игрока, изменяемых только при смене игроков
                    ShowNextData(progressData, nextActor, nextReason);
                }
            });
            #endregion

            // Запускаем основной цикл отображения изменяющихся данных (зависят от текущего времени)
            while (!_token.IsCancellationRequested) // Пока команда для остановки потока не придет, выполняем работу потока
            {
                // Весь бесконечный цикл проходим в потоке UI
                _progressDispatcher.Invoke(() =>
                {
                    // В метод надо пробрасывать только необходимые аргументы
                    MainProcess(progressData, human, DateTime.Now, previousActor, nextActor);

                    var currentTimeLap = DateTime.Now.Subtract(human.BirthDate); // Вычисление разрыва между рождением и текущим временем
                    if (currentTimeLap > (nextActor.DeathDate.Subtract(nextActor.BirthDate)))
                    {
                        // Сдвигаем игроков влево
                        // ВНИМАНИЕ! Здесь поставить проверку, а вдруг следующего игрока нет! Обана!
                        previousActor = nextActor; // Предыдущий игрок становится следующим
                        var prevReason = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == previousActor.DeathReasonId);
                        nextActor = orderedList.FirstOrDefault(x => x.DaysLived > currentTimeLap.TotalDays); // А следующий - вычисляется
                        var nReason = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == nextActor.DeathReasonId);
                        // И меняем картинки
                        progressData.PreviousImage = _commonDataController.GetPersonImage(previousActor.ImageFile); // Загружаем картинку пережитого игрока
                        if (nextActor != null)
                        {
                            progressData.NextImage = _commonDataController.GetPersonImage(nextActor.ImageFile); // Загружаем картинку еще не пережитого игрока

                            // Отображение данных, изменяемых только при смене игроков
                            ShowPreviousData(progressData, previousActor, prevReason);
                            ShowNextData(progressData, nextActor, nReason);
                        }
                        else
                        {
                            progressData.NextImage = null; // Если следующий игрок не найден - картинка обнуляется

                            //TODO обнулить данные для следующего игрока - его просто нет
                            ShowNextNullData(progressData);
                        }
                    }
                });

                Thread.Sleep(250); // Остановка потока для уменьшения нагрузки на программу
            }
        }

        /// <summary>
        /// Вывод информации о пережитом игроке, меняющейся только при смене игроков
        /// </summary>
        /// <param name="progressData"></param>
        /// <param name="previousActor"></param>
        /// <param name="previousReason"></param>
        private void ShowPreviousData(ComparedHumanProgressData progressData, Human previousActor, Reason previousReason)
        {
            // Выводим информацию (ФИО) о пережитом игроке
            progressData.PreviousHumanName = previousActor.LastName + " " +
                                            (previousActor.FirstName != string.Empty ? (previousActor.FirstName[0..1] + ".") : "") + " " +
                                            (previousActor.Patronymic != string.Empty ? (previousActor.Patronymic[0..1] + ".") : "");

            // Выводим дату рождения пережитого игрока
            progressData.PreviousHumanBirthDate = "Рождение: " + previousActor.BirthDate.ToLongDateString() +
                                                  " " + string.Format($"{previousActor.BirthDate.Hour:D2}") +
                                                  ":" + string.Format($"{previousActor.BirthDate.Minute:D2}");

            // Выводим причину смерти и дату смерти еще не пережитого игрока
            if (previousReason != null) // Если не пережитому игроку сопоставлена причина смерти, то выводим ее
            {
                progressData.PreviousHumanDeathDate = "(" + previousReason.ReasonName + ") " + "Уход: " +
                                                            previousActor.DeathDate.ToLongDateString() + " " +
                                                            string.Format($"{previousActor.DeathDate.Hour:D2}") + ":" +
                                                            string.Format($"{previousActor.DeathDate.Minute:D2}") + "";
            }
            else // Если не сопоставлена, то выведем лаконичную строку
            {
                progressData.PreviousHumanDeathDate = "(нет данных) Смерть:";
            }

            // Выводим количество прожитых лет пережитым игроком
            var previousPeriodGone = previousActor.DeathDate.Subtract(previousActor.BirthDate);
            int previousDaysGone = _commonDataController.GetYearsAndDaysConsideredLeaps(previousActor.BirthDate, previousActor.DeathDate).Item2;
            progressData.PreviousHumanFullYears = "Прожито: " + previousActor.FullYearsLived + " " +
                                                  _commonDataController.GetFinalText(previousActor.FullYearsLived, ScopeTypes.Years) +
                                                  " " + previousDaysGone + " " +
                                                  _commonDataController.GetFinalText(previousDaysGone, ScopeTypes.Days) +
                                                  " " + string.Format($"{previousPeriodGone.Hours:D2}:" +
                                                 $"{previousPeriodGone.Minutes:D2}");
        }

        /// <summary>
        /// Вывод информации о еще не пережитом игроке, меняющейся только при смене игроков
        /// </summary>
        /// <param name="progressData"></param>
        /// <param name="nextActor"></param>
        /// <param name="nextReason"></param>
        private void ShowNextData(ComparedHumanProgressData progressData, Human nextActor, Reason nextReason)
        {
            // Выводим информацию (ФИО) о еще не пережитом игроке
            progressData.NextHumanName = nextActor.LastName + " " +
                                        (nextActor.FirstName != string.Empty ? (nextActor.FirstName[0..1] + ".") : "") + " " +
                                        (nextActor.Patronymic != string.Empty ? (nextActor.Patronymic[0..1] + ".") : "");

            // Выводим дату рождения еще не пережитого игрока
            progressData.NextHumanBirthDate = "Рождение: " + nextActor.BirthDate.ToLongDateString() +
                                              " " + string.Format($"{nextActor.BirthDate.Hour:D2}") +
                                              ":" + string.Format($"{nextActor.BirthDate.Minute:D2}");

            // Выводим причину смерти и дату смерти еще не пережитого игрока
            if (nextReason != null) // Если не пережитому игроку сопоставлена причина смерти, то выводим ее
            {
                progressData.NextHumanDeathDate = "(" + nextReason.ReasonName + ") " + "Уход: " +
                                                        nextActor.DeathDate.ToLongDateString() + " " +
                                                        string.Format($"{nextActor.DeathDate.Hour:D2}") + ":" +
                                                        string.Format($"{nextActor.DeathDate.Minute:D2}") + "";
            }
            else // Если не сопоставлена, то выведем лаконичную строку
            {
                progressData.NextHumanDeathDate = "(нет данных) Смерть:";
            }

            // Выводим количество прожитых лет еще не пережитым игроком
            var nextPeriodLeft = nextActor.DeathDate.Subtract(nextActor.BirthDate);
            int nextDaysLeft = _commonDataController.GetYearsAndDaysConsideredLeaps(nextActor.BirthDate, nextActor.DeathDate).Item2;
            progressData.NextHumanFullYears = "Прожито: " + nextActor.FullYearsLived + " " +
                                              _commonDataController.GetFinalText(nextActor.FullYearsLived, ScopeTypes.Years) +
                                              " " + nextDaysLeft + " " +
                                              _commonDataController.GetFinalText(nextDaysLeft, ScopeTypes.Days) +
                                              " " + string.Format($"{nextPeriodLeft.Hours:D2}:" +
                                              $"{nextPeriodLeft.Minutes:D2}");
        }

        /// <summary>
        /// Обнуление всех полей следующего игрока - если анализируемый остался последним в списке
        /// </summary>
        /// <param name="progressData"></param>
        private void ShowNextNullData(ComparedHumanProgressData progressData)
        {
            progressData.NextHumanName = string.Empty;
            progressData.NextHumanBirthDate = string.Empty;
            progressData.NextHumanDeathDate = string.Empty;
            progressData.NextHumanFullYears = string.Empty;
            progressData.NextHumanOverLifeDate = string.Empty;
            progressData.RestDaysToNextHuman = string.Empty;
        }

        /// <summary>
        /// Основной процессорный метод вывода данных на экран
        /// </summary>
        /// <param name="human"></param>
        /// <param name="currentTime"></param>
        /// <param name="previousActor"></param>
        /// <param name="nextActor"></param>
        /// <param name="progressData"></param>
        private void MainProcess(ComparedHumanProgressData progressData, ComparedHuman human, DateTime currentTime,
                                 Human previousActor, Human nextActor)
        {
            // Выводим бесцельно потраченное время жизни анализируемого человека
            progressData.CurrentHumanLivedPeriod = GetWastedTime(human.BirthDate, DateTime.Now);

            #region Блок существования предыдущего игрока
            if (previousActor != null)
            {
                // Вычисляем время, прошедшее с момента ухода пережитого игрока
                var previousActorDays = (currentTime.Subtract(human.BirthDate) -
                                         previousActor.DeathDate.Subtract(previousActor.BirthDate)).TotalDays;

                // Вычисляем количество секунд, прошедшее с момента ухода пережитого игрока
                var previousActorSeconds = Math.Floor(previousActorDays * DAYS_HOURS * HOURS_MINUTES * MINUTES_SECONDS);

                // Проверка на наличие не пережитого игрока
                if (nextActor != null)
                {
                    // Выводим дату, когда еще не пережитый игрок будет пройден
                    progressData.NextHumanOverLifeDate = "Преодоление: " +
                                                         (human.BirthDate +
                                                         (nextActor.DeathDate - nextActor.BirthDate)).ToString("dd MMMM yyyy HH:mm");

                    // Оставшийся до не пережитого игрока период времени
                    var nextPeriodLeft = nextActor.DeathDate.Subtract(nextActor.BirthDate) - currentTime.Subtract(human.BirthDate);
                    var nextDaysLeft = nextPeriodLeft.Days/* + correctionDays*/;               // Оставшиеся целые дни
                    var nextTimeLeft = string.Format($"{nextPeriodLeft.Hours:D2}:" +       // Часы
                                                     $"{nextPeriodLeft.Minutes:D2}:" +     // Минуты
                                                     $"{nextPeriodLeft.Seconds:D2}." +     // Секунды
                                                     $"{nextPeriodLeft.Milliseconds:D3}"); // Миллисекунды

                    progressData.RestDaysToNextHuman = "Осталось: " + nextDaysLeft + " " +
                                                       _commonDataController.GetFinalText(nextDaysLeft, ScopeTypes.Days) +
                                                       " " + nextTimeLeft;

                    // Вычисляем время до момента его ухода
                    var nextActorDays = (nextActor.DeathDate.Subtract(nextActor.BirthDate) -
                                               currentTime.Subtract(human.BirthDate)).TotalDays/* + correctionDays*/;

                    // Вычисляем количество секунд, которое должно пройти до достижения возраста пережитого игрока
                    var nextActorSeconds = Math.Floor(nextActorDays * DAYS_HOURS * HOURS_MINUTES * MINUTES_SECONDS);

                    #region Управление прогресс-индикатором
                    progressData.StopValue = previousActorSeconds + nextActorSeconds; // Значение максимума прогресс-индикатора
                    progressData.CurrentValue = previousActorSeconds; // Значение текущей позиции прогресс-индикатора
                    #endregion
                }
                else
                {
                    // Сюда попадаем только тогда, когда анализируемый человек самый последний в списке - малопонятная схема
                    progressData.StopValue = previousActorSeconds; // Максимум прогресс-индикатора
                    progressData.CurrentValue = previousActorSeconds; // Текущая позиция прогресс-индикатора
                }

                // Выводим дату, когда еще не пережитый игрок будет пройден
                progressData.PreviousHumanOverLifeDate = "Преодоление: " +
                                                         (human.BirthDate +
                                                          previousActor.DeathDate.Subtract(previousActor.BirthDate)).ToString("dd MMMM yyyy HH:mm");


                // Выводим время, прошедшее с момента прохода пережитого игрока
                var previousPeriodGone = currentTime.Subtract(human.BirthDate) - previousActor.DeathDate.Subtract(previousActor.BirthDate);
                var previousDaysGone = previousPeriodGone.Days/* + correctionPreviousDays*/;
                var previousTimeGone = string.Format($"{previousPeriodGone.Hours:D2}:" +
                                                     $"{previousPeriodGone.Minutes:D2}:" +
                                                     $"{previousPeriodGone.Seconds:D2}." +
                                                     $"{previousPeriodGone.Milliseconds:D3}");
                progressData.SpentDaysFromPreviousHuman = "Прошло: " +
                                                          previousDaysGone.ToString() + " " +
                                                          _commonDataController.GetFinalText(previousDaysGone, ScopeTypes.Days) +
                                                          " " + previousTimeGone;
            }
            #endregion
            #region Блок отсутствия предыдущего игрока
            else
            {
                if (nextActor != null)
                {
                    // Вычисляем коррекцию дней
                    int correctionNextDays = _commonDataController.GetYearsAndDaysConsideredLeaps(nextActor.BirthDate, nextActor.DeathDate).Item2;

                    // Выводим дату, когда еще не пережитый игрок будет пройден
                    progressData.NextHumanOverLifeDate = "Преодоление: " +
                                                         (human.BirthDate +
                                                          nextActor.DeathDate.Subtract(nextActor.BirthDate)).ToString("dd MMMM yyyy HH:mm");

                    // Оставшийся до не пережитого игрока период времени
                    var nextPeriodLeft = nextActor.DeathDate.Subtract(nextActor.BirthDate) - currentTime.Subtract(human.BirthDate);
                    var nextDaysLeft = nextPeriodLeft.Days + correctionNextDays;           // Оставшиеся целые дни
                    var nextTimeLeft = string.Format($"{nextPeriodLeft.Hours:D2}:" +       // Часы
                                                     $"{nextPeriodLeft.Minutes:D2}:" +     // Минуты
                                                     $"{nextPeriodLeft.Seconds:D2}." +     // Секунды
                                                     $"{nextPeriodLeft.Milliseconds:D3}"); // Миллисекунды

                    progressData.RestDaysToNextHuman = "Осталось: " + nextDaysLeft + " " +
                                                       _commonDataController.GetFinalText(nextDaysLeft, ScopeTypes.Days) +
                                                       " " + nextTimeLeft;

                    // Вычисляем время до момента его ухода
                    var nextActorDays = (nextActor.DeathDate.Subtract(nextActor.BirthDate) -
                                         currentTime.Subtract(human.BirthDate)).TotalDays + correctionNextDays;

                    #region Управление прогресс-индикатором
                    // Вычисляем количество секунд, которое должно пройти до достижения возраста пережитого игрока
                    var nextActorSeconds = Math.Floor(nextActorDays * DAYS_HOURS * HOURS_MINUTES * MINUTES_SECONDS);
                    var currentHumanDays = currentTime.Subtract(human.BirthDate).TotalDays;
                    var currentHumanSeconds = Math.Floor(currentHumanDays * DAYS_HOURS * HOURS_MINUTES * MINUTES_SECONDS);
                    progressData.StopValue = nextActorSeconds + currentHumanSeconds; // Значение максимума прогресс-индикатора
                    progressData.CurrentValue = currentHumanSeconds; // Значение текущей позиции прогресс-индикатора
                    #endregion
                }
                else
                {
                    //TODO Ситуация странная - нет ни до ни после
                }
            }
            #endregion
        }

        /// <summary>
        /// Метод получения строки впустую потраченного времени своей никчемной жизни
        /// </summary>
        /// <param name="birthDate"></param>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        private string GetWastedTime(DateTime birthDate, DateTime currentDateTime)
        {
            (int y, int d) = _commonDataController.GetYearsAndDaysConsideredLeaps(birthDate, currentDateTime);

            var currentPos = currentDateTime.Subtract(birthDate);
            var time = string.Format($"{currentPos.Hours:D2}:" +
                                     $"{currentPos.Minutes:D2}:" +
                                     $"{currentPos.Seconds:D2}." +
                                     $"{currentPos.Milliseconds:D3}");

            string resultString = "Прошло: " +
                                  y + " " + _commonDataController.GetFinalText(y, ScopeTypes.Years) + ", " +
                                  d + " " + _commonDataController.GetFinalText(d, ScopeTypes.Days) + ", " +
                                  time;

            return resultString;
        }







        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="commonDataController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ComparingProcessViewModel(ICommonDataController commonDataController)
        {
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

            _token = _cancelTokenSource.Token;
            _progressDispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion
    }
}


#region Расчет количества високосных дней в периоде
// Это не совсем верно, так как начало периода может приходиться на день после високосного года, а здесь учитывается весь год
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
