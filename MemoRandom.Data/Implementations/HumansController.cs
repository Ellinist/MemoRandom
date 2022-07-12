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
    public class HumansController : IHumansController
    {
        private readonly IMemoRandomDbController _memoRandomDbController;

        public List<Human> GetHumansList()
        {
            HumansRepository.HumansList = _memoRandomDbController.GetHumasList();
            return HumansRepository.HumansList;
        }

        public void SetCurrentHuman(Human human)
        {
            HumansRepository.CurrentHuman = human;
        }

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
