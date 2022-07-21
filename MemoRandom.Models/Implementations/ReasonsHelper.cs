using MemoRandom.Models.Interfaces;
using MemoRandom.Models.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MemoRandom.Models.Implementations
{
    public class ReasonsHelper : IReasonsHelper
    {
        /// <summary>
        /// Формироваие иерархической коллекции причин смерти - для отображения в TreeView
        /// </summary>
        /// <param name="reasonsCollection"></param>
        public ObservableCollection<Reason> GetReasonsHierarchicalCollection()
        {
            return Reasons.ReasonsCollection;
        }

        /// <summary>
        /// Получение иерархической коллекции причин смерти
        /// </summary>
        /// <returns></returns>
        public void SetReasonsHierarchicalCollection(ObservableCollection<Reason> reasonsCollection)
        {
            Reasons.ReasonsCollection = reasonsCollection;
        }

        /// <summary>
        /// Формирование плоского списка - для отображения в окне деталей
        /// </summary>
        /// <param name="reasonsList"></param>
        public void SetReasonsPlainList(List<Reason> reasonsList)
        {
            Reasons.PlainReasonsList = reasonsList;
        }

        /// <summary>
        /// Получение плоского списка - для целей отображений в окнах работы с людьми
        /// </summary>
        /// <returns></returns>
        public List<Reason> GetReasonsPlainList()
        {
            return Reasons.PlainReasonsList;
        }
    }
}
