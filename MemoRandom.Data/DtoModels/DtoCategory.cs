using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Data.DtoModels
{
    /// <summary>
    /// Класс категории возрастного периода для хранения в XML-файле
    /// </summary>
    [Serializable]
    public class DtoCategory
    {
        /// <summary>
        /// Идентификатор категории
        /// </summary>
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Название категории
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// С какого возраста действует категория
        /// </summary>
        public string StartAge { get; set; }

        /// <summary>
        /// До какого возраста действует категория
        /// </summary>
        public string StopAge { get; set; }

        /// <summary>
        /// Цвет категории
        /// </summary>
        public string StringColor { get; set; }
    }
}
