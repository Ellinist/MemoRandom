using NLog;
using System;
using DryIoc;
using Prism.Mvvm;
using System.Windows;
using System.Threading;
using System.Configuration;
using System.Threading.Tasks;
using MemoRandom.Client.Views;
using System.Windows.Threading;
using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления стартового окна
    /// </summary>
    public class StartMemoRandomViewModel : BindableBase
    {
        public Action ButtonsVisibility { get; set; } // Действие видимости кнопок

        #region PRIVATE FIELDS
        private static string _storageFileName;   // Название файла хранения информации (БД или Xml)
        private static string _storageFilePath;   // Имя папки, где хранится информация
        private static string _storageImagesPath; // Имя папки, где хранятся изображения

        private static readonly string _dbConfigName    = "MsDbConfig"; // Конфигурация БД
        private static readonly string _dbFolderName    = "MsDbFolder"; // Конфигурация папки с БД
        private static readonly string _imageFolderName = "ImagePathConfig"; // Конфигурация папки с изображениями
        private string _startViewTitleDefault           = "Memo-Random"; // Дефолтный заголовок стартового окна
        private string _reasonsButtonName               = "Справочник";
        private string _humansButtonName                = "Memo-Random";
        private DateTime _currentDateTime               = DateTime.Now;
        
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly IMsSqlController _msSqlController;
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

        #region COMMAND IMPLEMENTATION
        /// <summary>
        /// Открытие окна со списком людей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HumansButton_Click(object sender, RoutedEventArgs e)
        {
            _active = false;
            _container.Resolve<HumansListView>().ShowDialog();
            _active = true;
            SetCurrentDateTime(); // Какой ужас - надо как-то по-другому делать!
        }

        /// <summary>
        /// Открытие окна справочника причин смерти
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReasonsButton_Click(object sender, RoutedEventArgs e)
        {
            _active = false;
            _container.Resolve<ReasonsView>().ShowDialog();
            _active = true;
            SetCurrentDateTime(); // Какой ужас - надо как-то по-другому делать!
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

        /// <summary>
        /// Загрузка стартового окна программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window)
            {
                SetInitialPaths();     // Начальная инициализация БД (или любой другой фигни) и путей
                //GetInitialReasons();   // Получаем справочник причин смерти
                //GetHumansCategories(); // Чтение возрастных категорий
                ReadStartData();

                (sender as Window).Closing += StartMemoRandomViewModel_Closing; // Подписываемся на событие закрытия окна
                SetCurrentDateTime(); // Вызываем метод отображения текущего времени
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
        /// Метод инициализации базы данных и папок хранения БД и изображений
        /// </summary>
        private void SetInitialPaths()
        {
            _storageFileName = ConfigurationManager.AppSettings[_dbConfigName]; // Получение имени файла хранилища информации
            if (_storageFileName == null) return; // Если в файле конфигурации нет имени хранилища, то выходим (ничего не делаем)
            _storageFilePath = ConfigurationManager.AppSettings[_dbFolderName]; // Получаем имя папки, в которой лежит хранилище
            if (_storageFilePath == null) return; // Если в файле конфигурации нет имени папки, то выходим (ничего не делаем)
            _storageImagesPath = ConfigurationManager.AppSettings[_imageFolderName]; // Имя папки с изображениями
            if(_storageImagesPath == null) return; // Если в файле конфигурации нет имени папки, то выходим (ничего не делаем)

            #region Вместо интерфейса вызова БД можно использовать интерфейс работы с XML-файлами
            // ВНИМАНИЕ! В параметрах есть имя сервера - только для работы с БД
            var res = _msSqlController.SetPaths(_storageFileName, _storageFilePath, _storageImagesPath, @"Kotarius\KotariusServer");
            if (!res)
            {
                MessageBox.Show("Ошибка установки соединения с хранилищем информации!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            #endregion
        }

        ///// <summary>
        ///// Чтение справочника причин смерти
        ///// </summary>
        //private async void GetInitialReasons()
        //{
        //    await Task.Run(() =>
        //    {
        //        List<Reason> reasonsResult = _msSqlController.GetReasons();
        //        if (reasonsResult != null)
        //        {
        //            Reasons.PlainReasonsList = reasonsResult; // Заносим плоский список в статический класс
        //            FormObservableCollection(Reasons.PlainReasonsList, null); // Формируем иерархическую коллекцию
        //        }
        //        else
        //        {
        //            MessageBox.Show("Чтение справочника причин смерти не удалось!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    });
        //}

        ///// <summary>
        ///// Чтение коллекции возрастных категорий
        ///// </summary>
        //private async void GetHumansCategories()
        //{
        //    await Task.Run(() =>
        //    {
        //        ObservableCollection<Category> categoriesResult = _msSqlController.GetCategories();
        //        if (categoriesResult != null)
        //        {
        //            Categories.AgeCategories = categoriesResult;
        //        }
        //        else
        //        {
        //            MessageBox.Show("Чтение возрастных категорий не удалось!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    });
        //}

        /// <summary>
        /// Чтение справочника причин смерти и справочника возрастных категорий
        /// </summary>
        private async void ReadStartData()
        {
            await Task.Run(() =>
            {
                List<Reason> reasonsResult = _msSqlController.GetReasons();
                ObservableCollection<Category> categoriesResult = _msSqlController.GetCategories();
                if (reasonsResult != null && categoriesResult != null)
                {
                    Reasons.PlainReasonsList = reasonsResult; // Заносим плоский список в статический класс
                    FormObservableCollection(Reasons.PlainReasonsList, null); // Формируем иерархическую коллекцию
                    Categories.AgeCategories = categoriesResult; // Задаем статический список категорий
                    ButtonsVisibility();   // Чтение данных выполнено - кнопки делаем видимыми
                }
                else if(reasonsResult == null)
                {
                    MessageBox.Show("Чтение справочника причин смерти не удалось!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Чтение возрастных категорий не удалось!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        /// <summary>
        /// Формирование иерархической коллекции
        /// </summary>
        /// <param name="reasons">Плоский список</param>
        /// <param name="headReason">Головной элемент (экземпляр класса)</param>
        private void FormObservableCollection(List<Reason> reasons, Reason headReason)
        {
            for (int i = 0; i < reasons.Count; i++)
            {
                if (reasons[i].ReasonParentId == Guid.Empty) // Случай корневых узлов
                {
                    Reason rsn = new()
                    {
                        ReasonParentId    = reasons[i].ReasonParentId,
                        ReasonId          = reasons[i].ReasonId,
                        ReasonName        = reasons[i].ReasonName,
                        ReasonComment     = reasons[i].ReasonComment,
                        ReasonDescription = reasons[i].ReasonDescription
                    };
                    Reasons.ReasonsCollection.Add(rsn);

                    // Проверка на наличие дочерних узлов
                    List<Reason> daughters = Reasons.PlainReasonsList.FindAll(x => x.ReasonParentId == rsn.ReasonId);
                    if (daughters.Count != 0) // Если дочерние узлы найдены
                    {
                        FormObservableCollection(daughters, rsn); // Вызываем рекурсивно
                    }
                }
                else if (headReason != null)// Случай вложенных узлов
                {
                    Reason rsn = new()
                    {
                        ReasonId          = reasons[i].ReasonId,
                        ReasonName        = reasons[i].ReasonName,
                        ReasonComment     = reasons[i].ReasonComment,
                        ReasonDescription = reasons[i].ReasonDescription,
                        ReasonParentId    = headReason.ReasonId,
                        ReasonParent      = headReason
                    };
                    headReason.ReasonChildren.Add(rsn);

                    // Проверка на наличие дочерних узлов
                    List<Reason> daughters = Reasons.PlainReasonsList.FindAll(x => x.ReasonParentId == rsn.ReasonId);
                    if (daughters.Count != 0) // Если дочерние узлы найдены
                    {
                        FormObservableCollection(daughters, rsn); // Вызываем рекурсивно
                    }
                }
            }
        }








        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="container"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public StartMemoRandomViewModel(ILogger logger, IContainer container, IMsSqlController msSqlController)
        {
            _logger          = logger ?? throw new ArgumentNullException(nameof(logger));
            _container       = container ?? throw new ArgumentNullException(nameof(container));
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));
        }
        #endregion
    }
}
