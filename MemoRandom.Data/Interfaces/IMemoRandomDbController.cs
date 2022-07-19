using System.Collections.Generic;
using System.Collections.ObjectModel;
using MemoRandom.Models.Models;

namespace MemoRandom.Data.Interfaces
{
    /// <summary>
    /// Интерфейс работы с БД
    /// </summary>
    public interface IMemoRandomDbController
    {
        /// <summary>
        /// Получение древовидной коллекции списка причин смерти
        /// </summary>
        /// <returns></returns>
        ObservableCollection<Reason> GetReasonsList();

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

        /// <summary>
        /// Получение списка людей из БД
        /// </summary>
        /// <returns></returns>
        void GetHumansList();

        /// <summary>
        /// Добавление человека в список людей в БД
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        bool AddHumanToList(Human human);

        bool UpdateHumanInList(Human human);

        bool DeleteHumanFromList(Human human);

        //void GetPicture(Human human);
    }
}
