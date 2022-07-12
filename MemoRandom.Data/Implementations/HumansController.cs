using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoRandom.Data.Interfaces;
using MemoRandom.Data.Repositories;
using MemoRandom.Models.Models;

namespace MemoRandom.Data.Implementations
{
    /// <summary>
    /// Контроллер работы с людьми
    /// </summary>
    public class HumansController : IHumansController
    {
        private readonly IMemoRandomDbController _memoRandomDbController;

        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public List<Human> GetHumansList()
        {
            HumansRepository.HumansList = _memoRandomDbController.GetHumasList();
            return HumansRepository.HumansList;
        }

        /// <summary>
        /// Установка текущего человека, с которым ведется работа
        /// </summary>
        /// <param name="human"></param>
        public void SetCurrentHuman(Human human)
        {
            HumansRepository.CurrentHuman = human;
        }

        /// <summary>
        /// Получение текущего человека, с которым ведется работа
        /// </summary>
        /// <returns></returns>
        public Human GetCurrentHuman()
        {
            return HumansRepository.CurrentHuman;
        }

        #region CTOR
        public HumansController(IMemoRandomDbController memoRandomDbController)
        {
            _memoRandomDbController = memoRandomDbController ?? throw new ArgumentNullException(nameof(memoRandomDbController));
        }
        #endregion
    }
}
