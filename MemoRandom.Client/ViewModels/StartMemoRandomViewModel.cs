using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DryIoc;
using MemoRandom.Client.Views;
using MemoRandom.Data.Interfaces;
using MemoRandom.Data.Repositories;
using Microsoft.Data.SqlClient;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace MemoRandom.Client.ViewModels
{
    public class StartMemoRandomViewModel : BindableBase
    {
        public Action ButtonsVisibility { get; set; } // Действие видимости кнопок

        #region PRIVATE FIELDS
        private static readonly string _dbConfigName = "MsDbConfig"; // Конфигурация БД
        private static readonly string _dbFolderName = "MsDbFolder"; // Конфигурация папки с БД
        private static readonly string _imageFolderName = "ImagePathConfig"; // Конфигурация папки с изображениями
        private string _startViewTitleDefault = "Memo-Random"; // Дефолтный заголовок стартового окна
        private string _reasonsButtonName = "Справочник";
        private string _humansButtonName = "Memo-Random";
        private DateTime _currentDateTime = DateTime.Now;
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly IReasonsController _reasonsController;
        private bool _active = true; // Флаг активности текущего окна
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок основного окна
        /// </summary>
        public string StartViewHeader
        {
            get => _startViewTitleDefault;
            set
            {
                if (_startViewTitleDefault != value)
                {
                    _startViewTitleDefault = value;
                    RaisePropertyChanged(nameof(StartViewHeader));
                }
            }
        }
        /// <summary>
        /// Название кнопки вызова справочника причин смерти
        /// </summary>
        public string ReasonsButtonName
        {
            get => _reasonsButtonName;
            set
            {
                _reasonsButtonName = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Название кнопки вызова основного рабочего окна
        /// </summary>
        public string HumansButtonName
        {
            get => _humansButtonName;
            set
            {
                _humansButtonName = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Текущая дата/время
        /// </summary>
        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                _currentDateTime = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region COMMANDS
        ///// <summary>
        ///// Команда подключения к базе данных
        ///// </summary>
        //public DelegateCommand ConnectDbCommand { get; private set; }
        /// <summary>
        /// Команда открытия окна справочника причин смерти
        /// </summary>
        public DelegateCommand OpenReasonsViewCommand { get; private set; }
        /// <summary>
        /// Команда открытия окна по люядм
        /// </summary>
        public DelegateCommand OpenHumansViewCommand { get; private set; }
        /// <summary>
        /// Команда загрузки окна
        /// </summary>
        public DelegateCommand<object> OnLoadedStartMemoRandomViewCommand { get; private set; }
        #endregion

        #region COMMAND IMPLEMENTATION
        /// <summary>
        /// Открытие окна справочника причин смерти
        /// </summary>
        private void OpenReasonsView()
        {
            _active = false;
            _container.Resolve<ReasonsView>().ShowDialog();
            _active = true;
            SetCurrentDateTime(); // Какой ужас - надо как-то по-другому делать!
        }

        /// <summary>
        /// Открытие основного окна работы программы
        /// </summary>
        private void OpenHumansView()
        {
            _active = false;
            _container.Resolve<HumansListView>().ShowDialog();
            _active = true;
            SetCurrentDateTime(); // Какой ужас - надо как-то по-другому делать!
        }
        
        /// <summary>
        /// Открытие главного окна программы
        /// </summary>
        private void OnLoadedStartMemoRandomView(object param)
        {
            if (param is Window)
            {
                SetInitialPaths(); // Начальная инициализация БД и путей
                GetInitialReasons(); // Получаем справочник причин смерти

                (param as Window).Closing += StartMemoRandomViewModel_Closing; // Подписываемся на событие закрытия окна
                SetCurrentDateTime(); // Вызываем метод отображения текущего времени
            }
        }
        /// <summary>
        /// Обработчик системной кнопки закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartMemoRandomViewModel_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Выйти из программы?", "Выход из программы!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                e.Cancel = false; // Окно не закрываем - отмена действия
            }
            else
            {
                e.Cancel = true; // Окно закрывается
            }
        }
        #endregion

        /// <summary>
        /// Установка текущей даты времени с шагом в одну секунду
        /// </summary>
        private void SetCurrentDateTime()
        {
            Task.Factory.StartNew(() =>
            {
                while (_active)
                {
                    Thread.Sleep(1000);
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        CurrentDateTime = DateTime.Now;
                    });
                }
            });
        }

        /// <summary>
        /// Инициализация команд стартового окна
        /// </summary>
        private void InitializeCommands()
        {
            OnLoadedStartMemoRandomViewCommand = new DelegateCommand<object>(OnLoadedStartMemoRandomView); // Загрузка окна
            OpenReasonsViewCommand = new DelegateCommand(OpenReasonsView); // Команда открытия окна причин смерти
            OpenHumansViewCommand = new DelegateCommand(OpenHumansView); // Команда открытия основного окна
        }

        /// <summary>
        /// Метод инициализации базы данных и папок хранения БД и изображений
        /// </summary>
        private void SetInitialPaths()
        {
            string dbfilename = ConfigurationManager.AppSettings[_dbConfigName]; // Получение имени файла базы данных
            if (dbfilename == null) return; // Если в файле конфигурации нет имени БД, то выходим (ничего не делаем)
            string dbfilepath = ConfigurationManager.AppSettings[_dbFolderName]; // Получаем имя папки, в которой лежит БД
            if (dbfilepath == null) return; // Если в файле конфигурации нет имени папки, то выходим (ничего не делаем)
            string imagefilepath = ConfigurationManager.AppSettings[_imageFolderName]; // Имя папки с изображениями
            if(imagefilepath == null) return; // Если в файле конфигурации нет имени папки, то выходим (ничего не делаем)

            // Проверяем, существует ли папка хранения БД - только для случая генерации БД
            var dbBaseDirectory = AppDomain.CurrentDomain.BaseDirectory + dbfilepath;
            if (!Directory.Exists(dbBaseDirectory))
            {
                Directory.CreateDirectory(dbBaseDirectory);
            }

            // Проверяем, существует ли папка хранения изображений
            HumansRepository.ImageFolder = AppDomain.CurrentDomain.BaseDirectory + imagefilepath;
            if (!Directory.Exists(HumansRepository.ImageFolder))
            {
                Directory.CreateDirectory(HumansRepository.ImageFolder);
            }

            string combinedPath = Path.Combine(dbBaseDirectory, dbfilename);
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = @"Kotarius\KotariusServer",
                AttachDBFilename = combinedPath,
                InitialCatalog = Path.GetFileNameWithoutExtension(combinedPath),
                IntegratedSecurity = true
            };
            HumansRepository.DbConnectionString = connectionStringBuilder.ConnectionString;
        }

        /// <summary>
        /// Чтение справочника причин смерти
        /// </summary>
        private void GetInitialReasons()
        {
            Task.Factory.StartNew(() =>
            {
                var result = _reasonsController.GetReasonsList();
                if (result)
                {
                    ButtonsVisibility(); // Чтение справоника выполнено - кнопки делаем видимыми
                }
                else
                {
                    MessageBox.Show("Чтение справочника причин смерти не удалось!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="container"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public StartMemoRandomViewModel(ILogger logger, IContainer container, IReasonsController reasonsController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _reasonsController = reasonsController ?? throw new ArgumentNullException(nameof(reasonsController));

            InitializeCommands();
        }
        #endregion
    }
}
