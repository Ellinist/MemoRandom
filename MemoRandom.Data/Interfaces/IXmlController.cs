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
        /// Добавление причины с общий список
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool AddReasonToList(DtoReason reason);
        #endregion
    }
}
