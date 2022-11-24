using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Models;
using MemoRandom.Data.DbModels;
using MemoRandom.Data.Interfaces;
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
        private readonly IMsSqlController _msSqlController;

        private string _categoriesTitle = "Возрастные категории";
        private IEnumerable<PropertyInfo> _colorsList;
        private ObservableCollection<Category> _categoriesCollection;
        private int _selectedIndex;
        private Guid _categoryId;
        private string _categoryName;
        private int _periodFrom;
        private int _periodTo;
        private int _selectedComboIndex;
        private Category _selectedCategory;
        private object _selectedComboColor;
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

                CategoryId = SelectedCategory.CategoryId;
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
                if(value != null)
                {
                    _selectedComboColor = value;
                    var selectedItem = (PropertyInfo)value;
                    SelectedColor = (Color)selectedItem.GetValue(null, null);

                    RaisePropertyChanged(nameof(SelectedComboColor));
                }
            }
        }

        /// <summary>
        /// Выбранный цвет
        /// </summary>
        private Color SelectedColor { get; set; }
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

        private bool newFlag = false;

        /// <summary>
        /// Создание новой категории
        /// </summary>
        private void NewCategory()
        {
            newFlag = true;
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
            if (!newFlag) // Существующая запись категории
            {
                #region Обновление выбранной категории
                SelectedCategory.CategoryName  = CategoryName;
                SelectedCategory.StartAge      = PeriodFrom;
                SelectedCategory.StopAge       = PeriodTo;
                SelectedCategory.CategoryColor = SelectedColor;
                #endregion

                await Task.Run(() =>
                {
                    DbCategory cat = new()
                    {
                        CategoryId = SelectedCategory.CategoryId,
                        CategoryName = SelectedCategory.CategoryName,
                        PeriodFrom = SelectedCategory.StartAge,
                        PeriodTo = SelectedCategory.StopAge,
                        ColorA = (byte)SelectedCategory.CategoryColor.ScA,
                        ColorR = (byte)SelectedCategory.CategoryColor.ScR,
                        ColorG = (byte)SelectedCategory.CategoryColor.ScG,
                        ColorB = (byte)SelectedCategory.CategoryColor.ScB
                    };

                    var result = _msSqlController.UpdateCategories(cat);
                    if (!result)
                    {
                        MessageBox.Show("Не удалось обновить категорию", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                });

                //CategoriesCollection?.Clear();
                CommonDataController.RearrangeCollection();
                //CategoriesCollection = Categories.GetCategories();
                RaisePropertyChanged(nameof(CategoriesCollection));
                SelectedIndex = CategoriesCollection.IndexOf(SelectedCategory);
            }
            else // Создание новой категории
            {
                Category category = new()
                {
                    CategoryId = CategoryId,
                    CategoryName = CategoryName,
                    StartAge = PeriodFrom,
                    StopAge = PeriodTo,
                    CategoryColor = SelectedColor
                };

                DbCategory cat = new()
                {
                    CategoryId = CategoryId,
                    CategoryName = CategoryName,
                    PeriodFrom = PeriodFrom,
                    PeriodTo = PeriodTo,
                    ColorA = SelectedColor.A,
                    ColorR = SelectedColor.R,
                    ColorG = SelectedColor.G,
                    ColorB = SelectedColor.B
                };

                await Task.Run(() =>
                {
                    var result = _msSqlController.UpdateCategories(cat);
                    if (!result)
                    {
                        MessageBox.Show("Не удалось добавить категорию", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                });

                //CategoriesCollection?.Clear();
                CommonDataController.AgeCategories.Add(category);
                CommonDataController.RearrangeCollection();
                //CategoriesCollection = Categories.GetCategories();
                RaisePropertyChanged(nameof(CategoriesCollection));
                SelectedIndex = CategoriesCollection.IndexOf(category);
            }

            newFlag = false;

            ChangeCategory.Invoke();

            return;

            ////TODO Здесь проверка на валидность начала и конца срока действия категории
            //// Проверять, чтобы конец не был меньше или равен началу - уведомление
            //// Проверять, чтобы не было пересечения с другими категориями - уведомление
            //var start = CategoriesCollection.FirstOrDefault(x => PeriodFrom >= x.StartAge &&
            //                                               PeriodFrom <= x.StopAge);

            //if(start == null)
            //{
            //    var stop = CategoriesCollection.FirstOrDefault(x => PeriodTo <= x.StopAge &&
            //                                                  PeriodTo >= x.StartAge);

            //    if (stop == null)
            //    {
            //    }
            //}

            //MessageBox.Show("Пересечение временных диапазонов!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Удаление выбранной категории
        /// </summary>
        private async void DeleteCategory()
        {
            if (SelectedCategory == null) return; // Здесь можно еще уведомление дать

            if (!await Task.Run(() => _msSqlController.DeleteCategory(SelectedCategory.CategoryId)))
            {
                MessageBox.Show("Не получилось удалить выбранную категорию!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CommonDataController.AgeCategories.Remove(SelectedCategory);
            //CategoriesCollection.Clear();
            //CategoriesCollection = Categories.GetCategories();
            RaisePropertyChanged(nameof(CategoriesCollection));
            SelectedIndex = 0;
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

            SelectedIndex = 0;

            CategoryName = SelectedCategory.CategoryName;
            PeriodFrom   = SelectedCategory.StartAge;
            PeriodTo     = SelectedCategory.StopAge;

            // Устанавливаем индекс списка выбора цвета на позицию выбранного в таблице цвета
            SelectedComboIndex = typeof(Colors).GetProperties()
                                               .Select(p => p.GetValue(null)).ToList()
                                               .FindIndex(x => (Color)x == SelectedCategory.CategoryColor);

            RaisePropertyChanged(nameof(CategoriesCollection));
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            NewCategoryCommand = new DelegateCommand(NewCategory);
            SaveCategoryCommand = new DelegateCommand(SaveCategory);
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

            CategoriesCollection = new(); // Создаем список категорий

            ColorsList = typeof(Colors).GetProperties(); // Получаем список свойств с цветами

            InitCommands();
        }
        #endregion
    }
}
