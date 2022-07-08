using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemoRandom.Data.DbModels
{
    public class DbHuman
    {
        [Key]
        [Required]
        public Guid HumanId { get; set; }
        [MinLength(100)]
        public string LastName { get; set; }
        [MinLength(100)]
        public string FirstName { get; set; }
        //public string Patronymic { get; set; }
        //public DateTime BirthDate { get; set; }
        //public string BirthCountry { get; set; }
        //public string BirthPlace { get; set; }
        //public DateTime DeathDate { get; set; }
        //public string DeathCountry { get; set; }
        //public string DeathPlace { get; set; }
        //public string ImageFile { get; set; }
        //[Key]
        //public Guid DeathReasonId { get; set; }
    }
}
