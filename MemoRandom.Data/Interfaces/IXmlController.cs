using MemoRandom.Data.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Data.Interfaces
{
    /// <summary>
    /// Интерфейс работы с XML-файлами
    /// </summary>
    public interface IXmlController
    {
        #region Блок работы со справочником причин смерти
        /// <summary>
        /// Сохранение всех причин смерти в файле XML
        /// Пока что временно - а там посмотрим
        /// </summary>
        /// <param name="reasons"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool SaveReasonsToFile(List<DtoReason> reasons, string filePath);

        /// <summary>
        /// Чтение справочника причин смерти из файла XML
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<DtoReason> ReadReasonsFromFile(string filePath);

        /// <summary>
        /// Добавление причины с общий список
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool AddReasonToList(DtoReason reason);
        #endregion

        #region Блок работы с категориями возрастов
        bool SaveCategoriesToFile(List<DtoCategory> categories, string filePath);
        #endregion

        #region Блок работы с людьми для сравнения
        bool SaveComparedHumansToFile(List<DtoComparedHuman> comparedHumans, string filePath);
        #endregion

        #region Блок работы с людьми
        //bool SaveHumansToFile(List<DtoHuman> humans, string filePath);
        #endregion
    }
}