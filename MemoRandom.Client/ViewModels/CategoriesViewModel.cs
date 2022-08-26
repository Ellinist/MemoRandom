using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для категорий
    /// </summary>
    public class CategoriesViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private readonly IMsSqlController _msSqlController;

        private List<Category> _categoriesList = new();
        private string _categoryName;
        private int _periodFrom;
        private int _periodTo;
        private Color _colorFill;
        #endregion

        #region PROPS
        /// <summary>
        /// Список используемых категорий
        /// </summary>
        public List<Category> CategoriesList
        {
            get => _categoriesList;
            set
            {
                _categoriesList = value;
                RaisePropertyChanged(nameof(CategoriesList));
            }
        }

        /// <summary>
        /// Название категории
        /// </summary>
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                RaisePropertyChanged(nameof(CategoryName));
            }
        }

        /// <summary>
        /// С какого возраста действует категория
        /// </summary>
        public int PeriodFrom
        {
            get => _periodFrom;
            set
            {
                _periodFrom = value;
                RaisePropertyChanged(nameof(PeriodFrom));
            }
        }

        /// <summary>
        /// До какого возраста действует категория
        /// </summary>
        public int PeriodTo
        {
            get => _periodTo;
            set
            {
                _periodTo = value;
                RaisePropertyChanged(nameof(PeriodTo));
            }
        }

        public Color ColorFill
        {
            get => _colorFill;
            set
            {
                _colorFill = value;
                RaisePropertyChanged(nameof(ColorFill));
            }
        }
        #endregion

        #region COMMANDS
        /// <summary>
        /// Команда добавления категории
        /// </summary>
        public DelegateCommand AddCategoryCommand { get; private set; } 

        /// <summary>
        /// Команда удаления категории
        /// </summary>
        public DelegateCommand DeleteCategoryCommand { get; private set; }
        #endregion

        private void AddCategory()
        {
            //TODO Здесь проверка на валидность начала и конца срока действия категории
            // Проверять, чтобы конец не был меньше или равен началу - уведомление
            // Проверять, чтобы не было пересечения с другими категориями - уведомление

            Category cat = new()
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = CategoryName,
                StartAge = PeriodFrom,
                StopAge = PeriodTo
            };

            CategoriesList.Add(cat);
            RaisePropertyChanged(nameof(CategoriesList));

            _msSqlController.AddCategoryToList(cat);
        }

        public void CategoriesView_Loaded(object sender, RoutedEventArgs e)
        {
            // Вынести в метод на момент загрузки окна
            CategoriesList = _msSqlController.GetCategories();
            RaisePropertyChanged(nameof(CategoriesList));
        }

        private void InitCommands()
        {
            AddCategoryCommand = new DelegateCommand(AddCategory);
        }








        public CategoriesViewModel(IMsSqlController msSqlController)
        {
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));

            InitCommands();
        }
    }
}
