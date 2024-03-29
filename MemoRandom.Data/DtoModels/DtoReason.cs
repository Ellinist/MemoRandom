﻿using System;

namespace MemoRandom.Data.DtoModels
{
    /// <summary>
    /// Класс причины смерти для хранения в XML-файле
    /// </summary>
    [Serializable]
    public class DtoReason
    {
        /// <summary>
        /// Идентификатор причины
        /// </summary>
        public Guid ReasonId { get; set; }

        /// <summary>
        /// Название причины смерти - лучше сокращенно
        /// </summary>
        public string ReasonName { get; set; }

        /// <summary>
        /// Краткое описание причины смерти (комментарий)
        /// </summary>
        public string ReasonComment { get; set; }

        /// <summary>
        /// Подробное описание причины смерти
        /// </summary>
        public string ReasonDescription { get; set; }

        /// <summary>
        /// Идентификатор родительской причины (узла)
        /// </summary>
        public Guid ReasonParentId { get; set; }
    }
}
