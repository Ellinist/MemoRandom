using MemoRandom.Models.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Data.Repositories
{
    public static class ReasonsRepository
    {
        /// <summary>
        /// Иерархическая коллекция причин смерти
        /// </summary>
        public static ObservableCollection<Reason> ReasonsCollection { get; set; } = new();


        /// <summary>
        /// Плоский список причин смерти для отображения
        /// </summary>
        public static List<Reason> ReasonsList { get; set; } = new();
    }
}
