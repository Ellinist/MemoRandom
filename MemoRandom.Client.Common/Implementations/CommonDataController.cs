using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using MemoRandom.Data.DbModels;
using MemoRandom.Client.Common.Models;
using System.Windows.Media;
using Human = MemoRandom.Client.Common.Models.Human;
using System.IO;
using System.Windows.Media.Imaging;

namespace MemoRandom.Client.Common.Implementations
{
    public class CommonDataController : ICommonDataController
    {
        #region PRIVATE FIELDS
        private readonly IMsSqlController _msSqlController;
        private readonly IMapper _mapper;
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
        /// Список людей
        /// </summary>
        public static ObservableCollection<Human> HumansList { get; set; } = new();

        /// <summary>
        /// Текущий выбор человека
        /// </summary>
        public static Human CurrentHuman { get; set; }
        #endregion

        #region IMPLEMENTATION
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
            AgeCategories = ConvertCategoriesFromDbSet(_msSqlController.GetCategories());
            //AgeCategories = _mapper.Map<List<DbCategory>, ObservableCollection<Category>>(_msSqlController.GetCategories());
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
        /// Получение списка категорий
        /// </summary>
        /// <param name="categoriesList"></param>
        /// <returns></returns>
        private ObservableCollection<Category> ConvertCategoriesFromDbSet(List<DbCategory> categoriesList)
        {
            ObservableCollection<Category> categories = new();
            foreach (var cat in categoriesList)
            {
                Category category = new();
                category = _mapper.Map<Category>(cat);
                category.CategoryColor = Color.FromArgb(cat.ColorA, cat.ColorR, cat.ColorG, cat.ColorB);
                categories.Add(category);
            }

            return categories;
        }

        /// <summary>
        /// Обновление иерархической коллекции причин смерти
        /// </summary>
        public void UpdateHierarchicalReasonsData()
        {
            ReasonsCollection.Clear();
            FormObservableCollection(PlainReasonsList, null);
        }

        public bool UpdateHumanData(Human human, BitmapImage humanImage)
        {
            DbHuman updatedHuman = new()
            {
                HumanId = human.HumanId,
                LastName = human.LastName,
                FirstName = human.FirstName,
                Patronymic = human.Patronymic,
                BirthDate = human.BirthDate,
                BirthCountry = human.BirthCountry,
                BirthPlace = human.BirthPlace,
                DeathDate = human.DeathDate,
                DeathCountry = human.DeathCountry,
                DeathPlace = human.DeathPlace,
                DeathReasonId = human.DeathReasonId,
                ImageFile = human.ImageFile,
                HumanComments = human.HumanComments,
                DaysLived = human.DaysLived,
                FullYearsLived = human.FullYearsLived
            };

            _msSqlController.UpdateHumans(updatedHuman);

            if (humanImage != null)
            {
                SaveImageToFile(humanImage, human); // Сохраняем изображение
            }

            return true;
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



        #region CTOR
        public CommonDataController(IMsSqlController msSqlController,
                                    IMapper mapper)
        {
            _msSqlController = msSqlController ?? throw new ArgumentNullException(nameof(msSqlController));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        #endregion
    }
}
