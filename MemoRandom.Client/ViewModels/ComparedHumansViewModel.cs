using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
        private BindingList<ComparedHuman> _comparedHumansList;
        private Guid _comparedHumanId;
        private string _humanFullName;
        private DateTime _humanBirthDate;
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
        public BindingList<ComparedHuman> ComparedHumansList
        {
            get => _comparedHumansList;
            set
            {
                _comparedHumansList = value;
                //RaisePropertyChanged(nameof(ComparedHumansList));
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
                _selectedHuman = value;
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
        public string HumanFullName
        {
            get => _humanFullName;
            set
            {
                _humanFullName = value;
                RaisePropertyChanged(nameof(HumanFullName));
            }
        }

        public DateTime HumanBirthDate
        {
            get => _humanBirthDate;
            set
            {
                _humanBirthDate = value;
                RaisePropertyChanged(nameof(HumanBirthDate));
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
            HumanFullName = "Введите полное имя";
        }

        /// <summary>
        /// Команда сохранения человека для сравнения во внешнем хранилище
        /// </summary>
        private async void SaveComparedHuman()
        {
            if (!newFlag) // Существующая запись человка дял сравнения
            {
                #region Обновление выбранного для сравнения человека

                #endregion
            }
            else // Создание новой записи человека для сравнения
            {
                ComparedHuman compHuman = new()
                {
                    ComparedHumanId = ComparedHumanId,
                    ComparedHumanFullName = HumanFullName,
                    ComparedHumanBirthDate = HumanBirthDate
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

                ComparedHumansList.Clear();
                //ComparedHumansList = 
            }
        }

        /// <summary>
        /// Команда удаления выбранного человека для сравнения
        /// </summary>
        private void DeleteComparedHuman()
        {

        }

        /// <summary>
        /// Обработчик загрузки окна работы с людьми для сравнения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComparedHumansView_Loaded(object sender, RoutedEventArgs e)
        {
            ComparedHumans.ComparedHumansList = _msSqlController.GetComparedHumans();
            ComparedHumansList = ComparedHumans.GetComparedHumans();

            //RaisePropertyChanged(nameof(ComparedHumansList));
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

            ComparedHumansList = new();
            //ComparedHumansList = _msSqlController.GetComparedHumans();

            InitCommands();
        }
        #endregion
    }
}
