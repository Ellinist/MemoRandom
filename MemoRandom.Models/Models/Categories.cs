using System.Collections.ObjectModel;

namespace MemoRandom.Models.Models
{
    public static class Categories
    {
        public static ObservableCollection<Category> AgeCategories { get; set; }

        /// <summary>
        /// Получение клонированной коллекции категорий
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Category> GetCategories()
        {
            ObservableCollection<Category> categories = new ObservableCollection<Category>();
            foreach (var item in AgeCategories)
            {
                categories.Add(item);
            }

            return categories;
        }
    }
}
