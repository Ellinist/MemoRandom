using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Data.DtoModels
{
    /// <summary>
    /// Класс человека для сравнения при хранении в XML-файле
    /// </summary>
    [Serializable]
    public class DtoComparedHuman
    {
        /// <summary>
        /// Идентификатор человека для сравнения
        /// </summary>
        public string ComparedHumanId { get; set; }

        /// <summary>
        /// Полное название человека для сравнения
        /// </summary>
        public string ComparedHumanFullName { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime ComparedHumanBirthDate { get; set; }

        /// <summary>
        /// Учитывать человека для сравнения в прогрессе сравнения
        /// </summary>
        public bool IsComparedHumanConsidered { get; set; }
    }
}
