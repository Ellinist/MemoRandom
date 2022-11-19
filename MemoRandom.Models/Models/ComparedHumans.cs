using System.Collections.ObjectModel;

namespace MemoRandom.Models.Models
{
    public static class ComparedHumans
    {
        /// <summary>
        /// Список людей для сравнения (статический)
        /// </summary>
        public static ObservableCollection<ComparedHuman> ComparedHumansList { get; set; }

        /// <summary>
        /// Получение клонированного списка людей для сравнения
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<ComparedHuman> GetComparedHumans()
        {
            var comparedHumans = new ObservableCollection<ComparedHuman>();
            foreach (var item in ComparedHumansList)
            {
                comparedHumans.Add(item);
            }

            return comparedHumans;
        }
    }
}
