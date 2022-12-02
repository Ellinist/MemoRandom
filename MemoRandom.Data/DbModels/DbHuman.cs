using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media.Imaging;

namespace MemoRandom.Data.DbModels
{
    /// <summary>
    /// Класс человека
    /// </summary>
    [Serializable]
    public class DbHuman
    {
        /// <summary>
        /// Идентификатор человека
        /// </summary>
        [Key]
        [Required]
        public Guid HumanId { get; set; }
        [MinLength(100)]
        public string LastName { get; set; }
        [MinLength(100)]
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthCountry { get; set; }
        public string BirthPlace { get; set; }
        public DateTime DeathDate { get; set; }
        public string DeathCountry { get; set; }
        public string DeathPlace { get; set; }
        public string ImageFile { get; set; }
        public Guid DeathReasonId { get; set; }
        public string HumanComments { get; set; }
        public double DaysLived { get; set; }
        public int FullYearsLived { get; set; }
    }
}
