using MemoRandom.Models.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        ///// <summary>
        ///// Получение иерархической коллекции причин смерти
        ///// </summary>
        ///// <returns></returns>
        //ObservableCollection<Reason> GetReasonsCollection();

        ///// <summary>
        ///// Получение плоского списка причин смерти
        ///// </summary>
        ///// <returns></returns>
        //List<Reason> GetReasonsList();

        //void AddReasonToPlainList(Reason reason);
    }
}
