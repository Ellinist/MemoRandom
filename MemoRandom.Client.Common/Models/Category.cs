using System;
using System.Windows.Media;

namespace MemoRandom.Client.Common.Models
{
    /// <summary>
    /// Класс жизненного периода
    /// </summary>
    public class Category
    {
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Название периода жизни
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Стартовый возраст (в годах), с которого начинается период
        /// </summary>
        public int StartAge { get; set; }

        /// <summary>
        /// Стоповый возраст (в годах), которым заканчивается период
        /// </summary>
        public int StopAge { get; set; }

        public Color CategoryColor { get; set; }

        public string StringColor { get; set; }
    }
}
