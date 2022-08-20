using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MemoRandom.Models.Models
{
    /// <summary>
    /// Статический класс, содержащий в себе список людей
    /// </summary>
    public static class Humans
    {
        ///// <summary>
        ///// Статический класс, получаемый из внешнего хранилища
        ///// </summary>
        //public static ObservableCollection<Human> HumansList { get; set; } = new();

        /// <summary>
        /// Текущий человек, с которым ведется работа
        /// </summary>
        public static Human CurrentHuman { get; set; }
    }
}
