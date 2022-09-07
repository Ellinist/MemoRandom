using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Models.Models
{
    /// <summary>
    /// Класс людей для сравнения
    /// </summary>
    [Serializable]
    public class ComparedHuman
    {
        /// <summary>
        /// Идентификатор человека для сравнения
        /// </summary>
        [Key]
        [Required]
        public Guid ComparedHumanId { get; set; }

        /// <summary>
        /// Полное название человека
        /// </summary>
        [Required]
        public string ComparedHumanFullName { get; set; }

        /// <summary>
        /// Дата рождения человека
        /// </summary>
        [Required]
        public DateTime ComparedHumanBirthDate { get; set; }
    }
}
