
namespace MemoRandom.Data.DtoModels
{
    /// <summary>
    /// Класс человека для сравнения при хранении в XML-файле
    /// </summary>
    public class DtoComparedHuman : DtoBasePerson
    {
        /// <summary>
        /// Полное название человека для сравнения
        /// </summary>
        public string ComparedHumanFullName { get; set; }

        /// <summary>
        /// Учитывать человека для сравнения в прогрессе сравнения
        /// </summary>
        public bool IsComparedHumanConsidered { get; set; }
    }
}
