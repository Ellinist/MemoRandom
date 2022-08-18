using DryIoc;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemoRandom.Client.Views;
using MemoRandom.Models.Models;
using MemoRandom.Data.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;
using System.Text;
using System.Collections.ObjectModel;
using MemoRandom.Client.Enums;

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
        private readonly StringBuilder YearsText = new();
        private string _sortMember;
        private string _sortDirection;

        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly IEventAggregator _eventAggregator;
        private readonly IMsSqlController _msSqlController;
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
            get => Humans.HumansList;
            set
            {
                Humans.HumansList = value;
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
            _sortMember = e.Column.SortMemberPath.ToString();
        }

        /// <summary>
        /// Запуск окна создания нового человека
        /// </summary>
        private void AddHuman()
        {
            Humans.CurrentHuman = null; // Для новой записи!
            _container.Resolve<HumanDetailedView>().ShowDialog(); // Открываем окно создания/редактирования
            
            if (Humans.CurrentHuman != null)
            {
                ResortHumansList(); // Сортировка по условию

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
        }

        /// <summary>
        /// Вызов окна редактирования выбранного человека
        /// </summary>
        private void EditHumanData()
        {
            _container.Resolve<HumanDetailedView>().ShowDialog();

            var id = Humans.CurrentHuman.HumanId;

            ResortHumansList(); // Сортировка по условию

            RaisePropertyChanged(nameof(Humans.HumansList));
            HumansList = Humans.HumansList;

            PersonIndex = HumansList.IndexOf(Humans.CurrentHuman);

            RaisePropertyChanged(nameof(HumansList));
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
            if(result == MessageBoxResult.Yes)
            {
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
        }

        /// <summary>
        /// Сортировка по условию упорядочивания при щелчке на столбце таблицы
        /// </summary>
        /// <returns></returns>
        private void ResortHumansList()
        {
            List<Human> result = new();
            if (_sortMember == null)
            {
                result = Humans.HumansList.OrderBy(x => x.DaysLived).ToList();
            }
            else
            {
                switch (_sortMember)
                {
                    case "LastName":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.LastName).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.LastName).ToList();
                        break;
                    case "FirstName":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.FirstName).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.FirstName).ToList();
                        break;
                    case "Patronymic":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.Patronymic).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.Patronymic).ToList();
                        break;
                    case "BirthDate":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.BirthDate).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.BirthDate).ToList();
                        break;
                    case "BirthCountry":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.BirthCountry).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.BirthCountry).ToList();
                        break;
                    case "BirthPlace":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.BirthPlace).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.BirthPlace).ToList();
                        break;
                    case "DeathDate":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.DeathDate).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.DeathDate).ToList();
                        break;
                    case "DeathCountry":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.DeathCountry).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.DeathCountry).ToList();
                        break;
                    case "DeathPlace":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.DeathPlace).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.DeathPlace).ToList();
                        break;
                    case "DaysLived":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.DaysLived).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.DaysLived).ToList();
                        break;
                    case "FullYearsLived":
                        if (_sortDirection == "Ascending") result = Humans.HumansList.OrderBy(x => x.FullYearsLived).ToList();
                        else result = Humans.HumansList.OrderByDescending(x => x.FullYearsLived).ToList();
                        break;
                }
            }

            Humans.HumansList.Clear();
            foreach (var item in result)
            {
                Humans.HumansList.Add(item);
            }
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
        /// <param name="eventAggregator"></param>
        /// <param name="msSqlController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HumansListViewModel(ILogger logger, IContainer container, IEventAggregator eventAggregator, IMsSqlController msSqlController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
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
