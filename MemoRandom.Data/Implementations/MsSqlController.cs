using System;
using System.Collections.Generic;
using System.Linq;
using MemoRandom.Models.Models;
using MemoRandom.Data.Controllers;
using MemoRandom.Data.DbModels;
using MemoRandom.Data.Interfaces;
using NLog;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace MemoRandom.Data.Implementations
{
    public class MsSqlController : IMsSqlController
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Строка соединения с базой данных
        /// </summary>
        public static string DbConnectionString { get; set; }

        /// <summary>
        /// Путь к папке хранения изображений
        /// </summary>
        public static string ImageFolder { get; set; }

        /// <summary>
        /// Контекст базы данных
        /// </summary>
        public static MemoRandomDbContext MemoContext { get; set; }

        /// <summary>
        /// Плоский список причин смерти для сериализации
        /// </summary>
        private List<DbReason> PlainReasonsList { get; set; } = new();

        #region Общий блок работы с БД и изображениями
        /// <summary>
        /// Установка путей сохранения и сроки подключения к БД
        /// </summary>
        /// <param name="fileName">Имя файла базы данных</param>
        /// <param name="filePath">Имя папка, где расположена БД</param>
        /// <param name="imagesFilePath">Имя папки, где хранятся изображения</param>
        /// <param name="serverName">Название сервера</param>
        /// <returns></returns>
        public bool SetPaths(string fileName, string filePath, string imagesFilePath, string serverName)
        {
            bool success = true;
            try
            {
                // Проверяем, существует ли папка хранения БД - только для случая генерации БД
                var dbBaseDirectory = AppDomain.CurrentDomain.BaseDirectory + filePath;
                if (!Directory.Exists(dbBaseDirectory))
                {
                    Directory.CreateDirectory(dbBaseDirectory);
                }

                // Проверяем, существует ли папка хранения изображений
                ImageFolder = AppDomain.CurrentDomain.BaseDirectory + imagesFilePath;
                if (!Directory.Exists(ImageFolder))
                {
                    Directory.CreateDirectory(ImageFolder);
                }

                string combinedPath = Path.Combine(dbBaseDirectory, fileName);
                SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = serverName,
                    AttachDBFilename = combinedPath,
                    InitialCatalog = Path.GetFileNameWithoutExtension(combinedPath),
                    IntegratedSecurity = true
                };
                DbConnectionString = connectionStringBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                success = false;
                _logger.Error($"Ошибка установки стартовых путей и строки подкючения: {ex.HResult}");
            }
            
            return success;
        }
        #endregion

        #region Блок справочника причин смерти
        /// <summary>
        /// Получение справочника причин смерти
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<Reason> GetReasons()
        {
            List<Reason> reasons = new();
            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    PlainReasonsList = MemoContext.DbReasons.ToList(); // Читаем контекст базы данных
                    reasons = FormPlainReasonsList(PlainReasonsList);
                }
                catch (Exception ex)
                {
                    reasons = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
                    _logger.Error($"Ошибка чтения файла настроек: {ex.HResult}");
                }
            }

            return reasons;
        }

        /// <summary>
        /// Добавление причины в общий список в БД
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool AddReasonToList(Reason reason)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    DbReason record = new DbReason()
                    {
                        DbReasonId          = reason.ReasonId,
                        DbReasonName        = reason.ReasonName,
                        DbReasonComment     = reason.ReasonComment,
                        DbReasonDescription = reason.ReasonDescription,
                        DbReasonParentId    = reason.ReasonParentId
                    };
                    MemoContext.DbReasons.Add(record);
                    MemoContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка записи файла справочника: {ex.HResult}");
                }
            }

            return successResult;
        }

        /// <summary>
        /// Обновление измененных данных причины смерти
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool UpdateReasonInList(Reason reason)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var updatedReason = MemoContext.DbReasons.FirstOrDefault(x => x.DbReasonId == reason.ReasonId);
                    if (updatedReason != null)
                    {
                        updatedReason.DbReasonName        = reason.ReasonName;
                        updatedReason.DbReasonComment     = reason.ReasonComment;
                        updatedReason.DbReasonDescription = reason.ReasonDescription;
                        updatedReason.DbReasonParentId    = reason.ReasonParentId;

                        MemoContext.SaveChanges();
                    }
                    else
                    {
                        successResult = false; // На случай, если не найдена обновляемая причина в отслеживаемом наборе
                    }
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка записи файла справочника: {ex.HResult}");
                }
            }

            return successResult;
        }

        /// <summary>
        /// Удаление причины смерти и всех ее дочерних узлов
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool DeleteReasonInList(Reason reason)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    //TODO Здесь как-то проверить, привязана ли причина к тому или иному человеку
                    DeletingDaughters(reason, MemoContext);

                    MemoContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка записи файла справочника: {ex.HResult}");
                }
            }

            return successResult;
        }
        #endregion

        #region <Блок работы с людьми
        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Human> GetHumans()
        {
            ObservableCollection<Human> humansList = new();

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    // Читаем контекст, выбирая только основные поля (без изображений)
                    var newList = MemoContext.DbHumans.Select(h => new
                    {
                        h.DbHumanId,
                        h.DbLastName,
                        h.DbFirstName,
                        h.DbPatronymic,
                        h.DbBirthDate,
                        h.DbBirthCountry,
                        h.DbBirthPlace,
                        h.DbDeathDate,
                        h.DbDeathCountry,
                        h.DbDeathPlace,
                        h.DbImageFile,
                        h.DbDeathReasonId,
                        h.DbHumanComments,
                        h.DbDaysLived,
                        h.DbFullYearsLived
                    }).OrderBy(x => x.DbFullYearsLived);

                    // Перегоняем в результирующий список
                    foreach (var person in newList)
                    {
                        Human human = new()
                        {
                            HumanId        = person.DbHumanId,
                            LastName       = person.DbLastName,
                            FirstName      = person.DbFirstName,
                            Patronymic     = person.DbPatronymic,
                            BirthDate      = person.DbBirthDate,
                            BirthCountry   = person.DbBirthCountry,
                            BirthPlace     = person.DbBirthPlace,
                            DeathDate      = person.DbDeathDate,
                            DeathCountry   = person.DbDeathCountry,
                            DeathPlace     = person.DbDeathPlace,
                            ImageFile      = person.DbImageFile,
                            DeathReasonId  = person.DbDeathReasonId,
                            HumanComments  = person.DbHumanComments,
                            DaysLived      = person.DbDaysLived,
                            FullYearsLived = person.DbFullYearsLived
                        };
                        humansList.Add(human);
                    }
                }
                catch (Exception ex)
                {
                    humansList = null; // В случае неуспеха чтения обнуляем список людей
                    _logger.Error($"Ошибка чтения файла по людям: {ex.HResult}");
                }
            }
            return humansList;
        }

        /// <summary>
        /// Обновление сущности человека в общем списке
        /// </summary>
        /// <param name="human"></param>
        /// <param name="humanImage"></param>
        /// <returns></returns>
        public bool UpdateHumans(Human human, BitmapImage humanImage)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var updatedHuman = MemoContext.DbHumans.FirstOrDefault(x => x.DbHumanId == human.HumanId);
                    if (updatedHuman != null) // Корректировка информации
                    {
                        updatedHuman.DbLastName       = human.LastName;
                        updatedHuman.DbFirstName      = human.FirstName;
                        updatedHuman.DbPatronymic     = human.Patronymic;
                        updatedHuman.DbBirthDate      = human.BirthDate;
                        updatedHuman.DbBirthCountry   = human.BirthCountry;
                        updatedHuman.DbBirthPlace     = human.BirthPlace;
                        updatedHuman.DbDeathDate      = human.DeathDate;
                        updatedHuman.DbDeathCountry   = human.DeathCountry;
                        updatedHuman.DbDeathPlace     = human.DeathPlace;
                        updatedHuman.DbDeathReasonId  = human.DeathReasonId;
                        updatedHuman.DbImageFile      = human.ImageFile;
                        updatedHuman.DbHumanComments  = human.HumanComments;
                        updatedHuman.DbDaysLived      = human.DaysLived;
                        updatedHuman.DbFullYearsLived = human.FullYearsLived;

                        MemoContext.SaveChanges();

                        if(humanImage != null)
                        {
                            SaveImageToFile(human, humanImage); // Сохраняем изображение
                        }
                    }
                    else // Добавление новой записи
                    {
                        DbHuman record = new()
                        {
                            DbHumanId        = human.HumanId,
                            DbLastName       = human.LastName,
                            DbFirstName      = human.FirstName,
                            DbPatronymic     = human.Patronymic,
                            DbBirthDate      = human.BirthDate,
                            DbBirthCountry   = human.BirthCountry,
                            DbBirthPlace     = human.BirthPlace,
                            DbDeathDate      = human.DeathDate,
                            DbDeathCountry   = human.DeathCountry,
                            DbDeathPlace     = human.DeathPlace,
                            DbDeathReasonId  = human.DeathReasonId,
                            DbImageFile      = human.ImageFile,
                            DbHumanComments  = human.HumanComments,
                            DbDaysLived      = human.DaysLived,
                            DbFullYearsLived = human.FullYearsLived
                        };

                        MemoContext.DbHumans.Add(record);
                        MemoContext.SaveChanges();
                    }

                    if(humanImage != null)
                    {
                        SaveImageToFile(human, humanImage); // Сохраняем изображение
                    }
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка записи в список людей: {ex.HResult}");
                }
            }
            return successResult;
        }

        /// <summary>
        /// Удаление человека из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public bool DeleteHuman(Human currentHuman)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var deletedHuman = MemoContext.DbHumans.FirstOrDefault(x => x.DbHumanId == currentHuman.HumanId);
                    if (deletedHuman != null)
                    {
                        MemoContext.Remove(deletedHuman);
                        MemoContext.SaveChanges();
                    }

                    if (currentHuman.ImageFile != string.Empty)
                    {
                        if (!DeleteImageFile(currentHuman.ImageFile))
                        {
                            successResult = false; // Если файл изображения удалить не удалось
                        }
                    }
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка удаления человека: {ex.HResult}");
                }
            }

            return successResult;
        }

        /// <summary>
        /// Получение изображения выбранного человека
        /// </summary>
        public BitmapImage GetHumanImage(Human currentHuman)
        {
            // Читаем файл изображения, если выбранный человек существует и у него есть изображение
            if (currentHuman != null && currentHuman.ImageFile != String.Empty)
            {
                string combinedImagePath = Path.Combine(ImageFolder, currentHuman.ImageFile);

                using Stream stream = File.OpenRead(combinedImagePath);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                stream.Close();

                return image;
            }

            return null;
        }

        /// <summary>
        /// Сохранение изображения в файл
        /// </summary>
        /// <param name="human"></param>
        private void SaveImageToFile(Human human, BitmapImage humanImage)
        {
            string combinedImagePath = Path.Combine(ImageFolder, human.ImageFile);

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
                string combinedImagePath = Path.Combine(ImageFolder, fileName);
                File.Delete(combinedImagePath);
            }
            catch (Exception ex)
            {
                successResult = false;
                _logger.Error($"Ошибка удаления файла изображения: {ex.HResult}");
            }

            return successResult;
        }
        #endregion

        #region Auxiliary methods
        /// <summary>
        /// Формирование плоского списка справочника причин смерти
        /// </summary>
        /// <param name="reasons"></param>
        /// <returns></returns>
        private List<Reason> FormPlainReasonsList(List<DbReason> reasons)
        {
            List<Reason> plainReasonsList = new();

            foreach (var reason in reasons)
            {
                Reason rsn = new()
                {
                    ReasonId = reason.DbReasonId,
                    ReasonName = reason.DbReasonName,
                    ReasonComment = reason.DbReasonComment,
                    ReasonDescription = reason.DbReasonDescription,
                    ReasonParentId = reason.DbReasonParentId
                };
                plainReasonsList.Add(rsn);
            }

            return plainReasonsList;
        }

        /// <summary>
        /// Рекурсивный метод удаления дочерних узлов для удаляемой причины смерти
        /// </summary>
        private void DeletingDaughters(Reason reason, MemoRandomDbContext context)
        {
            var deletedReason = context.DbReasons.FirstOrDefault(x => x.DbReasonId == reason.ReasonId);
            if (deletedReason != null)
            {
                context.Remove(deletedReason);

                foreach (var child in reason.ReasonChildren) // Если есть дочерние узлы, то выполняем удаление и по ним
                {
                    DeletingDaughters(child, context);
                }
            }
        }
        #endregion

        #region Блок работы с категориями
        /// <summary>
        /// Получение списка категорий из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Category> GetCategories()
        {
            ObservableCollection<Category> categories = new();
            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    List<DbCategory> categoriesList = MemoContext.DbCategories.OrderBy(x => x.DbPeriodFrom).ToList(); // Читаем контекст базы данных
                    foreach (var category in categoriesList)
                    {
                        //var x = System.Windows.Media.Color.FromArgb(category.DbColorA, category.DbColorR, category.DbColorG, category.DbColorB);
                        Category cat = new()
                        {
                            CategoryId = category.DbCategoryId,
                            CategoryName = category.DbCategoryName,
                            StartAge = category.DbPeriodFrom,
                            StopAge = category.DbPeriodTo,
                            CategoryColor = System.Windows.Media.Color.FromArgb(category.DbColorA, category.DbColorR, category.DbColorG, category.DbColorB)
                        };
                        categories.Add(cat);
                    }
                }
                catch (Exception ex)
                {
                    categories = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
                    _logger.Error($"Ошибка чтения категорий: {ex.HResult}");
                }
            }

            return categories;
        }

        /// <summary>
        /// Обновление (добавление или редактирование) категорий
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool UpdateCategories(Category category)
        {
            var success = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var updatedCategory = MemoContext.DbCategories.FirstOrDefault(x => x.DbCategoryId == category.CategoryId);
                    if (updatedCategory != null) // Корректировка информации
                    {
                        updatedCategory.DbCategoryId = category.CategoryId;
                        updatedCategory.DbCategoryName = category.CategoryName;
                        updatedCategory.DbPeriodFrom   = category.StartAge;
                        updatedCategory.DbPeriodTo     = category.StopAge;
                        updatedCategory.DbColorA       = category.CategoryColor.A;
                        updatedCategory.DbColorR       = category.CategoryColor.R;
                        updatedCategory.DbColorG       = category.CategoryColor.G;
                        updatedCategory.DbColorB       = category.CategoryColor.B;

                        MemoContext.SaveChanges();
                    }
                    else // Добавление новой записи в таблицу категорий
                    {
                        DbCategory record = new()
                        {
                            DbCategoryId = category.CategoryId,
                            DbCategoryName = category.CategoryName,
                            DbPeriodFrom = category.StartAge,
                            DbPeriodTo = category.StopAge,
                            DbColorA = category.CategoryColor.A,
                            DbColorR = category.CategoryColor.R,
                            DbColorG = category.CategoryColor.G,
                            DbColorB = category.CategoryColor.B

                        };

                        MemoContext.DbCategories.Add(record);
                        MemoContext.SaveChanges();
                    };
                }
                catch (Exception ex)
                {
                    success = false;
                    _logger.Error($"Ошибка обновления категории: {ex.HResult}");
                }
            }
            return success;
        }

        /// <summary>
        /// Удаление категории
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool DeleteCategory(Category category)
        {
            var success = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var deletedCategory = MemoContext.DbCategories.FirstOrDefault(x => x.DbCategoryId == category.CategoryId);

                    if (deletedCategory != null)
                    {
                        MemoContext.Remove(deletedCategory);
                        MemoContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    _logger.Error($"Ошибка удаления категории: {ex.HResult}");
                }
            }
                
            return success;
        }
        #endregion












        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        public MsSqlController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion
    }
}
