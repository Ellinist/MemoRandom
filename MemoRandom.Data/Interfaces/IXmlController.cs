using MemoRandom.Data.DtoModels;
using System.Collections.Generic;

namespace MemoRandom.Data.Interfaces
{
    /// <summary>
    /// Интерфейс работы с XML-файлами
    /// </summary>
    public interface IXmlController
    {
        #region Блок работы со справочником причин смерти
        /// <summary>
        /// Чтение справочника причин смерти из файла XML
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<DtoReason> ReadReasonsFromFile(string filePath);

        void SaveReasonsToFile(List<DtoReason> reasons, string filePath);

        /// <summary>
        /// Добавление причины с общий список
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        void AddReasonToList(DtoReason reason, string filePath);

        /// <summary>
        /// Изменение причины в файле
        /// </summary>
        /// <param name="rsn"></param>
        /// <param name="filePath"></param>
        void ChangeReasonInFile(DtoReason reason, string filePath);

        /// <summary>
        /// Удаление причины из файла
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        void DeleteReasonInFile(string id, string filePath);
        #endregion

        #region Блок работы с категориями возрастов
        /// <summary>
        /// Чтение категорий возрастов из XML-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<DtoCategory> ReadCategoriesFromFile(string filePath);

        /// <summary>
        /// Обновление категории в файле
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        void UpdateCategoryInFile(DtoCategory category, string filePath);

        /// <summary>
        /// Удаление категории из файла
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        void DeleteCategoryInFile(string id, string filePath);
        #endregion

        #region Блок работы с людьми для сравнения
        /// <summary>
        /// Чтение людей для сравнения из XML-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<DtoComparedHuman> ReadComparedHumansFromFile(string filePath);

        /// <summary>
        /// Обновление/добавление человека для сравнения в файле
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        void UpdateComparedHumanInFile(DtoComparedHuman comparedHuman, string filePath);

        /// <summary>
        /// Удаление человека для сравнения в файле
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        void DeleteComparedHumanInFile(string id, string filePath);
        #endregion

        #region Блок работы с людьми
        /// <summary>
        /// Чтение основного списка людей из XML-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<DtoHuman> ReadHumansFromFile(string filePath);

        /// <summary>
        /// Обновление/добавление человека из основного списка людей
        /// </summary>
        /// <param name="human"></param>
        /// <param name="filePath"></param>
        void UpdateHumanInFile(DtoHuman human, string filePath);

        /// <summary>
        /// Удаление человека из основного списка людей
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        void DeleteHumanInFile(string id, string filePath);
        #endregion
    }
}