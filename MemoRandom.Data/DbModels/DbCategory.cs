using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

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
        public int StartAge { get; set; }

        /// <summary>
        /// До какого возраста действует категория
        /// </summary>
        [Required]
        public int StopAge { get; set; }

        /// <summary>
        /// Цвет категории
        /// </summary>
        public string StringColor { get; set; }
    }
}
