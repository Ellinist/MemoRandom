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

        private string _sortingColumn;

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
            Humans.CurrentHuman = null;
            //_previousIndex = PersonIndex;
            _container.Resolve<HumanDetailedView>().ShowDialog(); // Открываем окно создания/редактирования
            if (Humans.CurrentHuman != null)
            {
                HumansList = Humans.HumansList;

                SelectedHuman = Humans.CurrentHuman;
                RaisePropertyChanged(nameof(HumansList));
                PersonIndex = HumansList.IndexOf(Humans.CurrentHuman);
                RaisePropertyChanged(nameof(PersonIndex));

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
            //var temp = PersonIndex;
            _container.Resolve<HumanDetailedView>().ShowDialog();

            var id = Humans.CurrentHuman.HumanId;

            switch (_sortMember)
            {
                case "LastName":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.LastName);
                    else Humans.HumansList.OrderByDescending(x => x.LastName);
                    break;
                case "FirstName":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.FirstName);
                    else Humans.HumansList.OrderByDescending(x => x.FirstName);
                    break;
                case "Patronymic":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.Patronymic);
                    else Humans.HumansList.OrderByDescending(x => x.Patronymic);
                    break;
                case "BirthDate":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.BirthDate);
                    else Humans.HumansList.OrderByDescending(x => x.BirthDate);
                    break;
                case "BirthCountry":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.BirthCountry);
                    else Humans.HumansList.OrderByDescending(x => x.BirthCountry);
                    break;
                case "BirthPlace":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.BirthPlace);
                    else Humans.HumansList.OrderByDescending(x => x.BirthPlace);
                    break;
                case "DeathDate":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.DeathDate);
                    else Humans.HumansList.OrderByDescending(x => x.DeathDate);
                    break;
                case "DeathCountry":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.DeathCountry);
                    else Humans.HumansList.OrderByDescending(x => x.DeathCountry);
                    break;
                case "DeathPlace":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.DeathPlace);
                    else Humans.HumansList.OrderByDescending(x => x.DeathPlace);
                    break;
                case "DaysLived":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.DaysLived);
                    else Humans.HumansList.OrderByDescending(x => x.DaysLived);
                    break;
                case "FullYearsLived":
                    if (_sortDirection == "Ascending") Humans.HumansList.OrderBy(x => x.FullYearsLived);
                    else Humans.HumansList.OrderByDescending(x => x.FullYearsLived);
                    break;
            }

            var interoperationList = Humans.HumansList.OrderBy(s => s.LastName).ToList();

            Humans.HumansList.Clear();
            foreach (var item in interoperationList)
            {
                Humans.HumansList.Add(item);
            }

            RaisePropertyChanged(nameof(Humans.HumansList));
            HumansList = Humans.HumansList;

            PersonIndex = HumansList.IndexOf(Humans.CurrentHuman);

            RaisePropertyChanged(nameof(HumansList));
            RaisePropertyChanged(nameof(PersonIndex));

            var currentReason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == SelectedHuman.DeathReasonId);
            if (currentReason != null)
            {
                HumanDeathReasonName = currentReason.ReasonName;
            }
        }

        /// <summary>
        /// Удаление выбранного человека
        /// </summary>
        private void DeleteHuman()
        {
            //TODO здесь удаляем выбранного в списке человека
            var t = PersonIndex;
            var t1 = _previousIndex;

            var result = MessageBox.Show("Удалить выбранного человека?", "Удаление!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                var temp = _previousIndex;
                
                _msSqlController.DeleteHuman(SelectedHuman);
                HumansList.Remove(SelectedHuman);

                PersonIndex = temp;

                RaisePropertyChanged(nameof(PersonIndex));
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
            var dg = (sender as Window).Content;

            ObservableCollection<Human> result = new();
            await Task.Run(() =>
            {
                result = _msSqlController.GetHumans();

                Humans.HumansList = result;
                HumansList = result;
            });


            var tt = Humans.HumansList;
            RaisePropertyChanged(nameof(HumansList));
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
        public HumansListViewModel(ILogger logger, IContainer container, IEventAggregator eventAggregator, IMsSqlController msSqlController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));

            //Title = HeaderDefault;
            ////_humansFile = ConfigurationManager.AppSettings["HumansPath"];

            //_messageHeader = _eventAggregator.GetEvent<ChangeViewHeaderEvent>().Subscribe(OnChangeHeader, ThreadOption.PublisherThread);
            //_reasonsDictionaryChanging = eventAggregator.GetEvent<ChangeReasonsDictionaryEvent>().Subscribe(OnReasonsDictionaryChanged, ThreadOption.PublisherThread);
            //_humansDataFileChanging = eventAggregator.GetEvent<ChangeHumansDataFile>().Subscribe(OnChangeHumansDataFile, ThreadOption.PublisherThread);

            //Humans.HumansList = new();

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
