using DryIoc;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Views;
using NLog;
using Prism.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления стартового окна
    /// </summary>
    public class StartMemoRandomViewModel : BindableBase, IDisposable
    {
        public Action ButtonsVisibility { get; set; } // Действие видимости кнопок

        #region PRIVATE FIELDS
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken token;

        private static string _storageFileName;   // Название файла хранения информации (БД или Xml)
        private static string _storageFilePath;   // Имя папки, где хранится информация
        private static string _storageImagesPath; // Имя папки, где хранятся изображения

        private string _startViewTitleDefault           = "Memo-Random"; // Дефолтный заголовок стартового окна
        private string _reasonsButtonName               = "Справочник";
        private string _humansButtonName                = "Memo-Random";
        private DateTime _currentDateTime               = DateTime.Now;
        
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IContainer _container; // Контейнер
        private readonly ICommonDataController _commonDataController;
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
        public void HumansButton_Open(object sender, RoutedEventArgs e)
        {
            _container.Resolve<HumansListView>().ShowDialog();
        }

        /// <summary>
        /// Открытие окна справочника причин смерти
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReasonsButton_Open(object sender, RoutedEventArgs e)
        {
            _container.Resolve<ReasonsView>().ShowDialog();
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
                Dispose();
                e.Cancel = false; // Окно закрываем
            }
            else
            {
                e.Cancel = true; // Окно не закрывается - отмена действия
            }
        }

        /// <summary>
        /// Загрузка стартового окна программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                var success = _commonDataController.SetFilesPaths(); // Проверка на выполняемость метода

                if (!success) // Если метод установки путей хранения информации неуспешен
                {
                    MessageBox.Show("Не удалось установить пути хранения информации!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.Close();
                    return;
                }

                ReadStartData();

                window.Closing += StartMemoRandomViewModel_Closing; // Подписываемся на событие закрытия окна
                cancelTokenSource = new CancellationTokenSource();
                token = cancelTokenSource.Token;
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
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        CurrentDateTime = DateTime.Now;
                    });
                }
            }, token);
        }

        /// <summary>
        /// Чтение справочника причин смерти и справочника возрастных категорий
        /// </summary>
        private async void ReadStartData()
        {
            bool success = true;

            await Task.Run(() =>
            {
                success = _commonDataController.ReadXmlData();
                ButtonsVisibility.Invoke();   // Чтение данных выполнено - кнопки делаем видимыми
            });

            if (!success)
            {
                MessageBox.Show("Чтение информации из XML-файлов не удалось!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource.Dispose();
        }






        #region CTOR

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="container"></param>
        /// <param name="msSqlController"></param>
        /// <param name="commonDataController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public StartMemoRandomViewModel(ILogger logger,
                                        IContainer container,
                                        ICommonDataController commonDataController)
        {
            _logger               = logger ?? throw new ArgumentNullException(nameof(logger));
            _container            = container ?? throw new ArgumentNullException(nameof(container));
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));
        }
        #endregion
    }
}
