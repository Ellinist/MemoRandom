using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media.Imaging;

namespace MemoRandom.Data.DbModels
{
    public class DbHuman
    {
        [Key]
        [Required]
        public Guid DbHumanId { get; set; }
        [MinLength(100)]
        public string DbLastName { get; set; }
        [MinLength(100)]
        public string DbFirstName { get; set; }
        public string DbPatronymic { get; set; }
        public DateTime DbBirthDate { get; set; }
        public string DbBirthCountry { get; set; }
        public string DbBirthPlace { get; set; }
        public DateTime DbDeathDate { get; set; }
        public string DbDeathCountry { get; set; }
        public string DbDeathPlace { get; set; }
        [NotMapped]
        public BitmapImage DbImageFile { get; set; }
        public Guid DbDeathReasonId { get; set; }
    }
}
