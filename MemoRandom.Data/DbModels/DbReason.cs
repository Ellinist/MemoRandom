using System;
using System.ComponentModel.DataAnnotations;

namespace MemoRandom.Data.DbModels
{
    public class DbReason
    {
        /// <summary>
        /// Идентификатор причины
        /// </summary>
        [Key]
        [Required]
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
        /// Идентификтор родительской причины (узла)
        /// </summary>
        [Required]
        public Guid ReasonParentId { get; set; }





        #region CTOR
        public DbReason()
        {

        }
        #endregion
    }
}
