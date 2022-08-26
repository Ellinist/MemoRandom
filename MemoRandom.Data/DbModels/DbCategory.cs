using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Data.DbModels
{
    public class DbCategory
    {
        [Key]
        [Required]
        public Guid DbCategoryId { get; set; }

        [Required]
        public string DbCategoryName { get; set; }

        [Required]
        public int DbPeriodFrom { get; set; }

        [Required]
        public int DbPeriodTo { get; set; }
    }
}
