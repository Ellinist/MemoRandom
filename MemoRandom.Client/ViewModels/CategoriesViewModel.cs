using MemoRandom.Models.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для категорий
    /// </summary>
    public class CategoriesViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private List<LifePeriodType> _categoriesList = new();
        private string _categoryName;
        private int _periodFrom;
        private int _periodTo;
        #endregion

        #region PROPS
        public List<LifePeriodType> CategoriesList
        {
            get => _categoriesList;
            set
            {
                _categoriesList = value;
                RaisePropertyChanged(nameof(CategoriesList));
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                RaisePropertyChanged(nameof(CategoryName));
            }
        }

        public int PeriodFrom
        {
            get => _periodFrom;
            set
            {
                _periodFrom = value;
                RaisePropertyChanged(nameof(PeriodFrom));
            }
        }

        public int PeriodTo
        {
            get => _periodTo;
            set
            {
                _periodTo = value;
                RaisePropertyChanged(nameof(PeriodTo));
            }
        }
        #endregion
    }
}
