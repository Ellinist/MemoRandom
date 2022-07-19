﻿using DryIoc;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemoRandom.Client.Views;
using MemoRandom.Models.Models;
using MemoRandom.Data.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;

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
        private int _personIndex;
        private Human _selectedHuman;
        private BitmapSource _imageSource;
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly IEventAggregator _eventAggregator;
        private readonly IHumansController _humansController;
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
        public List<Human> HumansList
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

                    _humansController.SetCurrentHuman(value);
                    RaisePropertyChanged(nameof(SelectedHuman));

                    ImageSource = _humansController.GetHumanImage();
                    RaisePropertyChanged(nameof(ImageSource));
                }
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Команда запуска окна со списком людей
        /// </summary>
        public DelegateCommand OnStartHumansViewCommand { get; private set; }
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
            OnStartHumansViewCommand = new DelegateCommand(OnStartHumansView);
            AddHumanCommand = new DelegateCommand(AddHuman);
            EditHumanDataCommand = new DelegateCommand(EditHumanData);
            DeleteHumanCommand = new DelegateCommand(DeleteHuman);
            StartAboutCommand = new DelegateCommand(OpenAboutView);
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

        /// <summary>
        /// Запуск окна создания нового человека
        /// </summary>
        private void AddHuman()
        {
            _humansController.SetCurrentHuman(null); // Новая запись - флаг
            _container.Resolve<HumanDetailedView>().ShowDialog(); // Открываем окно создания/редактирования

            HumansList.Clear();
            HumansList = _humansController.GetHumansList();
            var currentHumanId = _humansController.GetCurrentHuman();
            PersonIndex = HumansList.FindIndex(x => x.HumanId == currentHumanId.HumanId); // Прыжок на индекс добавленного человека
            RaisePropertyChanged(nameof(PersonIndex));
        }

        /// <summary>
        /// Вызов окна редактирования выбранного человека
        /// </summary>
        private void EditHumanData()
        {
            _container.Resolve<HumanDetailedView>().ShowDialog();

            HumansList.Clear();
            HumansList = _humansController.GetHumansList();
            var currentHumanId = _humansController.GetCurrentHuman();
            PersonIndex = HumansList.FindIndex(x => x.HumanId == currentHumanId.HumanId); // Прыжок на индекс редактируемого человека
            RaisePropertyChanged(nameof(PersonIndex));
        }

        /// <summary>
        /// Удаление выбранного человека
        /// </summary>
        private void DeleteHuman()
        {
            //TODO здесь удаляем выбранного в списке человека
            var result = MessageBox.Show("Удалить выбранного человека?", "Удаление!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                _humansController.DeleteHuman();

                HumansList.Clear();
                HumansList = _humansController.GetHumansList();
                
                if(HumansList.Count == 0)
                {
                    ImageSource = null;
                }
                else
                {
                    PersonIndex = 0; // Прыгаем на первую запись в списке
                }

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
        /// При открытии окна получаем список всех людей
        /// </summary>
        private void OnStartHumansView()
        {
            Task.Factory.StartNew(() =>
            {
                var result = _humansController.GetHumansList();
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    HumansList = result;
                    RaisePropertyChanged(nameof(HumansList));
                });
            });
        }

        #region CTOR
        public HumansListViewModel(ILogger logger, IContainer container, IEventAggregator eventAggregator, IHumansController humansController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _humansController = humansController ?? throw new ArgumentNullException(nameof(humansController));

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