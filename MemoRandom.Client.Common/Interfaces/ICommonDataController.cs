using MemoRandom.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

        bool UpdateHumanData(Human human, BitmapImage humanImage);

        /// <summary>
        /// Получение изображения человека
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        BitmapImage GetHumanImage(Human currentHuman);

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
