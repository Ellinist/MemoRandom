using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
        public static Action ChangeCategory { get; set; }

        #region PRIVATE FIELDS
        private readonly ICommonDataController _commonDataController;

        private string _categoriesTitle = "Возрастные категории";
        private IEnumerable<PropertyInfo> _colorsList;
        private ObservableCollection<Category> _categoriesCollection = new();
        private int _selectedIndex;
        private Guid _categoryId;
        private string _categoryName;
        private int _periodFrom;
        private int _periodTo;
        private int _selectedComboIndex;
        private Category _selectedCategory;
        private string _selectedPickerColor;
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна категорий
        /// </summary>
        public string CategoriesTitle
        {
            get => _categoriesTitle;
            set
            {
                _categoriesTitle = value;
                RaisePropertyChanged(nameof(CategoriesTitle));
            }
        }

        /// <summary>
        /// Список используемых категорий
        /// </summary>
        public ObservableCollection<Category> CategoriesCollection
        {
            get => _categoriesCollection;
            set
            {
                _categoriesCollection = value;
                RaisePropertyChanged(nameof(CategoriesCollection));
            }
        }

        /// <summary>
        /// Индекс в списке категорий
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(nameof(SelectedIndex));
            }
        }

        /// <summary>
        /// Идентификатор категории
        /// </summary>
        public Guid CategoryId
        {
            get => _categoryId;
            set
            {
                _categoryId = value;
                RaisePropertyChanged(nameof(CategoryId));
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

                CategoryId   = SelectedCategory.CategoryId;
                CategoryName = SelectedCategory.CategoryName;
                PeriodFrom   = SelectedCategory.StartAge;
                PeriodTo     = SelectedCategory.StopAge;
                SelectedPickerColor = SelectedCategory.CategoryColor;
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
        /// Выбранный в пикере цвет
        /// </summary>
        public string SelectedPickerColor
        {
            get => _selectedPickerColor;
            set
            {
                _selectedPickerColor = value;
                SelectedColor = value;
                RaisePropertyChanged(nameof(SelectedPickerColor));
            }
        }

        /// <summary>
        /// Выбранный цвет
        /// </summary>
        private string SelectedColor { get; set; }
        #endregion

        #region COMMANDS
        /// <summary>
        /// Команда добавления новой категории
        /// </summary>
        public DelegateCommand NewCategoryCommand { get; private set; }

        /// <summary>
        /// Команда сохранения категории
        /// </summary>
        public DelegateCommand SaveCategoryCommand { get; private set; } 

        /// <summary>
        /// Команда удаления категории
        /// </summary>
        public DelegateCommand DeleteCategoryCommand { get; private set; }
        #endregion

        private bool _newFlag = false;

        /// <summary>
        /// Создание новой категории
        /// </summary>
        private void NewCategory()
        {
            _newFlag = true;
            CategoryId = Guid.NewGuid();
            CategoryName = "Введите название!";
            PeriodFrom = 0;
            PeriodTo = 0;
        }

        /// <summary>
        /// Сохранение категории
        /// </summary>
        private async void SaveCategory()
        {
            if (!_newFlag) // Существующая запись категории
            {
                #region Обновление выбранной категории
                SelectedCategory.CategoryName  = CategoryName;
                SelectedCategory.StartAge      = PeriodFrom;
                SelectedCategory.StopAge       = PeriodTo;
                SelectedCategory.CategoryColor   = SelectedColor;
                #endregion

                if (!await Task.Run(() => _commonDataController.UpdateCategoriesInFile(SelectedCategory)))
                {
                        MessageBox.Show("Не удалось обновить категорию", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                };

                CommonDataController.RearrangeCollection();
                RaisePropertyChanged(nameof(CategoriesCollection));
                SelectedIndex = CategoriesCollection.IndexOf(SelectedCategory);
            }
            else // Создание новой категории
            {
                Category category = new()
                {
                    CategoryId    = CategoryId,
                    CategoryName  = CategoryName,
                    StartAge      = PeriodFrom,
                    StopAge       = PeriodTo,
                    CategoryColor   = SelectedColor.ToString()
                };

                if (!await Task.Run(() => _commonDataController.UpdateCategoriesInFile(category)))
                {
                    MessageBox.Show("Не удалось добавить категорию", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CommonDataController.AgeCategories.Add(category);
                CommonDataController.RearrangeCollection();
                RaisePropertyChanged(nameof(CategoriesCollection));
                SelectedIndex = CategoriesCollection.IndexOf(category);
            }

            _newFlag = false;

            ChangeCategory.Invoke();

            return;

            ////TODO Здесь проверка на валидность начала и конца срока действия категории
            //// Проверять, чтобы конец не был меньше или равен началу - уведомление
            //// Проверять, чтобы не было пересечения с другими категориями - уведомление
            //MessageBox.Show("Пересечение временных диапазонов!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Удаление выбранной категории
        /// </summary>
        private async void DeleteCategory()
        {
            if (SelectedCategory == null) return; // Здесь можно еще уведомление дать

            if (!await Task.Run(() => _commonDataController.DeleteCategoryInFile(SelectedCategory.CategoryId)))
            {
                MessageBox.Show("Не получилось удалить выбранную категорию!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CommonDataController.AgeCategories.Remove(SelectedCategory);
            RaisePropertyChanged(nameof(CategoriesCollection));
        }

        /// <summary>
        /// Обработчик загрузки окна категорий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CategoriesView_Loaded(object sender, RoutedEventArgs e)
        {
            CategoriesCollection = CommonDataController.AgeCategories;
            if(CategoriesCollection.Count == 0) return;

            SelectedCategory = CategoriesCollection[0];
            CategoryName     = SelectedCategory.CategoryName;
            PeriodFrom       = SelectedCategory.StartAge;
            PeriodTo         = SelectedCategory.StopAge;

            RaisePropertyChanged(nameof(CategoriesCollection));
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            NewCategoryCommand    = new DelegateCommand(NewCategory);
            SaveCategoryCommand   = new DelegateCommand(SaveCategory);
            DeleteCategoryCommand = new DelegateCommand(DeleteCategory);
        }








        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="commonDataController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CategoriesViewModel(ICommonDataController commonDataController)
        {
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

            InitCommands();
        }
        #endregion
    }
}
