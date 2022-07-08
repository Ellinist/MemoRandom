using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MemoRandom.Client.ViewModels
{
    public class HumanDetailesViewModel : BindableBase
    {
        #region Private Fields
        private string _lastName = "Введите фамилию";
        private string _firstName = "Введите имя";
        private string _patronymic = "Введите отчество";
        private DateTime _birthDate = DateTime.Now;
        private string _birthCountry = "Введите страну рождения";
        private string _birthPlace = "Введите место рождения";
        private DateTime _deathDate = DateTime.Now;
        private string _deathCountry = "Введите страну смерти";
        private string _deathPlace = "Введите место смерти";
        private Guid _deathReasonId;
        #endregion

        #region PROPS
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                RaisePropertyChanged();
            }
        }
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged();
            }
        }
        public string Patronymic
        {
            get => _patronymic;
            set
            {
                _patronymic = value;
                RaisePropertyChanged();
            }
        }
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                RaisePropertyChanged();
            }
        }
        public string BirthCountry
        {
            get => _birthCountry;
            set
            {
                _birthCountry = value;
                RaisePropertyChanged();
            }
        }
        public string BirthPlace
        {
            get => _birthPlace;
            set
            {
                _birthPlace = value;
                RaisePropertyChanged();
            }
        }
        public DateTime DeathDate
        {
            get => _deathDate;
            set
            {
                _deathDate = value;
                RaisePropertyChanged();
            }
        }
        public string DeathCountry
        {
            get => _deathCountry;
            set
            {
                _deathCountry = value;
                RaisePropertyChanged();
            }
        }
        public string DeathPlace
        {
            get => _deathPlace;
            set
            {
                _deathPlace = value;
                RaisePropertyChanged();
            }
        }

        public Guid DeathReasonId
        {
            get => _deathReasonId;
            set
            {
                _deathReasonId = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region COMMANDS
        public DelegateCommand SaveHumanCommand { get; private set; }
        #endregion

        #region COMMANDS IMPLEMENTATION
        private void SaveHuman()
        {
            // Пока для нового хьюмана
            Human human = new()
            {
                LastName = LastName,
                FirstName = FirstName,
                Patronymic = Patronymic,
                BirthDate = BirthDate,
                BirthCountry = BirthCountry,
                BirthPlace = BirthPlace,
                DeathDate = DeathDate,
                DeathCountry = DeathCountry,
                DeathPlace = DeathPlace,
                DeathReasonId = DeathReasonId
            };
        }
        #endregion

        private void InitializeCommands()
        {
            SaveHumanCommand = new DelegateCommand(SaveHuman);
        }

        #region CTOR
        public HumanDetailesViewModel()
        {


            InitializeCommands();
        }
        #endregion
    }
}
