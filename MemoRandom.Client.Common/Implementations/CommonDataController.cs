using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using MemoRandom.Client.Common.Models;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using NLog;
using MemoRandom.Client.Common.Enums;
using MemoRandom.Data.DtoModels;
using System.Configuration;

namespace MemoRandom.Client.Common.Implementations
{
    public class CommonDataController : ICommonDataController
    {
        #region PRIVATE FIELDS
        private readonly ILogger _logger;
        private readonly IXmlController _xmlController;
        private readonly IMapper _mapper;

        private string _reasonsFilePath;
        private string _categoriesFilePath;
        private string _comparedHumansFilePath;
        private string _humansFilePath;
        private string _imageFolder;
        #endregion

        #region PROPS
        /// <summary>
        /// Иерархическая коллекция причин смерти
        /// </summary>
        public static ObservableCollection<Reason> ReasonsCollection { get; set; } = new();

        /// <summary>
        /// Плоский список причин смерти для отображения
        /// </summary>
        public static List<Reason> PlainReasonsList { get; set; } = new();

        /// <summary>
        /// Коллекция категорий (статическая)
        /// </summary>
        public static ObservableCollection<Category> AgeCategories { get; set; } = new();

        /// <summary>
        /// Коллекция людей для сравнения
        /// </summary>
        public static ObservableCollection<ComparedHuman> ComparedHumansCollection { get; set; } = new();

        /// <summary>
        /// Список людей - основной список
        /// </summary>
        public static ObservableCollection<Human> HumansList { get; set; } = new();

        /// <summary>
        /// Текущий выбор человека
        /// </summary>
        public static Human CurrentHuman { get; set; }
        #endregion

        #region IMPLEMENTATION
        /// <summary>
        /// Установка путей доступа к файлам хранения информации
        /// </summary>
        public bool SetFilesPaths()
        {
            bool success = true;
            // Получаем папку, где установлено приложение и добавляем папку хранения XML-файлов
            var xmlFolder = AppDomain.CurrentDomain.BaseDirectory + @"\Data";
            // Получаем папку, где установлено приложение и добавляем папку хранения изображений
            var imageFolder = AppDomain.CurrentDomain.BaseDirectory + @"\Data\Images";

            try
            {
                // Проверяем, существует ли папка, где хранятся данные
                if (!Directory.Exists(xmlFolder))
                {
                    Directory.CreateDirectory(xmlFolder); // Если не существует, то создаем
                }

                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                // Путь к файлу хранения причин смерти
                _reasonsFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["ReasonsFile"]!);
                // Путь к файлу хранения возрастных категорий
                _categoriesFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["CategoriesFile"]!);
                // Путь к файлу хранения списка людей для сравнения
                _comparedHumansFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["ComparedHumansFile"]!);
                // Путь к файлу хранения основного списка людей
                _humansFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["HumansFile"]!);

                _imageFolder = Path.Combine(imageFolder);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Неудачная установка путей к файлам: {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Чтение информации из XML-файлов
        /// </summary>
        public bool ReadXmlData()
        {
            bool success = true; // Флаг успешности операции чтения файлов

            try
            {
                #region Чтение причин смерти из файла
                PlainReasonsList.Clear(); // Чистим плоский список
                ReasonsCollection.Clear(); // Чистим иерархическую коллекцию
                var reasonsResult = _xmlController.ReadReasonsFromFile(_reasonsFilePath);
                PlainReasonsList = _mapper.Map<List<DtoReason>, List<Reason>>(reasonsResult);
                FormObservableCollection(PlainReasonsList, null); // Формируем иерархическую коллекцию
                #endregion

                #region Чтение возрастных категорий
                AgeCategories.Clear(); // Чистим список категорий
                var categoriesResult = _xmlController.ReadCategoriesFromFile(_categoriesFilePath);
                AgeCategories = _mapper.Map<List<DtoCategory>, ObservableCollection<Category>>(categoriesResult);
                //foreach (var item in AgeCategories) // Преобразование строк в цвет
                //{
                //    //item.CategoryColor = (Color)ColorConverter.ConvertFromString(item.StringColor)!;
                //    item.StringColor = (Color)ColorConverter.ConvertFromString(item.StringColor)!;
                //}
                #endregion

                #region Чтение людей для сравнения
                ComparedHumansCollection.Clear(); // Чистим список людей для сравнения
                var comparedHumansResult = _xmlController.ReadComparedHumansFromFile(_comparedHumansFilePath);
                ComparedHumansCollection = _mapper.Map<List<DtoComparedHuman>, ObservableCollection<ComparedHuman>>(comparedHumansResult);
                #endregion

                #region Чтение основного списка людей
                HumansList.Clear(); // Чистим основной список людей
                var humansResult = _xmlController.ReadHumansFromFile(_humansFilePath).OrderBy(x => x.DaysLived).ToList();
                HumansList = _mapper.Map<List<DtoHuman>, ObservableCollection<Human>>(humansResult);
                foreach (var human in HumansList)
                {
                    var res = PlainReasonsList.FirstOrDefault(x => x.ReasonId == human.DeathReasonId);
                    human.HumanDeathReasonName = (res != null) ? res.ReasonName : string.Empty;
                }
                #endregion
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Неудачное чтение информации из XML-файлов: {ex.HResult}");
            }

            return success;
        }

        public bool SaveReasons()
        {
            bool success = true; // Флаг успешности операции

            try
            {
                List<DtoReason> dtoReasons = _mapper.Map<List<Reason>, List<DtoReason>>(PlainReasonsList);
                _xmlController.SaveReasonsToFile(dtoReasons, _reasonsFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка сохранения {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Добавление причины в список
        /// </summary>
        /// <param name="reason"></param>
        public bool AddReasonToFile(Reason reason)
        {
            bool success = true; // Флаг успешности операции добавления причины смерти в файл

            try
            {
                var dtoReason = _mapper.Map<Reason, DtoReason>(reason);
                _xmlController.AddReasonToList(dtoReason, _reasonsFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка добавления причины смерти {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Изменение причины в списке
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool ChangeReasonInFile(Reason reason)
        {
            bool success = true;

            try
            {
                var dtoReason = _mapper.Map<Reason, DtoReason>(reason);
                _xmlController.ChangeReasonInFile(dtoReason, _reasonsFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка изменения причины смерти {ex.HResult}");
            }
            
            return success;
        }

        /// <summary>
        /// Удаление причины и всех ее дочерних узлов (если таковые есть)
        /// </summary>
        /// <param name="guidList"></param>
        /// <returns></returns>
        public bool DeleteReasonAndDaughtersInFile(List<Guid> guidList)
        {
            bool success = true;

            try
            {
                foreach (var id in guidList)
                {
                    _xmlController.DeleteReasonInFile(id.ToString(), _reasonsFilePath);
                }
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка удаления причины смерти {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Обновление/добавление категории в файле
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool UpdateCategoriesInFile(Category category)
        {
            bool success = true;

            try
            {
                var dtoCategory = _mapper.Map<Category, DtoCategory>(category);
                _xmlController.UpdateCategoryInFile(dtoCategory, _categoriesFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка обновления возрастной категории {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Удаление категории в файле
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteCategoryInFile(Guid id)
        {
            var success = true;

            try
            {
                _xmlController.DeleteCategoryInFile(id.ToString(), _categoriesFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка удаления возрастной категории {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Обновление/добавление человека для сравнения
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <returns></returns>
        public bool UpdateComparedHuman(ComparedHuman comparedHuman)
        {
            var success = true;

            try
            {
                var dtoComparedHuman = _mapper.Map<ComparedHuman, DtoComparedHuman>(comparedHuman);
                _xmlController.UpdateComparedHumanInFile(dtoComparedHuman, _comparedHumansFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка обновления человека для сравнения {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Удаление человека для сравнения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteComparedHuman(Guid id)
        {
            var success = true;

            try
            {
                _xmlController.DeleteComparedHumanInFile(id.ToString(), _comparedHumansFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка удаления человека для сравнения {ex.HResult}");
            }

            return success;
        }

        /// <summary>
        /// Обновление/добавление человека из основного списка людей
        /// </summary>
        /// <param name="human"></param>
        /// <param name="humanImage"></param>
        /// <returns></returns>
        public bool UpdateHuman(Human human, BitmapImage humanImage)
        {
            var success = true;

            try
            {
                var dtoHuman = _mapper.Map<Human, DtoHuman>(human);
                _xmlController.UpdateHumanInFile(dtoHuman, _humansFilePath);

                if (humanImage != null)
                {
                    SaveImageToFile(humanImage, human); // Сохраняем изображение
                }
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка обновления/добавления человека из основного списка {ex.HResult}");
            }

            return success;
        }

        public bool DeleteHuman(Human human, string imageFile)
        {
            bool success = true;

            try
            {
                _xmlController.DeleteHumanInFile(human.HumanId.ToString(), _humansFilePath);

                if (imageFile != string.Empty)
                {
                    if (!DeleteImageFile(imageFile))
                    {
                        success = false; // Если файл изображения удалить не удалось
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка удаления человека из основного списка {ex.HResult}");
            }
            

            return success;
        }

        /// <summary>
        /// Обновление иерархической коллекции причин смерти
        /// </summary>
        public void UpdateHierarchicalReasonsData()
        {
            ReasonsCollection.Clear();
            FormObservableCollection(PlainReasonsList, null);
        }
        #endregion

        /// <summary>
        /// Получение изображения выбранного человека
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        public BitmapImage GetHumanImage(Human currentHuman)
        {
            // Читаем файл изображения, если выбранный человек существует и у него есть изображение
            if (currentHuman == null || currentHuman.ImageFile == string.Empty) return null;

            string combinedImagePath = Path.Combine(_imageFolder, currentHuman.ImageFile);

            if (!File.Exists(combinedImagePath)) return null;

            using Stream stream = File.OpenRead(combinedImagePath);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            stream.Close();

            return image;
        }

        /// <summary>
        /// Сохранение изображения в файл
        /// </summary>
        /// <param name="human"></param>
        /// <param name="humanImage"></param>
        private void SaveImageToFile(BitmapSource humanImage, Human human)
        {
            string combinedImagePath = Path.Combine(_imageFolder, human.ImageFile);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(humanImage));

            if (File.Exists(combinedImagePath))
            {
                File.Delete(combinedImagePath);
            }

            using FileStream fs = new FileStream(combinedImagePath, FileMode.Create);
            encoder.Save(fs);
        }

        /// <summary>
        /// Удаление файла изображения
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool DeleteImageFile(string fileName)
        {
            bool successResult = true;

            try
            {
                string combinedImagePath = Path.Combine(_imageFolder, fileName);
                File.Delete(combinedImagePath);
            }
            catch (Exception ex)
            {
                successResult = false;
                _logger.Error($"Ошибка удаления файла изображения: {ex.HResult}");
            }

            return successResult;
        }

        /// <summary>
        /// Формирование иерархической коллекции
        /// </summary>
        /// <param name="reasons">Плоский список</param>
        /// <param name="headReason">Головной элемент (экземпляр класса)</param>
        public void FormObservableCollection(List<Reason> reasons, Reason headReason)
        {
            foreach (var reason in reasons)
            {
                if (reason.ReasonParentId == Guid.Empty) // Случай корневых узлов
                {
                    Reason rsn = new()
                    {
                        ReasonParentId = reason.ReasonParentId,
                        ReasonId = reason.ReasonId,
                        ReasonName = reason.ReasonName,
                        ReasonComment = reason.ReasonComment,
                        ReasonDescription = reason.ReasonDescription
                    };
                    ReasonsCollection.Add(rsn);

                    // Проверка на наличие дочерних узлов
                    var daughters = PlainReasonsList.FindAll(x => x.ReasonParentId == rsn.ReasonId);
                    if (daughters.Count != 0) // Если дочерние узлы найдены
                    {
                        FormObservableCollection(daughters, rsn); // Вызываем рекурсивно
                    }
                }
                else if (headReason != null)// Случай вложенных узлов
                {
                    Reason rsn = new()
                    {
                        ReasonId = reason.ReasonId,
                        ReasonName = reason.ReasonName,
                        ReasonComment = reason.ReasonComment,
                        ReasonDescription = reason.ReasonDescription,
                        ReasonParentId = headReason.ReasonId,
                        ReasonParent = headReason
                    };
                    headReason.ReasonChildren.Add(rsn);

                    // Проверка на наличие дочерних узлов
                    var daughters = PlainReasonsList.FindAll(x => x.ReasonParentId == rsn.ReasonId);
                    if (daughters.Count != 0) // Если дочерние узлы найдены
                    {
                        FormObservableCollection(daughters, rsn); // Вызываем рекурсивно
                    }
                }
            }
        }

        /// <summary>
        /// Сортировка по возрастанию стартового возраста
        /// </summary>
        public static void RearrangeCollection()
        {
            var rearrangeCollection = AgeCategories.OrderBy(x => x.StartAge).ToList();
            AgeCategories.Clear();
            foreach (var item in rearrangeCollection)
            {
                AgeCategories.Add(item);
            }
        }

        /// <summary>
        /// Формирование текстов для отображения прожитых периодов (лет, месяцев, дней, часов, минут, секунд) в соответствии с числом
        /// </summary>
        /// <param name="i"></param>
        /// <param name="type"></param>
        public string GetFinalText(int i, ScopeTypes type)
        {
            var periodValues = Scopes.GetPeriodValues(type);
            string result = "";
            var t1 = i % 10;
            var t2 = i % 100;
            if (t1 == 1 && t2 != 11)
            {
                result = periodValues[0];
            }
            else if (t1 >= 2 && t1 <= 4 && (t2 < 10 || t2 >= 20))
            {
                result = periodValues[1];
            }
            else
            {
                result = periodValues[2];
            }

            return result;
        }

        /// <summary>
        /// Получение количества лет и дней за период
        /// В этом методе учитываются високосные годы
        /// С учетом дат рождения и смерти - попадание на период с 29 февраля
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public Tuple<int, int> GetYearsAndDaysConsideredLeaps(DateTime start, DateTime stop)
        {
            var age = stop.Year - start.Year;
            if (start > stop.AddYears(-age)) age--;
            var days = stop.Subtract(start).Days - (age * 365);
            for (int year = start.Year; year <= stop.Year; year++)
            {
                if (year == start.Year)
                {
                    if (DateTime.IsLeapYear(start.Year) && (start.Date < DateTime.Parse($"01.03.{start.Year}"))) days--;
                }
                if ((year != start.Year) && (year != stop.Year))
                {
                    if (DateTime.IsLeapYear(year)) days--;
                }
                if (year == stop.Year)
                {
                    if (DateTime.IsLeapYear(year) && (stop.Date > DateTime.Parse($"28.02.{stop.Year}"))) days--;
                }
            }
            return new Tuple<int, int>(age, days);
        }









        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="msSqlController"></param>
        /// <param name="xmlController"></param>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommonDataController(IXmlController xmlController,
                                    ILogger logger,
                                    IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xmlController = xmlController ?? throw new ArgumentNullException(nameof(xmlController));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        #endregion
    }
}
