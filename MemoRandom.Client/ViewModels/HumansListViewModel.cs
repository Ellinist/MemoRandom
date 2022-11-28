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

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления основного окна со списком людей
    /// </summary>
    public class HumansListViewModel : BindableBase, IDisposable
    {
        #region PRIVATE FIELDS
        private string _humansViewTitle = "Начало";
        private ObservableCollection<Human> _humansList;
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

        private CultureInfo cultureInfo = new CultureInfo("ru-RU");
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна справочника причин смерти
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
            get => _humansList;
            set
            {
                _humansList = value;
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
                    ImageSource = _commonDataController.GetHumanImage(CommonDataController.CurrentHuman);
                    RaisePropertyChanged(nameof(ImageSource));

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
        #endregion

        #region Частные методы
        /// <summary>
        /// Формирование текстов для отображения прожитых лет в соответствии с числом
        /// </summary>
        /// <param name="selectedHuman"></param>
        private void SetFullYearsText(Human selectedHuman)
        {
            int years = (int)Math.Floor(selectedHuman.FullYearsLived); // Считаем число полных лет
            _yearsText.Clear();

            _yearsText.Append("(" + years + " " + _commonDataController.GetFinalText(years, PeriodTypes.Years) + ")");

            DisplayedYears = _yearsText.ToString();
        }

        /// <summary>
        /// Событие сортировки по заголовку столбца
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DgHumans_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            _sortDirection = e.Column.SortDirection.ToString();
            _sortMember = e.Column.SortMemberPath;

            SortHumansCollection();
            RaisePropertyChanged(nameof(HumansCollection));
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

            HumansCollection.Clear();
            foreach (var item in result)
            {
                HumansCollection.Add(item);
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
                    _commonDataController.DeleteHumanInRepository(SelectedHuman, SelectedHuman.ImageFile); // Удаление во внешнем хранилище
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HumansListView_Loaded(object sender, RoutedEventArgs e)
        {
            HumansCollection = CommonDataController.HumansList;
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
                                   ICommonDataController commonDataController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

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
