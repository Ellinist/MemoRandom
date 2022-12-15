using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Data.DtoModels
{
    /// <summary>
    /// Класс человека для хранения в XML-файле
    /// </summary>
    [Serializable]
    public class DtoHuman
    {
        public string HumanId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Patronymic { get; set; }

        public DateTime BirthDate { get; set; }

        public string BirthCountry { get; set; }

        public string BirthPlace { get; set; }

        public DateTime DeathDate { get; set; }

        public string DeathCountry { get; set; }

        public string DeathPlace { get;set; }

        public string ImageFile { get; set; }

        public string DeathReasonId { get; set; }

        public string HumanComments { get; set; }

        public double DaysLived { get; set; }

        public int FullYearsLived { get; set; }
    }
}
