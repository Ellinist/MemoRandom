using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Data.DtoModels
{
    [Serializable]
    public abstract class DtoPerson
    {
        public Guid PersonId { get; set; }
        public DateTime BirthDate { get; set; }
        public string ImageFile { get; set; }
    }
}
