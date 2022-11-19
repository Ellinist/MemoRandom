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
