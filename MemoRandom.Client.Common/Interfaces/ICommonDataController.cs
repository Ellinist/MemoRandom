using MemoRandom.Client.Common.Models;
using System.Windows.Media.Imaging;

namespace MemoRandom.Client.Common.Interfaces
{
    public interface ICommonDataController
    {
        /// <summary>
        /// Чтение общей информации из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        bool ReadDataFromRepository();

        /// <summary>
        /// Обновление (или добавление) категории во внешнее хранилище
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        bool UpdateCategoriesInRepository(Category category);

        /// <summary>
        /// Удаление выбранной категории во внешнем хранилище
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        bool DeleteCategoryInRepository(Category category);

        bool UpdateComparedHumanInRepository(ComparedHuman comparedHuman);

        bool DeleteComparedHumanInRepository(ComparedHuman comparedHuman);

        void UpdateHierarchicalReasonsData();

        bool UpdateHumanData(Human human, BitmapImage humanImage);

        /// <summary>
        /// Получение изображения человека
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        BitmapImage GetHumanImage(Human currentHuman);
    }
}
