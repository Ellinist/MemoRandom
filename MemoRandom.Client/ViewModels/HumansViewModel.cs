using System;
using DryIoc;
using MemoRandom.Client.UserControls;
using MemoRandom.Models.Common;
using MemoRandom.Models.Events;
using Prism.Mvvm;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace MemoRandom.Client.ViewModels
{
    public class HumansViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private string _humansViewTitle = "Начало";
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer? _container; // Контейнер
        private readonly IRegionManager _regionManager; // Менеджер регионов
        private readonly IEventAggregator _eventAggregator;

        private IRegion _menuRegion;
        private IRegion _workRegion;
        private object _menu;
        private object _work;
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

        #region Commands
        public DelegateCommand SettingsMenuCommand { get; private set; }
        public DelegateCommand HumansListMenuCommand { get; private set; }
        public DelegateCommand StartMenuCommand { get; private set; }
        public DelegateCommand StartAboutCommand { get; private set; }
        public DelegateCommand AddNewHumanCommand { get; private set; }
        #endregion

        ///// <summary>
        ///// Метод смены заголовка окна
        ///// </summary>
        ///// <param name="message"></param>
        //private void OnChangeHeader(string message)
        //{
        //    if (!string.IsNullOrEmpty(message))
        //    {
        //        Title = HeaderDefault + " " + message;
        //    }
        //    else
        //    {
        //        Title = HeaderDefault;
        //    }
        //}

        ///// <summary>
        ///// Вызов окна списка людей
        ///// </summary>
        //private void OnHumansListMenuCommand()
        //{
        //    if (_menuRegion.Views.Any())
        //    {
        //        _menuRegion.RemoveAll();
        //        _menu = _container.Resolve<HumansListUserControl>(); // На замену на меню списка людей
        //        _menuRegion.Add(_menu);
        //    }

        //    if (_workRegion.Views.Any())
        //    {
        //        _workRegion.RemoveAll();

        //        _work = _container.Resolve<HumansView>();
        //        _workRegion.Add(_work);
        //    }
        //}

        ///// <summary>
        ///// Вызов окна добавления человека
        ///// </summary>
        //private void OnAddNewHumanCommand()
        //{
        //    if (_menuRegion.Views.Any())
        //    {
        //        _menuRegion.RemoveAll();
        //        _menu = _container.Resolve<HumansListUserControl>(); // На замену на меню списка людей
        //        _menuRegion.Add(_menu);
        //    }

        //    if (_workRegion.Views.Any())
        //    {
        //        _workRegion.RemoveAll();

        //        _work = _container.Resolve<HumanInformationView>();
        //        _workRegion.Add(_work);
        //        //_workRegion.Context = new HumanInformationViewModel(_eventAggregator);
        //    }
        //}

        ///// <summary>
        ///// Выбор меню настроек
        ///// </summary>
        //private void OpenSettingsView()
        //{
        //    if (_menuRegion.Views.Any())
        //    {
        //        _menuRegion.RemoveAll();
        //        _menu = _container.Resolve<SettingsMenuUserControl>();
        //        _menuRegion.Add(_menu);
        //    }

        //    if (_workRegion.Views.Any())
        //    {
        //        _workRegion.RemoveAll();

        //        _work = _container.Resolve<SettingsView>();
        //        _workRegion.Add(_work);
        //    }
        //}

        ///// <summary>
        ///// Возврат к стартовому состоянию
        ///// </summary>
        //private void OpenStartView()
        //{
        //    if (_menuRegion.Views.Any())
        //    {
        //        _menuRegion.RemoveAll();
        //        _menu = _container.Resolve<StartMenuUserControl>();
        //        _menuRegion.Add(_menu);
        //    }

        //    if (_workRegion.Views.Any())
        //    {
        //        _workRegion.RemoveAll();

        //        _work = _container.Resolve<StartPageUserControl>();
        //        _workRegion.Add(_work);
        //    }

        //    var tempevent = _eventAggregator.GetEvent<ChangeViewHeaderEvent>();
        //    tempevent.Publish(String.Empty);
        //}

        /// <summary>
        /// Окно "О программе"
        /// </summary>
        private void OpenAboutView()
        {
            //TODO Заменить на вызов окна "О программе"
            //_dialogServices.ShowNotificationDialog("Test!", "This is test message", DialogTypesEnum.DialogNitificationType);

            //MessageBox.Show("Test", "Test1");
        }

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

        /// <summary>
        /// Инициализация регионов by UC
        /// </summary>
        private void InitializeRegions()
        {
            _menuRegion = _regionManager.Regions[RegionNames.MenuRegion];
            if (_menuRegion != null)
            {
                _menu = _container.Resolve<HumansMainMenuUserControl>();
                _menuRegion.Add(_menu);
            }

            _workRegion = _regionManager.Regions[RegionNames.WorkRegion];
            if (_menuRegion != null)
            {
                _work = _container.Resolve<MainPageUserControl>();
                _workRegion.Add(_work);
            }

            var tempevent = _eventAggregator.GetEvent<ChangeViewHeaderEvent>(); // Смена заголовка окна
            tempevent.Publish(string.Empty);
        }

        #region CTOR
        public HumansViewModel(ILogger logger, IContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            InitializeRegions();
            InitializeCommands();
        }
        #endregion
    }
}
