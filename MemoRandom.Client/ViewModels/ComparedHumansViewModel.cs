using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления для окна добавления люедй для сравнения
    /// </summary>
    public class ComparedHumansViewModel :BindableBase
    {
        #region PRIVATE FIELDS
        private readonly IMsSqlController _msSqlController;

        private string _comparedHumansTitle = "Люди для сравнения";
        private ObservableCollection<ComparedHuman> _comparedHumansCollection;
        private Guid _comparedHumanId;
        private string _comparedHumanFullName;
        private DateTime _comparedHumanBirthDate;
        private int _selectedIndex;
        private ComparedHuman _selectedHuman;
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
        /// Связный список людей для сравнения
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
                RaisePropertyChanged(nameof(SelectedHuman));
            }
        }

        /// <summary>
        /// Идентификатор человека длч сравнения
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

        private bool newFlag = false;

        /// <summary>
        /// Команда добавления нового человека для сравнения
        /// </summary>
        private void NewComparedHuman()
        {
            newFlag = true;
            ComparedHumanId = Guid.NewGuid();
            ComparedHumanFullName = "Введите полное имя";
            ComparedHumanBirthDate = DateTime.Now.AddYears(-50);
        }

        /// <summary>
        /// Команда сохранения человека для сравнения во внешнем хранилище
        /// </summary>
        private async void SaveComparedHuman()
        {
            if (!newFlag) // Существующая запись человка дял сравнения
            {
                #region Обновление выбранного для сравнения человека
                SelectedHuman.ComparedHumanFullName = ComparedHumanFullName;
                SelectedHuman.ComparedHumanBirthDate = ComparedHumanBirthDate;
                #endregion

                await Task.Run(() =>
                {
                    var result = _msSqlController.UpdateComparedHuman(SelectedHuman);
                    if (!result)
                    {
                        MessageBox.Show("Не удалось обновить человека для сравнения", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                });
            }
            else // Создание новой записи человека для сравнения
            {
                ComparedHuman compHuman = new()
                {
                    ComparedHumanId = ComparedHumanId,
                    ComparedHumanFullName = ComparedHumanFullName,
                    ComparedHumanBirthDate = ComparedHumanBirthDate
                };

                await Task.Run(() =>
                {
                    var result = _msSqlController.UpdateComparedHuman(compHuman);
                    if (!result)
                    {
                        MessageBox.Show("Не удалось добавить человека для сравнения!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                });

                ComparedHumansCollection.Clear();
                ComparedHumans.ComparedHumansList.Add(compHuman);
                ComparedHumansCollection = ComparedHumans.GetComparedHumans();
                RaisePropertyChanged(nameof(ComparedHumansCollection));
                SelectedIndex = ComparedHumansCollection.IndexOf(compHuman);
            }

            newFlag = false;
        }

        /// <summary>
        /// Команда удаления выбранного человека для сравнения
        /// </summary>
        private void DeleteComparedHuman()
        {
            var result = _msSqlController.DeleteComparedHuman(SelectedHuman);
            if (!result)
            {
                MessageBox.Show("Не удалось удалить человека для сравнения!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ComparedHumans.ComparedHumansList.Remove(SelectedHuman);
            ComparedHumansCollection.Clear();
            ComparedHumansCollection = ComparedHumans.GetComparedHumans();
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
            ComparedHumans.ComparedHumansList = _msSqlController.GetComparedHumans();
            ComparedHumansCollection = ComparedHumans.GetComparedHumans();
            SelectedIndex = 0;

            RaisePropertyChanged(nameof(ComparedHumansCollection));
        }

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
        public ComparedHumansViewModel(IMsSqlController msSqlController)
        {
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));

            ComparedHumansCollection = new();
            //ComparedHumansList = _msSqlController.GetComparedHumans();

            InitCommands();
        }
        #endregion
    }
}
