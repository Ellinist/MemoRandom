using MemoRandom.Models.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MemoRandom.Models.Interfaces
{
    public interface IReasonsHelper
    {
        /// <summary>
        /// Формироваие иерархической коллекции причин смерти - для отображения в TreeView
        /// </summary>
        /// <param name="reasonsCollection"></param>
        void SetReasonsHierarchicalCollection(ObservableCollection<Reason> reasonsCollection);

        /// <summary>
        /// Получение иерархической коллекции причин смерти
        /// </summary>
        /// <returns></returns>
        ObservableCollection<Reason> GetReasonsHierarchicalCollection();

        /// <summary>
        /// Формирование плоского списка - для отображения в окне деталей
        /// </summary>
        /// <param name="reasonsList"></param>
        void SetReasonsPlainList(List<Reason> reasonsList);

        /// <summary>
        /// Получение плоского списка - для целей отображений в окнах работы с людьми
        /// </summary>
        /// <returns></returns>
        List<Reason> GetReasonsPlainList();
    }
}
