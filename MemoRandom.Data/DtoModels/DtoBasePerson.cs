using System;

namespace MemoRandom.Data.DtoModels
{
    [Serializable]
    public abstract class DtoBasePerson
    {
        public Guid PersonId { get; set; }
        public DateTime BirthDate { get; set; }
        public string ImageFile { get; set; }
    }
}
