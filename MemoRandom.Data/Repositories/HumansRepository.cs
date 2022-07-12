using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoRandom.Models.Models;

namespace MemoRandom.Data.Repositories
{
    /// <summary>
    /// Статический класс - репозиторий по людям
    /// </summary>
    public static class HumansRepository
    {
        /// <summary>
        /// Статический класс, получаемый из внешнего хранилища
        /// </summary>
        public static List<Human> HumansList { get; set; }
    }
}