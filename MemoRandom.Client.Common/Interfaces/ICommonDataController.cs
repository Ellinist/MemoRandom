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
