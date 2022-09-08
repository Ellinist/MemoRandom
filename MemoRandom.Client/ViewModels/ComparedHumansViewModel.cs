using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления для окна добавления люедй для сравнения
    /// </summary>
    public class ComparedHumansViewModel :BindableBase
    {
        #region PRIVATE FIELDS
        private string _comparedHumansTitle = "Люди для сравнения";
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string ComparedHumansTitle
        {
            get => _comparedHumansTitle;
            set
            {
                _comparedHumansTitle = value;
                RaisePropertyChanged(nameof(ComparedHumansTitle));
            }
        }
        #endregion

        #region COMMANDS

        #endregion








        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        public ComparedHumansViewModel()
        {

        }
        #endregion
    }
}
