using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Models.Models
{
    /// <summary>
    /// Класс жизненного периода
    /// </summary>
    public class LifePeriodType
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
    }
}
