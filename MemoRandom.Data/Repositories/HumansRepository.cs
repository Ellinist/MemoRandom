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
        /// Строка соединения с базой данных
        /// </summary>
        public static string DbConnectionString { get; set; }

        /// <summary>
        /// Путь к папке хранения изображений
        /// </summary>
        public static string ImageFolder { get; set; }

        /// <summary>
        /// Статический класс, получаемый из внешнего хранилища
        /// </summary>
        public static List<Human> HumansList { get; set; } = new();

        /// <summary>
        /// Текущий человек, с которым ведется работа
        /// </summary>
        public static Human CurrentHuman { get; set; }
    }
}