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

        /// <summary>
        /// Сохранение всех причин смерти в файле XML
        /// Пока что временно - а там посмотрим
        /// </summary>
        /// <param name="reasons"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool SaveReasonsToFile(List<DtoReason> reasons, string filePath);

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
        bool UpdateCategoryInFile(DtoCategory category, string filePath);

        /// <summary>
        /// Удаление категории из файла
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool DeleteCategoryInFile(string id, string filePath);

        /// <summary>
        /// Временно - Сохранение всех категорйи в файле
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool SaveCategoriesToFile(List<DtoCategory> categories, string filePath);
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
        bool UpdateComparedHumanInFile(DtoComparedHuman comparedHuman, string filePath);

        /// <summary>
        /// Удаление человека для сравнения в файле
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool DeleteComparedHumanInFile(string id, string filePath);

        /// <summary>
        /// Временно - Сохранение всех людей для сравнения в файле
        /// </summary>
        /// <param name="comparedHumans"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool SaveComparedHumansToFile(List<DtoComparedHuman> comparedHumans, string filePath);
        #endregion

        #region Блок работы с людьми
        /// <summary>
        /// Чтение основного списка людей из XML-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<DtoHuman> ReadHumansFromFile(string filePath);

        bool UpdateHumanInFile(DtoHuman human, string filePath);

        bool DeleteHumanInFile(string id, string filePath);

        /// <summary>
        /// Временно - Сохранение всего основного списка людей в файле
        /// </summary>
        /// <param name="humans"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool SaveHumansToFile(List<DtoHuman> humans, string filePath);
        #endregion
    }
}