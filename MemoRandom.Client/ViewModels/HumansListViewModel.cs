using DryIoc;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using MemoRandom.Client.Views;
using MemoRandom.Models.Models;
using MemoRandom.Data.Interfaces;
using System.Windows.Media.Imaging;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления основного окна со списком людей
    /// </summary>
    public class HumansListViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private string _humansViewTitle = "Начало";
        private List<Human> _humansList = new();
        private int _humansIndex = 0;
        private BitmapImage _humanImage;
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly IEventAggregator _eventAggregator;
        private readonly IMemoRandomDbController _dbController;
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

        public List<Human> HumansList
        {
            get => _humansList;
            set
            {
                _humansList = value;
                RaisePropertyChanged(nameof(HumansList));
            }
        }

        public int HumansIndex
        {
            get => _humansIndex;
            set
            {
                _humansIndex = value;
                //HumanImage = HumansList[value].HumanImage;
                RaisePropertyChanged(nameof(HumansIndex));
            }
        }

        public BitmapImage HumanImage
        {
            get => _humanImage;
            set
            {
                _humanImage = value;
                RaisePropertyChanged(nameof(HumanImage));
            }
        } 
        #endregion

        //private readonly IEventAggregator _eventAggregator;
        //private readonly SubscriptionToken _messageHeader; // Подумать - нужен ли?
        //private readonly SubscriptionToken _reasonsDictionaryChanging;
        //private readonly SubscriptionToken _humansDataFileChanging;

        #region Commands
        public DelegateCommand OnStartHumansViewCommand { get; private set; }
        public DelegateCommand AddHumanMenuCommand { get; private set; }
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
            OnStartHumansViewCommand = new DelegateCommand(OnStartHumansView);
            AddHumanMenuCommand = new DelegateCommand(AddHumanMenu);
            //SettingsMenuCommand = new DelegateCommand(OpenSettingsView);
            //StartMenuCommand = new DelegateCommand(OpenStartView);
            StartAboutCommand = new DelegateCommand(OpenAboutView);
            //HumansListMenuCommand = new DelegateCommand(OnHumansListMenuCommand);
            //AddNewHumanCommand = new DelegateCommand(OnAddNewHumanCommand);
        }

        /// <summary>
        /// Запуск окна создания нового человека
        /// </summary>
        private void AddHumanMenu()
        {
            _container.Resolve<HumanDetailedView>().ShowDialog();
        }

        /// <summary>
        /// Открытие окна "О программе"
        /// </summary>
        private void OpenAboutView()
        {
            _container.Resolve<AboutView>().ShowDialog();
        }

        /// <summary>
        /// При открытии окна получаем список всех людей
        /// </summary>
        private void OnStartHumansView()
        {
            HumansList = _dbController.GetHumasList();

            ////TODO Этот метод переписать для того, чтобы не плодить в безумном количестве сущности - подумать
            //HumanImage = new BitmapImage();
            //using (var mem = new MemoryStream(HumansList[HumansIndex].HumanImage))
            //{
            //    mem.Position = 0;
            //    HumanImage.BeginInit();
            //    HumanImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            //    HumanImage.CacheOption = BitmapCacheOption.OnLoad;
            //    HumanImage.UriSource = null;
            //    HumanImage.StreamSource = mem;
            //    HumanImage.EndInit();
            //}
            //HumanImage.Freeze();
        }

        #region CTOR
        public HumansListViewModel(ILogger logger, IContainer container, IEventAggregator eventAggregator, IMemoRandomDbController dbController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dbController = dbController ?? throw new ArgumentNullException(nameof(dbController));

            //Title = HeaderDefault;
            ////_humansFile = ConfigurationManager.AppSettings["HumansPath"];

            //_messageHeader = _eventAggregator.GetEvent<ChangeViewHeaderEvent>().Subscribe(OnChangeHeader, ThreadOption.PublisherThread);
            //_reasonsDictionaryChanging = eventAggregator.GetEvent<ChangeReasonsDictionaryEvent>().Subscribe(OnReasonsDictionaryChanged, ThreadOption.PublisherThread);
            //_humansDataFileChanging = eventAggregator.GetEvent<ChangeHumansDataFile>().Subscribe(OnChangeHumansDataFile, ThreadOption.PublisherThread);

            InitializeCommands();

            //OnStartHumansMainView();
        }
        #endregion
    }
}