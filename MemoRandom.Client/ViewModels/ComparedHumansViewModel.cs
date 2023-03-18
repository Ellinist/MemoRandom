using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления для окна добавления люедй для сравнения
    /// </summary>
    public class ComparedHumansViewModel :BindableBase
    {
        #region PRIVATE FIELDS
        private readonly ICommonDataController _commonDataController;

        private string _comparedHumansTitle = "Люди для сравнения";
        private ObservableCollection<ComparedHuman> _comparedHumansCollection;
        private Guid _comparedHumanId;
        private string _comparedHumanFullName;
        private DateTime _comparedHumanBirthDate;
        private int _selectedIndex;
        private ComparedHuman _selectedHuman;
        private bool _isConsidered;
        private bool _newFlag = false;
        private BitmapSource _comparedHumanImage;
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string ComparedHumansTitle
        {
            get => _comparedHumansTitle;
            set
            {
                _comparedHumansTitle = value;
                RaisePropertyChanged(nameof(ComparedHumansTitle));
            }
        }

        /// <summary>
        /// Коллекция людей для сравнения
        /// </summary>
        public ObservableCollection<ComparedHuman> ComparedHumansCollection
        {
            get => _comparedHumansCollection;
            set
            {
                _comparedHumansCollection = value;
                RaisePropertyChanged(nameof(ComparedHumansCollection));
            }
        }

        /// <summary>
        /// Индекс выбранного человека для сравнения
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(nameof(SelectedIndex));
            }
        }

        /// <summary>
        /// Выбранный человек
        /// </summary>
        public ComparedHuman SelectedHuman
        {
            get => _selectedHuman;
            set
            {
                if (value == null) return;
                _selectedHuman = value;

                ComparedHumanId = SelectedHuman.ComparedHumanId;
                ComparedHumanFullName = SelectedHuman.ComparedHumanFullName;
                ComparedHumanBirthDate = SelectedHuman.ComparedHumanBirthDate;
                IsConsidered = SelectedHuman.IsComparedHumanConsidered;
                RaisePropertyChanged(nameof(SelectedHuman));
            }
        }

        /// <summary>
        /// Идентификатор человека для сравнения
        /// </summary>
        public Guid ComparedHumanId
        {
            get => _comparedHumanId;
            set
            {
                _comparedHumanId = value;
                RaisePropertyChanged(nameof(ComparedHumanId));
            }
        }

        /// <summary>
        /// Полное имя человека для сравнения
        /// </summary>
        public string ComparedHumanFullName
        {
            get => _comparedHumanFullName;
            set
            {
                _comparedHumanFullName = value;
                RaisePropertyChanged(nameof(ComparedHumanFullName));
            }
        }

        /// <summary>
        /// Дата рождения человека для сравнения
        /// </summary>
        public DateTime ComparedHumanBirthDate
        {
            get => _comparedHumanBirthDate;
            set
            {
                _comparedHumanBirthDate = value;
                RaisePropertyChanged(nameof(ComparedHumanBirthDate));
            }
        }

        /// <summary>
        /// Рассматривается ли человек для сравнения в прогрессе анализа
        /// </summary>
        public bool IsConsidered
        {
            get => _isConsidered;
            set
            {
                _isConsidered = value;
                RaisePropertyChanged(nameof(IsConsidered));
            }
        }

        /// <summary>
        /// Изображение человека для сравнения
        /// </summary>
        public BitmapSource ComparedHumanImage
        {
            get => _comparedHumanImage;
            set
            {
                _comparedHumanImage = value;
                RaisePropertyChanged(nameof(ComparedHumanImage));
            }
        }
        #endregion

        #region COMMANDS
        /// <summary>
        /// Команда добавления человека для сравнения
        /// </summary>
        public DelegateCommand NewComparedHumanCommand { get; private set; }

        /// <summary>
        /// Команда сохранения человека для сравнения во внешнем хранилище
        /// </summary>
        public DelegateCommand SaveComparedHumanCommand { get; private set; }

        /// <summary>
        /// Команда удаления выбранного человека для сравнения
        /// </summary>
        public DelegateCommand DeleteComparedHumanCommand { get; private set; }
        #endregion

        

        /// <summary>
        /// Команда добавления нового человека для сравнения
        /// </summary>
        private void NewComparedHuman()
        {
            _newFlag = true;
            ComparedHumanId = Guid.NewGuid();
            ComparedHumanFullName = "Введите полное имя";
            ComparedHumanBirthDate = DateTime.Now.AddYears(-50);
        }

        /// <summary>
        /// Команда сохранения человека для сравнения во внешнем хранилище
        /// </summary>
        private async void SaveComparedHuman()
        {
            if (!_newFlag) // Существующая запись человека дял сравнения
            {
                #region Обновление выбранного для сравнения человека
                SelectedHuman.ComparedHumanFullName     = ComparedHumanFullName;
                SelectedHuman.ComparedHumanBirthDate    = ComparedHumanBirthDate;
                SelectedHuman.IsComparedHumanConsidered = IsConsidered;
                #endregion

                await Task.Run(() =>
                {
                    //var result = _commonDataController.UpdateComparedHumanInRepository(SelectedHuman);
                    var result = _commonDataController.UpdateComparedHuman(SelectedHuman);
                    if (!result)
                    {
                        MessageBox.Show("Не удалось обновить человека для сравнения", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                });
            }
            else // Создание новой записи человека для сравнения
            {
                ComparedHuman compHuman = new()
                {
                    ComparedHumanId           = ComparedHumanId,
                    ComparedHumanFullName     = ComparedHumanFullName,
                    ComparedHumanBirthDate    = ComparedHumanBirthDate,
                    IsComparedHumanConsidered = IsConsidered
                };

                await Task.Run(() =>
                {
                    var result = _commonDataController.UpdateComparedHuman(compHuman);
                    if (!result)
                    {
                        MessageBox.Show("Не удалось добавить человека для сравнения!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                });

                CommonDataController.ComparedHumansCollection.Add(compHuman);
                RaisePropertyChanged(nameof(ComparedHumansCollection));
                SelectedIndex = ComparedHumansCollection.IndexOf(compHuman);
            }

            _newFlag = false;
        }

        /// <summary>
        /// Команда удаления выбранного человека для сравнения
        /// </summary>
        private void DeleteComparedHuman()
        {
            var result = _commonDataController.DeleteComparedHuman(SelectedHuman.ComparedHumanId);
            if (!result)
            {
                MessageBox.Show("Не удалось удалить человека для сравнения!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CommonDataController.ComparedHumansCollection.Remove(SelectedHuman);
            RaisePropertyChanged(nameof(ComparedHumansCollection));
            SelectedIndex = 0;
        }

        /// <summary>
        /// Обработчик загрузки окна работы с людьми для сравнения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComparedHumansView_Loaded(object sender, RoutedEventArgs e)
        {
            ComparedHumansCollection = CommonDataController.ComparedHumansCollection;
            SelectedIndex = 0;

            RaisePropertyChanged(nameof(ComparedHumansCollection));
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            NewComparedHumanCommand = new DelegateCommand(NewComparedHuman);
            SaveComparedHumanCommand = new DelegateCommand(SaveComparedHuman);
            DeleteComparedHumanCommand = new DelegateCommand(DeleteComparedHuman);
        }






        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="commonDataController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ComparedHumansViewModel(ICommonDataController commonDataController)
        {
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

            InitCommands();
        }
        #endregion
    }
}
