using System.Collections.ObjectModel;
using MemoRandom.Models.Models;

namespace MemoRandom.Data.Interfaces
{
    /// <summary>
    /// Интерфейс работы с БД
    /// </summary>
    public interface IReasonsController
    {
        /// <summary>
        /// Получение древовидной коллекции списка причин смерти
        /// </summary>
        /// <returns></returns>
        bool GetReasonsList();

        /// <summary>
        /// Добавление причины с общий список
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool AddReasonToList(Reason reason);

        /// <summary>
        /// Обновление измененных данных причины смерти
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool UpdateReasonInList(Reason reason);

        /// <summary>
        /// Удаление выбранной причины смерти и всех ее дочек
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool DeleteReasonInList(Reason reason);
    }
}
