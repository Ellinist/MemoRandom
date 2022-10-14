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
                RaisePropertyChanged(nameof(ComparedHumansList));
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

        }

        /// <summary>
        /// Команда сохранения человека для сравнения во внешнем хранилище
        /// </summary>
        private void SaveComparedHuman()
        {

        }

        /// <summary>
        /// Команда удаления выбранного человека для сравнения
        /// </summary>
        private void DeleteComparedHuman()
        {

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

            InitCommands();
        }
        #endregion
    }
}
