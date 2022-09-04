using System.Collections.ObjectModel;

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
    }
}
