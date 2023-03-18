using System;

namespace MemoRandom.Data.DtoModels
{
    /// <summary>
    /// Класс категории возрастного периода для хранения в XML-файле
    /// </summary>
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
        public string CategoryColor { get; set; }
    }
}
