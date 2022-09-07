using System;
using System.ComponentModel.DataAnnotations;

namespace MemoRandom.Data.DbModels
{
    [Serializable]
    /// <summary>
    /// Класс человека для сравнения для внешнего хранилища
    /// </summary>
    public class DbComparedHuman
    {
        [Key]
        [Required]
        /// <summary>
        /// Идентификатор человека для сравнения
        /// </summary>
        public Guid DbComparedHumanId { get; set; }
        
        /// <summary>
        /// Полное название человека для сравнения
        /// </summary>
        [Required]
        public string DbComparedHumanFullName { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        [Required]
        public DateTime DbComparedHumanBirthDate { get; set; }
    }
}
