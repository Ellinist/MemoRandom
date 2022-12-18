using MemoRandom.Client.Common.Enums;
using MemoRandom.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace MemoRandom.Client.Common.Interfaces
{
    public interface ICommonDataController
    {
        /// <summary>
        /// Установка путей доступа к файлам хранения информации
        /// </summary>
        bool SetFilesPaths();

        /// <summary>
        /// Чтение информации из XML-файлов
        /// </summary>
        bool ReadXmlData();

        #region Работа с причинами смерти
        /// <summary>
        /// Добавление причины в список
        /// </summary>
        /// <param name="reason"></param>
        bool AddReasonToFile(Reason reason);

        /// <summary>
        /// Изменение причины в списке
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool ChangeReasonInFile(Reason reason);
        
        /// <summary>
        /// Удаление причины и всех ее дочерних узлов (если таковые есть)
        /// </summary>
        /// <param name="guidList"></param>
        /// <returns></returns>
        bool DeleteReasonAndDaughtersInFile(List<Guid> guidList);
        #endregion

        #region Работа с возрастными категориями
        /// <summary>
        /// Обновление/добавление категории в файле
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        bool UpdateCategoriesInFile(Category category);

        /// <summary>
        /// Удаление категории в файле
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteCategoryInFile(Guid id);
        #endregion

        #region Работа с людьми для сравнения
        /// <summary>
        /// Обновление/добавление человека для сравнения
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <returns></returns>
        bool UpdateComparedHuman(ComparedHuman comparedHuman);

        /// <summary>
        /// Удаление человека для сравнения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteComparedHuman(Guid id);
        #endregion

        #region Работа с основным списком людей

        /// <summary>
        /// Обновление/добавление человека из основного списка людей
        /// </summary>
        /// <param name="human"></param>
        /// <param name="humanImage"></param>
        /// <returns></returns>
        bool UpdateHuman(Human human, BitmapImage humanImage);
        bool DeleteHuman(Human human, string imageFile);
        #endregion


        //void SaveXmlData();
        









        ///// <summary>
        ///// Чтение общей информации из внешнего хранилища
        ///// </summary>
        ///// <returns></returns>
        //bool ReadDataFromRepository();

        ///// <summary>
        ///// Обновление (или добавление) категории во внешнее хранилище
        ///// </summary>
        ///// <param name="category"></param>
        ///// <returns></returns>
        //bool UpdateCategoriesInRepository(Category category);

        ///// <summary>
        ///// Удаление выбранной категории во внешнем хранилище
        ///// </summary>
        ///// <param name="category"></param>
        ///// <returns></returns>
        //bool DeleteCategoryInRepository(Category category);

        ///// <summary>
        ///// Обновление (добавление) человека для сравнения во внешнем хранилище
        ///// </summary>
        ///// <param name="comparedHuman"></param>
        ///// <returns></returns>
        //bool UpdateComparedHumanInRepository(ComparedHuman comparedHuman);

        ///// <summary>
        ///// Удаление человека для сравнения во внешнем хранилище
        ///// </summary>
        ///// <returns></returns>
        //bool DeleteComparedHumanInRepository(ComparedHuman comparedHuman);

        ///// <summary>
        ///// Обновление (добавление) человека во внешнем хранилище
        ///// </summary>
        ///// <param name="human"></param>
        ///// <returns></returns>
        //bool UpdateHumanInRepository(Human human, BitmapImage humanImage);

        ///// <summary>
        ///// Удаление человека из внешнего хранилища
        ///// </summary>
        ///// <param name="human"></param>
        ///// <param name="imageFile"></param>
        ///// <returns></returns>
        //bool DeleteHumanInRepository(Human human, string imageFile);

        /// <summary>
        /// Обновление иерархической коллекции причин смерти
        /// </summary>
        void UpdateHierarchicalReasonsData();

        /// <summary>
        /// Получение изображения человека
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        BitmapImage GetHumanImage(Human currentHuman);

        /// <summary>
        /// Получение слов с правильными окончаниями
        /// </summary>
        /// <param name="i"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetFinalText(int i, ScopeTypes type);

        /// <summary>
        /// Получение количества лет и дней за период
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        Tuple<int, int> GetYearsAndDaysConsideredLeaps(DateTime start, DateTime stop);
    }
}
