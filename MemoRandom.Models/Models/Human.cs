﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace MemoRandom.Models.Models
{
    /// <summary>
    /// Класс человека
    /// </summary>
    public class Human : BindableBase
    {
        #region Private fields
        private Guid _humanId;
        private string _firstName;
        private string _lastName;
        private string _patronymic;
        private DateTime _birthDate;
        private string _birthCountry;
        private string _birthPlace;
        private DateTime _deathDate;
        private string _deathCountry;
        private string _deathPlace;
        private BitmapImage _humanImage;
        private Guid _deathReasonId;
        #endregion

        /// <summary>
        /// Идентификатор человека
        /// </summary>
        [Key]
        [Required]
        public Guid HumanId
        {
            get => _humanId;
            set
            {
                _humanId = value;
                RaisePropertyChanged(nameof(HumanId));
            }
        }

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
        /// Дата рождения человека (подумать над временем суток)
        /// </summary>
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                RaisePropertyChanged(nameof(BirthDate));
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
                RaisePropertyChanged(nameof(_birthPlace));
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
        /// Свойство - путь к изображению человека
        /// </summary>
        public BitmapImage HumanImage
        {
            get => _humanImage;
            set
            {
                _humanImage = value;
                RaisePropertyChanged(nameof(HumanImage));
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
    }
}
