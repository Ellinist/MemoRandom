using DryIoc;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления основного окна со списком людей
    /// </summary>
    public class HumansViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private string _humansViewTitle = "Начало";
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly IEventAggregator _eventAggregator;
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
        #endregion

        //private readonly IEventAggregator _eventAggregator;
        //private readonly SubscriptionToken _messageHeader; // Подумать - нужен ли?
        //private readonly SubscriptionToken _reasonsDictionaryChanging;
        //private readonly SubscriptionToken _humansDataFileChanging;

        #region Commands
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
            //SettingsMenuCommand = new DelegateCommand(OpenSettingsView);
            //StartMenuCommand = new DelegateCommand(OpenStartView);
            //StartAboutCommand = new DelegateCommand(OpenAboutView);
            //HumansListMenuCommand = new DelegateCommand(OnHumansListMenuCommand);
            //AddNewHumanCommand = new DelegateCommand(OnAddNewHumanCommand);
        }

        #region CTOR
        public HumansViewModel(ILogger logger, IContainer container, IEventAggregator eventAggregator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

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