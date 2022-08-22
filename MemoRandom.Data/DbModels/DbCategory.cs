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
        public Guid CategoryId { get; set; }
        public string DbCategoryName { get; set; }
        public int DbPeriodFrom { get; set; }
        public int DbPeriodTo { get; set; }
    }
}
