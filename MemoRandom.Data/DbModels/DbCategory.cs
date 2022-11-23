using System;
using System.ComponentModel.DataAnnotations;

namespace MemoRandom.Data.DbModels
{
    /// <summary>
    /// Класс категории человеческого возраста
    /// </summary>
    [Serializable]
    public class DbCategory
    {
        /// <summary>
        /// Идентификатор категории
        /// </summary>
        [Key]
        [Required]
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Название категории
        /// </summary>
        [Required]
        public string CategoryName { get; set; }

        /// <summary>
        /// С какого возраста действует категория
        /// </summary>
        [Required]
        public int PeriodFrom { get; set; }

        /// <summary>
        /// До какого возраста действует категория
        /// </summary>
        [Required]
        public int PeriodTo { get; set; }

        /// <summary>
        /// Яркость цвета
        /// </summary>
        [Required]
        public byte ColorA { get; set; }

        /// <summary>
        /// Красный цвет
        /// </summary>
        [Required]
        public byte ColorR { get; set; }

        /// <summary>
        /// Зеленый цвет
        /// </summary>
        [Required]
        public byte ColorG { get; set; }

        /// <summary>
        /// Синий цвет
        /// </summary>
        [Required]
        public byte ColorB { get; set; }
    }
}
