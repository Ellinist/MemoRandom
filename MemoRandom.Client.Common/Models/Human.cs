using System;

namespace MemoRandom.Client.Common.Models
{
    /// <summary>
    /// Класс человека
    /// </summary>
    public class Human : BasePerson
    {
        #region PRIVATE FIELDS
        private string _firstName;
        private string _lastName;
        private string _patronymic;
        private string _birthCountry;
        private string _birthPlace;
        private DateTime _deathDate;
        private string _deathCountry;
        private string _deathPlace;
        private Guid _deathReasonId;
        private string _humanComments;
        private double _daysLived;
        private int _fullYearsLived;
        #endregion

        #region PROPS
        ///// <summary>
        ///// Идентификатор человека
        ///// </summary>
        //[Key]
        //[Required]
        //public Guid HumanId
        //{
        //    get => _humanId;
        //    set
        //    {
        //        _humanId = value;
        //        RaisePropertyChanged(nameof(HumanId));
        //    }
        //}

        /// <summary>
        /// Имя человека
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged(nameof(FirstName));
            }
        }

        /// <summary>
        /// Фамилия человека
        /// </summary>
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                RaisePropertyChanged(nameof(LastName));
            }
        }

        /// <summary>
        /// Отчество человека (в случае наличия)
        /// </summary>
        public string Patronymic
        {
            get => _patronymic;
            set
            {
                _patronymic = value;
                RaisePropertyChanged(nameof(Patronymic));
            }
        }

        /// <summary>
        /// Страна рождения человека
        /// </summary>
        public string BirthCountry
        {
            get => _birthCountry;
            set
            {
                _birthCountry = value;
                RaisePropertyChanged(nameof(BirthCountry));
            }
        }

        /// <summary>
        /// Место рождения человека (город, село, область и т.п.)
        /// </summary>
        public string BirthPlace
        {
            get => _birthPlace;
            set
            {
                _birthPlace = value;
                RaisePropertyChanged(nameof(BirthPlace));
            }
        }

        /// <summary>
        /// Дата смерти человека
        /// </summary>
        public DateTime DeathDate
        {
            get => _deathDate;
            set
            {
                _deathDate = value;
                RaisePropertyChanged(nameof(DeathDate));
            }
        }

        /// <summary>
        /// Страна смерти человека
        /// </summary>
        public string DeathCountry
        {
            get => _deathCountry;
            set
            {
                _deathCountry = value;
                RaisePropertyChanged(nameof(DeathCountry));
            }
        }

        /// <summary>
        /// Место смерти человека
        /// </summary>
        public string DeathPlace
        {
            get => _deathPlace;
            set
            {
                _deathPlace = value;
                RaisePropertyChanged(nameof(DeathPlace));
            }
        }

        /// <summary>
        /// Идентификатор причины смерти (берется из справочника)
        /// </summary>
        public Guid DeathReasonId
        {
            get => _deathReasonId;
            set
            {
                _deathReasonId = value;
                RaisePropertyChanged(nameof(DeathReasonId));
            }
        }

        /// <summary>
        /// Расширенный комментарий
        /// </summary>
        public string HumanComments
        {
            get => _humanComments;
            set
            {
                _humanComments = value;
                RaisePropertyChanged(nameof(HumanComments));
            }
        }

        /// <summary>
        /// Число прожитых дней
        /// Сохраняется в БД для ускорения чтения информации
        /// </summary>
        public double DaysLived
        {
            get => _daysLived;
            set
            {
                _daysLived = value;
                RaisePropertyChanged(nameof(DaysLived));
            }
        }

        /// <summary>
        /// Полное число прожитых лет - для упорядочения по возрастанию прожитых лет
        /// </summary>
        public int FullYearsLived
        {
            get => _fullYearsLived;
            set
            {
                _fullYearsLived = value;
                RaisePropertyChanged(nameof(FullYearsLived));
            }
        }
        #endregion
    }
}
