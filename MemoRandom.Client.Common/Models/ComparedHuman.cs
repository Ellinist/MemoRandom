using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;

namespace MemoRandom.Client.Common.Models
{
    /// <summary>
    /// Класс людей для сравнения
    /// </summary>
    [Serializable]
    public class ComparedHuman : BasePerson
    {
        #region PRIVATE FIELDS
        private string _comparedHumanFullName;
        private bool _isComparedHumanConsidered;
        #endregion

        #region PROPS
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
        #endregion
    }
}
