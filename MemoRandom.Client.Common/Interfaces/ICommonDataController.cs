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

        /// <summary>
        /// Обновление (добавление) человека для сравнения во внешнем хранилище
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <returns></returns>
        bool UpdateComparedHumanInRepository(ComparedHuman comparedHuman);

        /// <summary>
        /// Удаление человека для сравнения во внешнем хранилище
        /// </summary>
        /// <returns></returns>
        bool DeleteComparedHumanInRepository(ComparedHuman comparedHuman);

        /// <summary>
        /// Обновление (добавление) человека во внешнем хранилище
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        bool UpdateHumanInRepository(Human human, BitmapImage humanImage);

        /// <summary>
        /// Удаление человека из внешнего хранилища
        /// </summary>
        /// <param name="human"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        bool DeleteHumanInRepository(Human human, string imageFile);

        void UpdateHierarchicalReasonsData();

        /// <summary>
        /// Получение изображения человека
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        BitmapImage GetHumanImage(Human currentHuman);
    }
}
