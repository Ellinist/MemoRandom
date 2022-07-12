using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoRandom.Models.Models;

namespace MemoRandom.Data.Interfaces
{
    public interface IHumansController
    {
        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        List<Human> GetHumansList();
    }
}
