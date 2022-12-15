﻿using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using AutoMapper;
using MemoRandom.Data.DbModels;
using MemoRandom.Client.Common.Models;
using System.Windows.Media;
using Human = MemoRandom.Client.Common.Models.Human;
using System.IO;
using System.Windows.Media.Imaging;
using NLog;
using MemoRandom.Client.Common.Enums;
using System.Windows.Controls;
using MemoRandom.Data.DtoModels;
using System.Configuration;

namespace MemoRandom.Client.Common.Implementations
{
    public class CommonDataController : ICommonDataController
    {
        #region PRIVATE FIELDS
        private readonly ILogger _logger;
        private readonly IMsSqlController _msSqlController;
        private readonly IXmlController _xmlController;
        private readonly IMapper _mapper;

        private string _reasonsFilePath;
        private string _categoriesFilePath;
        private string _comparedHumansFilePath;
        private string _humansFilePath;
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
        public void SetFilesPaths()
        {
            // Получаем папку, где установлено приложение и добавляем папку хранения XML-файлов
            var xmlFolder = AppDomain.CurrentDomain.BaseDirectory + @"\Data";
            // Проверяем, существует ли папка, где хранятся данные
            if (!Directory.Exists(xmlFolder))
            {
                Directory.CreateDirectory(xmlFolder); // Если не существует, то создаем
            }

            _reasonsFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["ReasonsFile"]);

            _categoriesFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["CategoriesFile"]);

            _comparedHumansFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["ComparedHumansFile"]);

            _humansFilePath = Path.Combine(xmlFolder, ConfigurationManager.AppSettings["HumansFile"]);
        }

        /// <summary>
        /// Временно - потом все преобразовать
        /// </summary>
        public void SaveXmlData()
        {
            #region Для причин
            List<DtoReason> dtoReasons = _mapper.Map<List<Reason>, List<DtoReason>>(PlainReasonsList);

            // Вызов метода сохранения справочника (плоский)
            _xmlController.SaveReasonsToFile(dtoReasons, _reasonsFilePath);
            #endregion

            #region Для категорий
            List<DtoCategory> dtoCategories = _mapper.Map<ObservableCollection<Category>, List<DtoCategory>>(AgeCategories);

            _xmlController.SaveCategoriesToFile(dtoCategories, _categoriesFilePath);
            #endregion

            #region Для людей для сравнения
            List<DtoComparedHuman> dtoComparedHumans = _mapper.Map<ObservableCollection<ComparedHuman>, List<DtoComparedHuman>>(ComparedHumansCollection);

            _xmlController.SaveComparedHumansToFile(dtoComparedHumans, _comparedHumansFilePath);
            #endregion

            #region Для людей
            List<DtoHuman> dtoHumans = _mapper.Map<ObservableCollection<Human>, List<DtoHuman>>(HumansList);

            _xmlController.SaveHumansToFile(dtoHumans, _humansFilePath);
            #endregion
        }

        /// <summary>
        /// Временно - читаем все из файла
        /// </summary>
        /// <param name="filePath"></param>
        public void ReadXmlData()
        {
            #region Чтение причин смерти из файла - потом структуру переделать
            PlainReasonsList.Clear(); // Чистим плоский список
            ReasonsCollection.Clear(); // Чистим иерархическую коллекцию

            var reasonsResult = _xmlController.ReadReasonsFromFile(_reasonsFilePath);
            // Преобразование через маппер
            PlainReasonsList = _mapper.Map<List<DtoReason>, List<Reason>>(reasonsResult);
            FormObservableCollection(PlainReasonsList, null); // Формируем иерархическую коллекцию
            #endregion

            #region Чтение возрастных категорий
            AgeCategories.Clear();
            var categoriesResult = _xmlController.ReadCategoriesFromFile(_categoriesFilePath);
            AgeCategories = _mapper.Map<List<DtoCategory>, ObservableCollection<Category>>(categoriesResult);
            foreach (var item in AgeCategories) // Преобразование строк в цвет
            {
                item.CategoryColor = (Color)ColorConverter.ConvertFromString(item.StringColor)!;
            }
            #endregion

            #region Чтение людей для сравнения
            ComparedHumansCollection.Clear();
            var comparedHumansResult = _xmlController.ReadComparedHumansFromFile(_comparedHumansFilePath);
            ComparedHumansCollection = _mapper.Map<List<DtoComparedHuman>, ObservableCollection<ComparedHuman>>(comparedHumansResult);
            #endregion

            #region Чтение людей
            HumansList.Clear();
            var humansResult = _xmlController.ReadHumansFromFile(_humansFilePath);
            HumansList = _mapper.Map<List<DtoHuman>, ObservableCollection<Human>>(humansResult);
            #endregion
        }

        public void AddReasonToFile(Reason rsn)
        {
            DtoReason dtoReason = new()
            {
                ReasonId = rsn.ReasonId,
                ReasonName = rsn.ReasonName,
                ReasonComment = rsn.ReasonComment,
                ReasonDescription = rsn.ReasonDescription,
                ReasonParentId = rsn.ReasonParentId
            };

            _xmlController.AddReasonToList(dtoReason, _reasonsFilePath);
        }

        public void ChangeReason(Reason reason)
        {
            DtoReason dtoReason = new()
            {
                ReasonId = reason.ReasonId,
                ReasonName = reason.ReasonName,
                ReasonComment = reason.ReasonComment,
                ReasonDescription = reason.ReasonDescription,
                ReasonParentId = reason.ReasonParentId
            };

            _xmlController.ChangeReasonInFile(dtoReason, _reasonsFilePath);
        }

        public void DeleteReason(Guid id)
        {
            _xmlController.DeleteReasonInFile(id.ToString(), _reasonsFilePath);
        }


        /// <summary>
        /// Чтение общей информации из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public bool ReadDataFromRepository()
        {
            bool successResult = true;

            #region Чтение причин смерти и формирование плоского и иерархического списков
            PlainReasonsList = _mapper.Map<List<DbReason>, List<Reason>>(_msSqlController.GetReasons());
            FormObservableCollection(PlainReasonsList, null);
            #endregion

            #region Чтение списка категорий
            AgeCategories = _mapper.Map<List<DbCategory>, ObservableCollection<Category>>(_msSqlController.GetCategories());
            foreach (var item in AgeCategories) // Преобразование строк в цвет
            {
                item.CategoryColor = (Color)ColorConverter.ConvertFromString(item.StringColor)!;
            }
            #endregion

            #region Чтение списка людей для сравнения
            ComparedHumansCollection = _mapper.Map<List<DbComparedHuman>, ObservableCollection<ComparedHuman>>(_msSqlController.GetComparedHumans());
            #endregion

            #region Чтение списка людей
            HumansList = _mapper.Map<List<DbHuman>, ObservableCollection<Human>>(_msSqlController.GetHumans());
            #endregion

            return successResult;
        }

        /// <summary>
        /// Обновление (добавление) категории во внешнее хранилище
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool UpdateCategoriesInRepository(Category category)
        {
            DbCategory dbCategory = _mapper.Map<DbCategory>(category);
            return _msSqlController.UpdateCategories(dbCategory);
        }

        /// <summary>
        /// Удаление выбранной категории во внешнем хранилище
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool DeleteCategoryInRepository(Category category)
        {
            return _msSqlController.DeleteCategory(category.CategoryId);
        }

        /// <summary>
        /// Обновление иерархической коллекции причин смерти
        /// </summary>
        public void UpdateHierarchicalReasonsData()
        {
            ReasonsCollection.Clear();
            FormObservableCollection(PlainReasonsList, null);
        }

        /// <summary>
        /// Обновление (добавление) человека для сравнения во внешнем хранилище
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <returns></returns>
        public bool UpdateComparedHumanInRepository(ComparedHuman comparedHuman)
        {
            DbComparedHuman dbComparedHuman = _mapper.Map<DbComparedHuman>(comparedHuman);
            return _msSqlController.UpdateComparedHuman(dbComparedHuman);
        }

        /// <summary>
        /// Удаление человека для сравнения во внешнем хранилище
        /// </summary>
        /// <returns></returns>
        public bool DeleteComparedHumanInRepository(ComparedHuman comparedHuman)
        {
            return _msSqlController.DeleteComparedHuman(comparedHuman.ComparedHumanId);
        }

        /// <summary>
        /// Обновление (добавление) человека во внешнем хранилище
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool UpdateHumanInRepository(Human human, BitmapImage humanImage)
        {
            DbHuman updatedHuman = _mapper.Map<DbHuman>(human);

            _msSqlController.UpdateHumans(updatedHuman);

            if (humanImage != null)
            {
                SaveImageToFile(humanImage, human); // Сохраняем изображение
            }

            return true;
        }

        /// <summary>
        /// Удаление человека из внешнего хранилища
        /// </summary>
        /// <param name="human"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        public bool DeleteHumanInRepository(Human human, string imageFile)
        {
            bool success = _msSqlController.DeleteHuman(human.HumanId);
            if (imageFile != string.Empty)
            {
                if (!DeleteImageFile(imageFile))
                {
                    success = false; // Если файл изображения удалить не удалось
                }
            }
            return success;
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

            string combinedImagePath = Path.Combine(_msSqlController.GetImageFolder(), currentHuman.ImageFile);
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
            string combinedImagePath = Path.Combine(_msSqlController.GetImageFolder(), human.ImageFile);

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
                string combinedImagePath = Path.Combine(_msSqlController.GetImageFolder(), fileName);
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
        private void FormObservableCollection(List<Reason> reasons, Reason headReason)
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
            List<Category> rearrangeCollection = new();
            rearrangeCollection = AgeCategories.OrderBy(x => x.StartAge).ToList();
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
        public string GetFinalText(int i, ScopeTypes type)
        {
            var periodValues = Scopes.GetPeriodValues(type);
            string result = "";
            int t1, t2;
            t1 = i % 10;
            t2 = i % 100;
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
        public CommonDataController(IMsSqlController msSqlController,
                                    IXmlController xmlController,
                                    ILogger logger,
                                    IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));
            _xmlController = xmlController ?? throw new ArgumentNullException(nameof(xmlController));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        #endregion
    }
}
