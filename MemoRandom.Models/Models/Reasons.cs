using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MemoRandom.Models.Models
{
    /// <summary>
    /// Статический класс, хранящий в себе причины (иерархическое дерево и плоский список)
    /// </summary>
    public static class Reasons
    {
        /// <summary>
        /// Иерархическая коллекция причин смерти
        /// </summary>
        public static ObservableCollection<Reason> ReasonsCollection { get; set; } = new();


        /// <summary>
        /// Плоский список причин смерти для отображения
        /// </summary>
        public static List<Reason> PlainReasonsList { get; set; } = new();
    }
}
