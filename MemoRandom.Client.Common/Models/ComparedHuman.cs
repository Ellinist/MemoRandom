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
        private bool _isComparedHumanConsidered;
        private string _imageFile;
        #endregion

        #region PROPS
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
                _comparedHumanId = value;
                RaisePropertyChanged(nameof(ComparedHumanId));
            }
        }

        /// <summary>
        /// Полное название человека
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
        /// Свойство - рассматривается ли человек для сравнения в прогрессе сравнения
        /// </summary>
        public bool IsComparedHumanConsidered
        {
            get => _isComparedHumanConsidered;
            set
            {
                _isComparedHumanConsidered = value;
                RaisePropertyChanged(nameof(IsComparedHumanConsidered));
            }
        }

        /// <summary>
        /// Путь к файлу изображения - все файлы хранятся в папке
        /// </summary>
        public string ImageFile
        {
            get => _imageFile;
            set
            {
                _imageFile = value;
                RaisePropertyChanged(nameof(ImageFile));
            }
        }
        #endregion
    }
}
