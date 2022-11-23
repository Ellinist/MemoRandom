using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;

namespace MemoRandom.Client.Common.Models
{
    /// <summary>
    /// Класс людей для сравнения
    /// </summary>
    [Serializable]
    public class ComparedHuman : BindableBase
    {
        #region PRIVATE FIELDS
        private Guid _comparedHumanId;
        private string _comparedHumanFullName;
        private DateTime _comparedHumanBirthDate;
        #endregion

        /// <summary>
        /// Идентификатор человека для сравнения
        /// </summary>
        [Key]
        [Required]
        public Guid ComparedHumanId
        {
            get => _comparedHumanId;
            set
            {
                _comparedHumanId= value;
                RaisePropertyChanged(nameof(ComparedHumanId));
            }
        }

        /// <summary>
        /// Полное название человека
        /// </summary>
        [Required]
        public string ComparedHumanFullName
        {
            get => _comparedHumanFullName;
            set
            {
                _comparedHumanFullName= value;
                RaisePropertyChanged(nameof(ComparedHumanFullName));
            }
        }

        /// <summary>
        /// Дата рождения человека
        /// </summary>
        [Required]
        public DateTime ComparedHumanBirthDate
        {
            get => _comparedHumanBirthDate;
            set
            {
                _comparedHumanBirthDate= value;
                RaisePropertyChanged(nameof(ComparedHumanBirthDate));
            }
        }
    }
}
