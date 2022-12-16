using DryIoc;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemoRandom.Client.Views;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Models;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Enums;
using ScottPlot;

using ScottPlot.Statistics;
using MemoRandom.Data.Interfaces;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления основного окна со списком людей
    /// </summary>
    public class HumansListViewModel : BindableBase, IDisposable
    {
        public event Action<Human> SetCurrentRecordEvent;

        #region PRIVATE FIELDS
        private string _humansViewTitle = "День уходящий не вернуть! Не торопись пройти свой путь!";
        private int _personIndex;
        private int _previousIndex = 0; // Индекс предыдущего выбранного узла в списке
        private Human _selectedHuman;
        private BitmapSource _imageSource;
        private string _displayedYears = "";
        private string _humanDeathReasonName;
        private readonly StringBuilder _yearsText = new();
        private string _sortMember;
        private string _sortDirection;

        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly ICommonDataController _commonDataController;
        // Временно - потом уйти на общий контроллер
        private readonly IXmlController _xmlController;

        private CultureInfo cultureInfo = new CultureInfo("ru-RU");

        private int _humansQuantity;
        private int _averageAge;
        private int _minimumAge;
        private int _maximumAge;
        private string _earliestHuman;
        private string _oldestHuman;
        private string _menQuantities;
        private string _earliestYears;
        private string _averageYears;
        private string _oldestYears;

        private WpfPlot _mainPlot;
        private WpfPlot _populationPlot;
        private WpfPlot _secondPlot;
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна со списком людей
        /// </summary>
        public string HumansViewTitle
        {
            get => _humansViewTitle;
            set
            {
                _humansViewTitle = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Отображаемый список людей
        /// </summary>
        public ObservableCollection<Human> HumansCollection
        {
            get => CommonDataController.HumansList;
            set
            {
                CommonDataController.HumansList = value;
                RaisePropertyChanged(nameof(HumansCollection));
            }
        }

        /// <summary>
        /// Индекс выбранного человека
        /// </summary>
        public int PersonIndex
        {
            get => _personIndex;
            set
            {
                _previousIndex = _personIndex;
                _personIndex = value;
                RaisePropertyChanged(nameof(PersonIndex));
            }
        }

        /// <summary>
        /// Свойство-изображение
        /// </summary>
        public BitmapSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                RaisePropertyChanged(nameof(ImageSource));
            }
        }

        /// <summary>
        /// Выбранный в списке человек
        /// </summary>
        public Human SelectedHuman
        {
            get => _selectedHuman;
            set
            {
                if(HumansCollection != null && HumansCollection.Count > 0 && value != null)
                {
                    _selectedHuman = value;

                    // При смене выбранного человека устанавливаем его текущим
                    CommonDataController.CurrentHuman = value;
                    RaisePropertyChanged(nameof(SelectedHuman));

                    // Изменение изображения
                    var imageResult = _commonDataController.GetHumanImage(CommonDataController.CurrentHuman);
                    if (imageResult != null)
                    {
                        ImageSource = imageResult;
                        RaisePropertyChanged(nameof(ImageSource));
                    }
                    else
                    {
                        ImageSource = null;
                        RaisePropertyChanged(nameof(ImageSource));
                    }

                    // Изменение текста прожитых лет
                    SetFullYearsText(SelectedHuman);

                    // Название причины смерти
                    var res = PlainReasonsList.FirstOrDefault(x => x.ReasonId == SelectedHuman.DeathReasonId);
                    if(res != null)
                    {
                        HumanDeathReasonName = res.ReasonName;
                        RaisePropertyChanged(nameof(HumanDeathReasonName));
                    }
                    else
                    {
                        HumanDeathReasonName = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Отображаемое значение прожитых лет
        /// </summary>
        public string DisplayedYears
        {
            get => _displayedYears.ToString();
            set
            {
                _displayedYears = value;
                RaisePropertyChanged(nameof(DisplayedYears));
            }
        }

        /// <summary>
        /// Название причины смерти
        /// </summary>
        public string HumanDeathReasonName
        {
            get => _humanDeathReasonName;
            set
            {
                _humanDeathReasonName = value;
                RaisePropertyChanged(nameof(HumanDeathReasonName));
            }
        }

        /// <summary>
        /// Плоский список справочника причин смерти для вытягивания названия причины
        /// </summary>
        public List<Reason> PlainReasonsList
        {
            get => CommonDataController.PlainReasonsList;
            set
            {
                CommonDataController.PlainReasonsList = value;
                RaisePropertyChanged(nameof(PlainReasonsList));
            }
        }

        /// <summary>
        /// Количество записей (людей) в хранилище - количество анализируемых людей
        /// </summary>
        public int HumansQuantity
        {
            get => _humansQuantity;
            set
            {
                _humansQuantity = value;
                RaisePropertyChanged(nameof(HumansQuantity));
            }
        }

        /// <summary>
        /// Средний (прожитый) возраст анализируемых людей
        /// </summary>
        public int AverageAge
        {
            get => _averageAge;
            set
            {
                _averageAge = value;
                RaisePropertyChanged(nameof(AverageAge));
            }
        }

        /// <summary>
        /// Минимальный прожитый возраст (самый ранний уход)
        /// </summary>
        public int MinimumAge
        {
            get => _minimumAge;
            set
            {
                _minimumAge = value;
                RaisePropertyChanged(nameof(MinimumAge));
            }
        }

        /// <summary>
        /// Максимальный прожитый возраст (самый поздний уход)
        /// </summary>
        public int MaximumAge
        {
            get => _maximumAge;
            set
            {
                _maximumAge = value;
                RaisePropertyChanged(nameof(MaximumAge));
            }
        }

        public string EarliestHuman
        {
            get => _earliestHuman;
            set
            {
                _earliestHuman = value;
                RaisePropertyChanged(nameof(EarliestHuman));
            }
        }

        public string OldestHuman
        {
            get => _oldestHuman;
            set
            {
                _oldestHuman = value;
                RaisePropertyChanged(nameof(OldestHuman));
            }
        }

        public string MenQuantities
        {
            get => _menQuantities;
            set
            {
                _menQuantities = value;
                RaisePropertyChanged(nameof(MenQuantities));
            }
        }

        public string EarliestYears
        {
            get => _earliestYears;
            set
            {
                _earliestYears = value;
                RaisePropertyChanged(nameof(EarliestYears));
            }
        }

        public string OldestYears
        {
            get => _oldestYears;
            set
            {
                _oldestYears = value;
                RaisePropertyChanged(nameof(OldestYears));
            }
        }

        public string AverageYears
        {
            get => _averageYears;
            set
            {
                _averageYears = value;
                RaisePropertyChanged(nameof(AverageYears));
            }
        }

        public WpfPlot MainPlot
        {
            get => _mainPlot;
            set
            {
                _mainPlot = value;
                RaisePropertyChanged(nameof(MainPlot));
            }
        }

        public WpfPlot PopulationPlot
        {
            get => _populationPlot;
            set
            {
                _populationPlot = value;
                RaisePropertyChanged(nameof(PopulationPlot));
            }
        }

        public WpfPlot SecondPlot
        {
            get => _secondPlot;
            set
            {
                _secondPlot = value;
                RaisePropertyChanged(nameof(SecondPlot));
            }
        }
        #endregion

        #region Частные методы
        /// <summary>
        /// Формирование текстов для отображения прожитых лет в соответствии с числом
        /// </summary>
        /// <param name="selectedHuman"></param>
        private void SetFullYearsText(Human selectedHuman)
        {
            //int years = (int)Math.Floor(selectedHuman.FullYearsLived); // Считаем число полных лет
            int years = selectedHuman.FullYearsLived; // Считаем число полных лет
            _yearsText.Clear();

            _yearsText.Append("(" + years + " " + _commonDataController.GetFinalText(years, ScopeTypes.Years) + ")");

            DisplayedYears = _yearsText.ToString();
        }

        /// <summary>
        /// Событие сортировки по заголовку столбца
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DgHumans_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            var previousHuman = CommonDataController.CurrentHuman;

            _sortDirection = e.Column.SortDirection.ToString();
            _sortMember = e.Column.SortMemberPath;

            SortHumansCollection();
            RaisePropertyChanged(nameof(CommonDataController.HumansList));

            SelectedHuman = previousHuman;

            var i = CommonDataController.HumansList.IndexOf(previousHuman);
            PersonIndex = i;

            RaisePropertyChanged(nameof(SelectedHuman));
            RaisePropertyChanged(nameof(PersonIndex));

            SetCurrentRecordEvent?.Invoke(previousHuman);
        }

        /// <summary>
        /// Сортировка по условию упорядочивания при щелчке на столбце таблицы
        /// </summary>
        /// <returns></returns>
        private void SortHumansCollection()
        {
            List<Human> result;
            if (_sortMember == null)
            {
                result = HumansCollection.OrderBy(x => x.DaysLived).ToList();
            }
            else
            {
                var param = _sortMember;
                var propertyInfo = typeof(Human).GetProperty(param);
                // Создаем новую сущность, упорядоченную по столбцу сортировки
                result = (_sortDirection == null || _sortDirection == "Ascending") ?
                          HumansCollection.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList() :
                          HumansCollection.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
            }

            CommonDataController.HumansList.Clear();
            foreach (var item in result)
            {
                CommonDataController.HumansList.Add(item);
            }
            RaisePropertyChanged(nameof(HumansCollection));
        }

        /// <summary>
        /// Запуск окна создания нового человека
        /// </summary>
        private void AddHuman()
        {
            CommonDataController.CurrentHuman = null; // Для новой записи!
            _container.Resolve<HumanDetailedView>().ShowDialog(); // Открываем окно создания/редактирования

            if (CommonDataController.CurrentHuman == null) return;

            CommonDataController.HumansList.Add(CommonDataController.CurrentHuman); // Добавляем человека в список местного хранилища
            SortHumansCollection(); // Сортировка по условию

            SelectedHuman = CommonDataController.CurrentHuman;
            PersonIndex = HumansCollection.IndexOf(CommonDataController.CurrentHuman);
            RaisePropertyChanged(nameof(PersonIndex));

            ImageSource = _commonDataController.GetHumanImage(CommonDataController.CurrentHuman);
            RaisePropertyChanged(nameof(ImageSource));

            var currentReason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == SelectedHuman.DeathReasonId);
            if (currentReason != null)
            {
                HumanDeathReasonName = currentReason.ReasonName;
            }
            CalculateAnalitics();
        }

        /// <summary>
        /// Вызов окна редактирования выбранного человека
        /// </summary>
        private void EditHumanData()
        {
            _container.Resolve<HumanDetailedView>().ShowDialog(); // Запуск окна создания и редактирования человека

            SortHumansCollection(); // Сортировка по условию

            PersonIndex = HumansCollection.IndexOf(CommonDataController.CurrentHuman);
            RaisePropertyChanged(nameof(PersonIndex));
            SetFullYearsText(CommonDataController.CurrentHuman);

            ImageSource = _commonDataController.GetHumanImage(CommonDataController.CurrentHuman);
            RaisePropertyChanged(nameof(ImageSource));

            var currentReason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == SelectedHuman.DeathReasonId);
            if (currentReason != null)
            {
                HumanDeathReasonName = currentReason.ReasonName;
            }
            CalculateAnalitics();
        }

        public void DgHumans_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditHumanData();
        }

        /// <summary>
        /// Удаление выбранного человека
        /// </summary>
        private async void DeleteHuman()
        {
            var result = MessageBox.Show("Удалить выбранного человека?", "Удаление!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            if (_previousIndex == -1) _previousIndex = 0; // Пока так - но надо умнее сделать

            var formerId = HumansCollection[_previousIndex].HumanId;
            try
            {
                await Task.Run(() =>
                {
                    _commonDataController.DeleteHuman(SelectedHuman, SelectedHuman.ImageFile);
                    //_commonDataController.DeleteHumanInRepository(SelectedHuman, SelectedHuman.ImageFile); // Удаление во внешнем хранилище
                });

                CommonDataController.HumansList.Remove(SelectedHuman); // Удаление в списке
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось Удалить!\n Код ошибки в журнале", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                _logger.Error($"Ошибка: {ex}");
            }
            PersonIndex = HumansCollection.IndexOf(HumansCollection.FirstOrDefault(x => x.HumanId == formerId));
            RaisePropertyChanged(nameof(PersonIndex));

            CalculateAnalitics();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Команда добавления человека
        /// </summary>
        public DelegateCommand AddHumanCommand { get; private set; }

        /// <summary>
        /// Команда редактирования данных по выбранному человеку
        /// </summary>
        public DelegateCommand EditHumanDataCommand { get; private set; }
        
        /// <summary>
        /// Команда удаления выбранного человека
        /// </summary>
        public DelegateCommand DeleteHumanCommand { get; private set; }

        /// <summary>
        /// Команда вызова окна создания людей для сравнения
        /// </summary>
        public DelegateCommand ComparedHumansOpenCommand { get; private set; }

        public DelegateCommand DynamicShowCommand { get; private set; }

        public DelegateCommand SettingsMenuCommand { get; private set; }
        
        public DelegateCommand HumansListMenuCommand { get; private set; }
        
        public DelegateCommand StartMenuCommand { get; private set; }
        
        public DelegateCommand StartAboutCommand { get; private set; }
        
        public DelegateCommand AddNewHumanCommand { get; private set; }

        public DelegateCommand CategoriesCommand { get; private set; }

        public DelegateCommand SaveXmlCommand { get; private set; }
        public DelegateCommand ReadXmlCommand { get; private set; }
        public DelegateCommand AddReasonCommand { get; private set; }
        #endregion

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            AddHumanCommand      = new DelegateCommand(AddHuman);
            EditHumanDataCommand = new DelegateCommand(EditHumanData);
            DeleteHumanCommand   = new DelegateCommand(DeleteHuman);
            StartAboutCommand    = new DelegateCommand(OpenAboutView);
            CategoriesCommand    = new DelegateCommand(CategoriesOpen);
            ComparedHumansOpenCommand = new DelegateCommand(ComparedHumansOpen);
            DynamicShowCommand   = new DelegateCommand(DynamicShow);

            SaveXmlCommand = new DelegateCommand(SaveXml);
            ReadXmlCommand = new DelegateCommand(ReadXml);
            AddReasonCommand = new DelegateCommand(AddReason);
        }

        /// <summary>
        /// Временно
        /// </summary>
        private void SaveXml()
        {
            _commonDataController.SaveXmlData(); // Вызов сохранения
        }

        private void ReadXml()
        {
            _commonDataController.ReadXmlData();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HumansCollection)); // Без этого список не обновляется
        }

        /// <summary>
        /// Тестовый метод добавления причины
        /// </summary>
        private void AddReason()
        {
            //Reason rsn = new()
            //{
            //    ReasonId = Guid.NewGuid(),
            //    ReasonName = "TestReason",
            //    ReasonComment = "TestComment",
            //    ReasonDescription = "TestDescription",
            //    ReasonParentId = Guid.Empty
            //};


            //_commonDataController.AddReasonToFile(rsn);
            
            //var rs = PlainReasonsList.FirstOrDefault(x => x.ReasonId == Guid.Parse("250f4559-04dc-45b3-bdb4-21068750dd26"));

            //rs.ReasonName = "New reason name";
            //rs.ReasonComment = "new reason comment";
            //rs.ReasonDescription = "new reason description";

            //_commonDataController.ChangeReason(rs);

            //_commonDataController.DeleteReasonAndDaughters(rs.ReasonId);
        }

        /// <summary>
        /// Вызов окна динамического отображения прогресса
        /// </summary>
        private void DynamicShow()
        {
            _container.Resolve<ComparingProcessView>().ShowDialog();
        }

        /// <summary>
        /// Вызов окна редактирования категорий
        /// </summary>
        private void CategoriesOpen()
        {
            _container.Resolve<CategoriesView>().ShowDialog();
        }

        /// <summary>
        /// Вызов окна редактирования людей для сравнения
        /// </summary>
        private void ComparedHumansOpen()
        {
            _container.Resolve<ComparedHumansView>().ShowDialog();
        }

        /// <summary>
        /// Открытие окна "О программе"
        /// </summary>
        private void OpenAboutView()
        {
            _container.Resolve<AboutView>().ShowDialog();
        }

        /// <summary>
        /// Загрузка окна со списком людей
        /// </summary>
        /// <param name="e"></param>
        /// <param name="plot"></param>
        /// <param name="plot2"></param>
        /// <param name="plot3"></param>
        public void HumansListView_Loaded(WpfPlot plot, WpfPlot plot2, WpfPlot plot3)
        {
            MainPlot = plot;
            PopulationPlot = plot3;
            SecondPlot = plot2;
            
            CalculateAnalitics();
        }

        /// <summary>
        /// Калькуляция статистических параметров
        /// </summary>
        private void CalculateAnalitics()
        {
            #region Штатный график
            MainPlot.Plot.Clear(); // Очистка графика - на случай изменений на лету
            var plt = MainPlot.Plot;

            // Получим новый список, в котором будут только родительские ReasonID верхнего уровня
            #region Создание нового списка, в котором будут только родительские ReasonID верхнего уровня
            
            Guid upperLevelId = Guid.Empty; // Этот параметр можно менять
            
            List<Guid> headerIdsList = new();
            for (int q = 0; q < CommonDataController.HumansList.Count; q++)
            {
                var reasonId = CommonDataController.HumansList[q].DeathReasonId; // Id причины
                OnceAgain(); // Вызываем локальную рекурсивную функцию

                void OnceAgain() // Локальная функция рекурсивная - поиск главного родителя
                {
                    var reason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == reasonId);
                    if(reason == null) // Ситуация, когда человек не связан с причиной смерти
                    {
                        headerIdsList.Add(Guid.Empty);
                    }
                    else if (reason.ReasonParentId != upperLevelId) // Если есть родитель
                    {
                        reasonId = reason.ReasonParentId;
                        OnceAgain(); // Рекурсия
                    }
                    else // Нет родителя
                    {
                        headerIdsList.Add(CommonDataController.HumansList[q].DeathReasonId);
                        headerIdsList[^1] = reasonId; // Замена текущего Id на Id головного родителя
                    }
                }
            }
            #endregion

            var groupedList = headerIdsList.GroupBy(x => x).ToList(); // Группировка по Id причины
            double[] valuesArray = new double[groupedList.Count];
            string[] labelsArray = new string[groupedList.Count];

            int counter = 0;
            foreach (var item in groupedList)
            {
                if (item.Key == Guid.Empty)
                {
                    valuesArray[counter] = item.Count(); // Количество записей с одинаковым пустым Id причины
                    labelsArray[counter] = "Нет данных"; // Нет привязки - нет информации
                }
                else
                {
                    valuesArray[counter] = item.Count();
                    labelsArray[counter] = PlainReasonsList.FirstOrDefault(x => x.ReasonId == item.Key).ReasonName;
                }
                counter++;
            }

            var pie = plt.AddPie(valuesArray);
            pie.SliceLabels = labelsArray;
            plt.Legend();
            pie.Explode = true;
            //pie.ShowValues = true;
            //pie.ShowLabels = true;
            //pie.DonutSize = .6;
            pie.ShowPercentages = true;

            MainPlot.Refresh();
            #endregion


            #region Это для двух графиков - ниже
            double[] scores2 = new double[CommonDataController.HumansList.Count];
            for (int j = 0; j < CommonDataController.HumansList.Count; j++)
            {
                scores2[j] = CommonDataController.HumansList[j].FullYearsLived;
            }
            var pop2 = new Population(scores2);
            #endregion

            #region Дополнительный график
            SecondPlot.Plot.Clear();
            var plt2 = SecondPlot.Plot;

            // generate sample heights are based on https://ourworldindata.org/human-height
            //Random rand = new(0);
            //double[] values = DataGen.RandomNormal(rand, pointCount: 1234, mean: 178.4, stdDev: 7.6);

            // create a histogram
            //(double[] counts, double[] binEdges) = ScottPlot.Statistics.Common.Histogram(values, min: 140, max: 220, binSize: 1);
            //double[] leftEdges = binEdges.Take(binEdges.Length - 1).ToArray();

            // Параметры статистики для гистограммы
            // первый - массив возрастов
            // второй - минимальный учитываемый
            // третий - максимальный учитываемый
            // четвертый - принятый шаг по годам
            (double[] counts, double[] binEdges) = ScottPlot.Statistics.Common.Histogram(scores2, min: 0, max: 110, binSize: 1);
            double[] leftEdges = binEdges.Take(binEdges.Length - 1).ToArray();

            // display the histogram counts as a bar plot
            var bar = plt2.AddBar(values: counts, positions: leftEdges);
            bar.BarWidth = 1;

            // customize the plot style
            plt2.YAxis.Label("Распределение (людей)");
            plt2.XAxis.Label("Возраст (лет)");
            plt2.SetAxisLimits(yMin: 0);

            SecondPlot.Refresh();
            #endregion

            #region Еще один график - по популяции - с ним надо поработать тщательнее
            // The population plot makes it easy to display populations as bar graphs, box - and - whisker plots, scattered values,
            // or box plots and data points side-by - side.The population plot is different than using a box plot
            // with an error bar in that you pass your original data into the population plot and it determines the standard deviation,
            // standard error, quartiles, mean, median, outliers, etc., and you get to determine how to display these values.

            // https://scottplot.net/cookbook/4.1/category/plottable-population/#population-plot

            // Описание параметров класса Population
            //public double[] values { get; private set; }
            //public double[] sortedValues { get; private set; }
            //public double min { get; private set; }
            //public double max { get; private set; }
            //public double median { get; private set; }
            //public double sum { get; private set; }
            //public int count { get; private set; }
            //public double mean { get; private set; }
            //public double stDev { get; private set; }
            //public double plus3stDev { get; private set; }
            //public double minus3stDev { get; private set; }
            //public double plus2stDev { get; private set; }
            //public double minus2stDev { get; private set; }
            //public double stdErr { get; private set; }
            //public double Q1 { get; private set; }
            //public double Q3 { get; private set; }
            //public double IQR { get; private set; }
            //public double[] lowOutliers { get; private set; }
            //public double[] highOutliers { get; private set; }
            //public double maxNonOutlier { get; private set; }
            //public double minNonOutlier { get; private set; }
            //public int n { get { return values.Length; } }
            //public double span { get { return sortedValues.Last() - sortedValues.First(); } }

            PopulationPlot.Plot.Clear();
            var plt3 = PopulationPlot.Plot;
            // create sample data to represent test scores
            //Random rand2 = new Random(0);
            //double[] scores = DataGen.RandomNormal(rand2, 35, 85, 5);

            // First, create a Population object from your test scores
            //var pop = new Population(scores);

            // You can access population statistics as public fields
            //plt3.Title($"Mean: {pop.mean} +/- {pop.stdErr}");
            plt3.Title($"Среднее: {pop2.mean}, +/- {pop2.stdErr}");

            var p1 = pop2.min;
            var p2 = pop2.max;
            var p3 = pop2.median; // Медиана
            var p4 = pop2.mean; // Видимо, среднее значение
            var p5 = pop2.stdErr; // Стандартная ошибка
            var p6 = pop2.stDev; // Стандартное отклонение (девиация)

            // You can plot a population
            //plt3.AddPopulation(pop);
            plt3.AddPopulation(pop2);

            // improve the style of the plot
            plt3.XAxis.Ticks(true);
            plt3.XAxis.Grid(true);

            PopulationPlot.Refresh();
            #endregion

            #region Общие статистические данные
            HumansQuantity = CommonDataController.HumansList.Count;
            MenQuantities = _commonDataController.GetFinalText(HumansQuantity, ScopeTypes.Men);

            var min = CommonDataController.HumansList.Min(x => x.DaysLived);
            var minHuman = CommonDataController.HumansList.FirstOrDefault(x => x.DaysLived == min);
            MinimumAge = minHuman.FullYearsLived;
            EarliestHuman = minHuman.LastName + " " +
                            minHuman.FirstName[0..1] + "." +
                           (minHuman.Patronymic != string.Empty ? (minHuman.Patronymic[0..1] + ".") : string.Empty);
            EarliestYears = _commonDataController.GetFinalText(MinimumAge, ScopeTypes.Years);

            //AverageAge = (int)(CommonDataController.HumansList.Average(x => x.DaysLived) / 365);
            AverageAge = (int)p4;
            AverageYears = _commonDataController.GetFinalText(AverageAge, ScopeTypes.Years);

            var max = CommonDataController.HumansList.Max(x => x.DaysLived);
            var maxHuman = CommonDataController.HumansList.FirstOrDefault(x => x.DaysLived == max);
            MaximumAge = maxHuman.FullYearsLived;
            OldestHuman = maxHuman.LastName + " " +
                          maxHuman.FirstName[0..1] + "." +
                         (maxHuman.Patronymic != string.Empty ? (maxHuman.Patronymic[0..1] + ".") : string.Empty);
            OldestYears = _commonDataController.GetFinalText(MaximumAge, ScopeTypes.Years);
            #endregion
        }


        /// <summary>
        /// Обработчик закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HumansListView_Closed(object sender, System.EventArgs e)
        {
            Dispose();
            (sender as Window).Close();
        }

        /// <summary>
        /// Если буду чистить ненужные объекты
        /// </summary>
        public void Dispose()
        {
            
        }











        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="container"></param>
        /// <param name="msSqlController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HumansListViewModel(ILogger logger, IContainer container,
                                   ICommonDataController commonDataController,
                                   IXmlController xmlController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

            _xmlController = xmlController ?? throw new ArgumentNullException(nameof(xmlController));

            InitializeCommands();

            if (CategoriesViewModel.ChangeCategory == null)
            {
                // Делегат обновления списка людей при изменении категорий
                CategoriesViewModel.ChangeCategory = new Action(() =>
                {
                    //TODO Здесь думать, чтобы не обращаться к БД многократно
                    //HumansCollection = Humans.GetHumans();
                    SortHumansCollection();
                    RaisePropertyChanged(nameof(HumansCollection));
                });
            }
        }
        #endregion
    }
}



//private BitmapImage ConvertFromByteArray(byte[] array)
//{
//    if (array == null) return null;

//    BitmapImage myBitmapImage = new BitmapImage();
//    myBitmapImage.BeginInit();
//    myBitmapImage.StreamSource = new MemoryStream(array);
//    myBitmapImage.DecodePixelWidth = 200;
//    myBitmapImage.EndInit();
//    return myBitmapImage;
//}
