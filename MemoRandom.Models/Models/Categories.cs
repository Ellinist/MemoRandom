using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MemoRandom.Models.Models
{
    /// <summary>
    /// Статический класс коллекции категорий
    /// </summary>
    public static class Categories
    {
        /// <summary>
        /// Коллекция категорий (статическая)
        /// </summary>
        public static ObservableCollection<Category> AgeCategories { get; set; }

        /// <summary>
        /// Получение клонированной коллекции категорий
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Category> GetCategories()
        {
            var categories = new ObservableCollection<Category>();
            foreach (var item in AgeCategories)
            {
                categories.Add(item);
            }

            return categories;
        }

        /// <summary>
        /// Сортировка по возрастанию стартового возраста
        /// </summary>
        public static void RearrangeCollection()
        {
            List<Category> rearrangeCollection = new();
            rearrangeCollection = AgeCategories.OrderBy(x => x.StartAge).ToList();
            AgeCategories.Clear();
            foreach (var item in rearrangeCollection)
            {
                AgeCategories.Add(item);
            }
        }
    }
}
