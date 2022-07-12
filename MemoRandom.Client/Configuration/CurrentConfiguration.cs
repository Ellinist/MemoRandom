using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoRandom.Models.Models;

namespace MemoRandom.Client.Configuration
{
    public static class CurrentConfiguration
    {
        private static string _currentHumanId;

        public static List<Human> CurrentHumansList { get; set; } = new();

        public static void SetCurrentHuman(Human currentHuman)
        {
            if(currentHuman == null)
            {
                _currentHumanId = String.Empty;
            }
            else
            {
                _currentHumanId = currentHuman.HumanId.ToString();
            }
        }

        public static Human GetCurrentHuman()
        {
            if (_currentHumanId == String.Empty)
            {
                return null;
            }
            else
            {
                return CurrentHumansList.FirstOrDefault(x => x.HumanId.ToString() == _currentHumanId);
            }
        }
    }
}
