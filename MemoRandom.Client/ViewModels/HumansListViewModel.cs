using DryIoc;
using NLog;
using Prism.Commands;
//using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemoRandom.Client.Views;
using MemoRandom.Models.Models;
using MemoRandom.Data.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;

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
        private List<LifePeriodType> _categoriesList = new();
        private int _personIndex;
        private int _previousIndex = 0; // Индекс предыдущего выбранного узла в списке
        private Human _selectedHuman;
        private BitmapSource _imageSource;
        private string _displayedYears = "";
        private string _humanDeathReasonName;
        private readonly StringBuilder YearsText = new();
        private string _sortMember;
        private string _sortDirection;
        private string _categoryName;
        private int _periodFrom;
        private int _periodTo;

        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly IMsSqlController _msSqlController;

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
        public ObservableCollection<Human> HumansList
        {
            get => _humansList;
            set
            {
                _humansList = value;
                RaisePropertyChanged(nameof(HumansList));
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
                if(HumansList != null && HumansList.Count > 0 && value != null)
                {
                    _selectedHuman = value;

                    // При смене выбранного человека устанавливаем его текущим
                    Humans.CurrentHuman = value;
                    RaisePropertyChanged(nameof(SelectedHuman));

                    // Изменение изображения
                    ImageSource = _msSqlController.GetHumanImage(Humans.CurrentHuman);
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
            get => Reasons.PlainReasonsList;
            set
            {
                Reasons.PlainReasonsList = value;
                RaisePropertyChanged(nameof(PlainReasonsList));
            }
        }

        public List<LifePeriodType> CategoriesList
        {
            get => _categoriesList;
            set
            {
                _categoriesList = value;
                RaisePropertyChanged(nameof(CategoriesList));
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                RaisePropertyChanged(nameof(CategoryName));
            }
        }

        public int PeriodFrom
        {
            get => _periodFrom;
            set
            {
                _periodFrom = value;
                RaisePropertyChanged(nameof(PeriodFrom));
            }
        }

        public int PeriodTo
        {
            get => _periodTo;
            set
            {
                _periodTo = value;
                RaisePropertyChanged(nameof(PeriodTo));
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
            YearsText.Clear();
            int t1, t2;
            t1 = years % 10;
            t2 = years % 100;
            if (t1 == 1 && t2 != 11)
            {
                YearsText.Append("(" + years + " полный год)");
            }
            else if(t1 >= 2 && t1 <= 4 && (t2 < 10 || t2 >= 20))
            {
                YearsText.Append("(" + years + " полных года)");
            }
            else
            {
                YearsText.Append("(" + years + " полных лет)");
            }
            
            DisplayedYears = YearsText.ToString();
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
        
        public DelegateCommand SettingsMenuCommand { get; private set; }
        
        public DelegateCommand HumansListMenuCommand { get; private set; }
        
        public DelegateCommand StartMenuCommand { get; private set; }
        
        public DelegateCommand StartAboutCommand { get; private set; }
        
        public DelegateCommand AddNewHumanCommand { get; private set; }
        #endregion

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            AddHumanCommand = new DelegateCommand(AddHuman);
            EditHumanDataCommand = new DelegateCommand(EditHumanData);
            DeleteHumanCommand = new DelegateCommand(DeleteHuman);
            StartAboutCommand = new DelegateCommand(OpenAboutView);
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

            SortHumansList();
            RaisePropertyChanged(nameof(HumansList));
        }

        /// <summary>
        /// Запуск окна создания нового человека
        /// </summary>
        private void AddHuman()
        {
            Humans.CurrentHuman = null; // Для новой записи!
            _container.Resolve<HumanDetailedView>().ShowDialog(); // Открываем окно создания/редактирования

            if (Humans.CurrentHuman == null) return;
            
            HumansList.Add(Humans.CurrentHuman);
            SortHumansList(); // Сортировка по условию
            
            SelectedHuman = Humans.CurrentHuman;
            PersonIndex = HumansList.IndexOf(Humans.CurrentHuman);
            RaisePropertyChanged(nameof(PersonIndex));

            ImageSource = _msSqlController.GetHumanImage(Humans.CurrentHuman);
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
            
            SortHumansList(); // Сортировка по условию

            PersonIndex = HumansList.IndexOf(Humans.CurrentHuman);
            RaisePropertyChanged(nameof(PersonIndex));

            ImageSource = _msSqlController.GetHumanImage(Humans.CurrentHuman);
            RaisePropertyChanged(nameof(ImageSource));

            var currentReason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == SelectedHuman.DeathReasonId);
            if (currentReason != null)
            {
                HumanDeathReasonName = currentReason.ReasonName;
            }
        }

        /// <summary>
        /// Удаление выбранного человека
        /// </summary>
        private async void DeleteHuman()
        {
            var result = MessageBox.Show("Удалить выбранного человека?", "Удаление!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            
            var formerId = HumansList[_previousIndex].HumanId;
            try
            {
                await Task.Run(() =>
                {
                    _msSqlController.DeleteHuman(SelectedHuman); // Удаление во внешнем хранилище
                });
                HumansList.Remove(SelectedHuman); // Удаление в списке
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось Удалить!\n Код ошибки в журнале", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                _logger.Error($"Ошибка: {ex}");
            }
            PersonIndex = HumansList.IndexOf(HumansList.FirstOrDefault(x => x.HumanId == formerId));
            RaisePropertyChanged(nameof(PersonIndex));
        }

        /// <summary>
        /// Сортировка по условию упорядочивания при щелчке на столбце таблицы
        /// </summary>
        /// <returns></returns>
        private void SortHumansList()
        {
            List<Human> result;
            if (_sortMember == null)
            {
                result = HumansList.OrderBy(x => x.DaysLived).ToList();
            }
            else
            {
                var param = _sortMember;
                var propertyInfo = typeof(Human).GetProperty(param);
                // Создаем новую сущность, упорядоченную по столбцу сортировки
                result = (_sortDirection == null || _sortDirection == "Ascending") ?
                    HumansList.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList() :
                    HumansList.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
            }

            HumansList.Clear();
            foreach (var item in result)
            {
                HumansList.Add(item);
            }
            RaisePropertyChanged(nameof(HumansList));
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
        public async void HumansListView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<Human> result = new(); // Результирующая коллекция людей
                await Task.Run(() =>
                {
                    result = _msSqlController.GetHumans(); // Получаем из внешнего источника

                    HumansList = result;
                });

                RaisePropertyChanged(nameof(HumansList));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось прочитать данные!\n Код ошибки в журнале", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                _logger.Error($"Ошибка: {ex}");
            }
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
        public HumansListViewModel(ILogger logger, IContainer container, IMsSqlController msSqlController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));

            InitializeCommands();
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
