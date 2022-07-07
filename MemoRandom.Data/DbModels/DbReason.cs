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
        public Guid DbReasonId { get; set; }
        
        /// <summary>
        /// Название причины смерти - лучше сокращенно
        /// </summary>
        public string DbReasonName { get; set; }
        
        /// <summary>
        /// Краткое описание причины смерти (комментарий)
        /// </summary>
        public string DbReasonComment { get; set; }
        
        /// <summary>
        /// Подробное описание причины смерти
        /// </summary>
        public string DbReasonDescription { get; set; }

        /// <summary>
        /// Идентификтор родительской причины (узла)
        /// </summary>
        [Required]
        public Guid DbReasonParentId { get; set; }





        #region CTOR
        public DbReason()
        {

        }
        #endregion
    }
}
