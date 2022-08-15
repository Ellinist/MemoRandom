using System.Collections.Generic;
using System.Windows.Media.Imaging;
using MemoRandom.Models.Models;

namespace MemoRandom.Data.Interfaces
{
    public interface IHumansController
    {
        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        List<Human> GetHumans();

        /// <summary>
        /// Установка текущего человека при работе со списком
        /// Если null, то новая запись
        /// </summary>
        /// <param name="human"></param>
        void SetCurrentHuman(Human human);

        /// <summary>
        /// Получение текущего человека при работе со списком
        /// </summary>
        /// <returns></returns>
        Human GetCurrentHuman();

        /// <summary>
        /// Обновление (добавление или редактирование) списка людей
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        bool UpdateHumans(BitmapImage humanImage);

        /// <summary>
        /// Удаление человека из списка
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        bool DeleteHuman();

        BitmapImage GetHumanImage();
    }
}
