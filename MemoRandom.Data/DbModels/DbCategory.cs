using System;
using System.ComponentModel.DataAnnotations;

namespace MemoRandom.Data.DbModels
{
    /// <summary>
    /// Класс категории человеческого возраста
    /// </summary>
    public class DbCategory
    {
        /// <summary>
        /// Идентификатор категории
        /// </summary>
        [Key]
        [Required]
        public Guid DbCategoryId { get; set; }

        /// <summary>
        /// Название категории
        /// </summary>
        [Required]
        public string DbCategoryName { get; set; }

        /// <summary>
        /// С какого возраста действует категория
        /// </summary>
        [Required]
        public int DbPeriodFrom { get; set; }

        /// <summary>
        /// До какого возраста действует категория
        /// </summary>
        [Required]
        public int DbPeriodTo { get; set; }

        /// <summary>
        /// Яркость цвета
        /// </summary>
        [Required]
        public byte DbColorA { get; set; }

        /// <summary>
        /// Красный цвет
        /// </summary>
        [Required]
        public byte DbColorR { get; set; }

        /// <summary>
        /// Зеленый цвет
        /// </summary>
        [Required]
        public byte DbColorG { get; set; }

        /// <summary>
        /// Синий цвет
        /// </summary>
        [Required]
        public byte DbColorB { get; set; }
    }
}
