using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Models.Models
{
    public static class ComparedHumans
    {
        /// <summary>
        /// Список людей для сравнения (статический)
        /// </summary>
        public static BindingList<ComparedHuman> ComparedHumansList { get; set; }

        /// <summary>
        /// Получение клонированного списка людей для сравнения
        /// </summary>
        /// <returns></returns>
        public static BindingList<ComparedHuman> GetComparedHumans()
        {
            var comparedHumans = new BindingList<ComparedHuman>();
            foreach (var item in ComparedHumansList)
            {
                comparedHumans.Add(item);
            }

            return comparedHumans;
        }
    }
}
