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
using System.Windows.Controls;
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
        private int _humansIndex = 0;
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
        public int HumansIndex
        {
            get => _humansIndex;
            set
            {
                if (HumansList == null || HumansList.Count == 0) return;
                _humansIndex = value;
                if (value == -1)
                {
                    _humansController.SetCurrentHuman(null);
                }
                else
                {
                    _humansController.SetCurrentHuman(HumansList[value]);
                }
                RaisePropertyChanged(nameof(HumansIndex));
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

        /// <summary>
        /// Запуск окна создания нового человека
        /// </summary>
        private void AddHuman()
        {
            _humansController.SetCurrentHuman(null);
            _container.Resolve<HumanDetailedView>().ShowDialog();

            HumansList.Clear();
            HumansList = _humansController.GetHumansList();
            HumansIndex = 0; // Прыгаем на первую запись в списке
        }

        /// <summary>
        /// Вызов окна редактирования выбранного человека
        /// </summary>
        private void EditHumanData()
        {
            var prevIndex = HumansIndex;
            _container.Resolve<HumanDetailedView>().ShowDialog();
            HumansIndex = prevIndex;
        }

        private void DeleteHuman()
        {
            //TODO здесь удаляем выбранного в списке человека
            var result = MessageBox.Show("Удалить выбранного человека?", "Удаление!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                _humansController.DeleteHuman(_humansController.GetCurrentHuman());

                HumansList.Clear();
                HumansList = _humansController.GetHumansList();
                HumansIndex = 0; // Прыгаем на первую запись в списке
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

            HumansIndex = 0;
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