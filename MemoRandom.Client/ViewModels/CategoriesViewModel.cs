using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

        private IEnumerable<PropertyInfo> _colorsList;
        private ObservableCollection<Category> _categoriesList;
        private string _categoryName;
        private int _periodFrom;
        private int _periodTo;
        private int _selectedComboIndex;
        private Category _selectedCategory;
        private object _selectedComboColor;
        #endregion

        #region PROPS
        /// <summary>
        /// Список используемых категорий
        /// </summary>
        public ObservableCollection<Category> CategoriesList
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

        /// <summary>
        /// Выбранная в таблице категория
        /// </summary>
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (value == null) return;
                _selectedCategory = value;

                CategoryName = SelectedCategory.CategoryName;
                PeriodFrom = SelectedCategory.StartAge;
                PeriodTo = SelectedCategory.StopAge;
                // Устанавливаем индекс списка выбора цвета на позицию выбранного в таблице цвета
                SelectedComboIndex = typeof(Colors).GetProperties()
                                                   .Select(p => p.GetValue(null)).ToList()
                                                   .FindIndex(x => (Color)x == _selectedCategory.CategoryColor);

                RaisePropertyChanged(nameof(SelectedCategory));
            }
        }

        /// <summary>
        /// Список цветов
        /// </summary>
        public IEnumerable<PropertyInfo> ColorsList
        {
            get => _colorsList;
            set
            {
                _colorsList = value;
                RaisePropertyChanged(nameof(ColorsList));
            }
        }

        
        /// <summary>
        /// Индекс цвета в списке выбора
        /// </summary>
        public int SelectedComboIndex
        {
            get => _selectedComboIndex;
            set
            {
                _selectedComboIndex = value;
                RaisePropertyChanged(nameof(SelectedComboIndex));
            }
        }

        /// <summary>
        /// Выбранный в списке цвет
        /// </summary>
        public object SelectedComboColor
        {
            get => _selectedComboColor;
            set
            {
                _selectedComboColor = value;
                var selectedItem = (PropertyInfo)value;
                SelectedColor = (Color)selectedItem.GetValue(null, null);

                RaisePropertyChanged(nameof(SelectedComboColor));
            }
        }

        /// <summary>
        /// Выбранный цвет
        /// </summary>
        private Color SelectedColor { get; set; }
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

        /// <summary>
        /// Добавление категории
        /// </summary>
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
                StopAge = PeriodTo,
                CategoryColor = SelectedColor
            };

            CategoriesList.Add(cat);
            RaisePropertyChanged(nameof(CategoriesList));

            _msSqlController.AddCategoryToList(cat);
        }

        /// <summary>
        /// Удаление выбранной категории
        /// </summary>
        private void DeleteCategory()
        {
            if (SelectedCategory == null) return; // Здесь можно еще уведомление дать

            if (!_msSqlController.DeleteCategoryFromList(SelectedCategory))
            {
                MessageBox.Show("Не получилось удалить выбранную категорию!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CategoriesList.Remove(SelectedCategory);
            RaisePropertyChanged(nameof(CategoriesList));
        }

        /// <summary>
        /// Обработчик загрузки окна категорий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CategoriesView_Loaded(object sender, RoutedEventArgs e)
        {
            // Вынести в метод на момент загрузки окна
            var categoriesRead = _msSqlController.GetCategories();
            if(categoriesRead != null)
            {
                CategoriesList = _msSqlController.GetCategories();
            }

            SelectedCategory = CategoriesList[0];

            CategoryName = SelectedCategory.CategoryName;
            PeriodFrom = SelectedCategory.StartAge;
            PeriodTo = SelectedCategory.StopAge;
            // Устанавливаем индекс списка выбора цвета на позицию выбранного в таблице цвета
            SelectedComboIndex = typeof(Colors).GetProperties()
                                               .Select(p => p.GetValue(null)).ToList()
                                               .FindIndex(x => (Color)x == SelectedCategory.CategoryColor);

            RaisePropertyChanged(nameof(CategoriesList));
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            AddCategoryCommand = new DelegateCommand(AddCategory);
            DeleteCategoryCommand = new DelegateCommand(DeleteCategory);
        }








        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="msSqlController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CategoriesViewModel(IMsSqlController msSqlController)
        {
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));

            CategoriesList = new(); // Создаем список категорий

            ColorsList = typeof(Colors).GetProperties(); // Получаем список свойств с цветами

            InitCommands();
        }
        #endregion
    }
}
