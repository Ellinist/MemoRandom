using System;
using System.ComponentModel.DataAnnotations;

namespace MemoRandom.Data.DbModels
{
    /// <summary>
    /// Класс человека для сравнения для внешнего хранилища
    /// </summary>
    [Serializable]
    public class DbComparedHuman
    {
        /// <summary>
        /// Идентификатор человека для сравнения
        /// </summary>
        [Key]
        [Required]
        public Guid ComparedHumanId { get; set; }
        
        /// <summary>
        /// Полное название человека для сравнения
        /// </summary>
        [Required]
        public string ComparedHumanFullName { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        [Required]
        public DateTime ComparedHumanBirthDate { get; set; }
    }
}
