using MemoRandom.Data.Controllers;
using MemoRandom.Data.DbModels;
using MemoRandom.Data.Interfaces;
using Microsoft.Data.SqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MemoRandom.Data.Implementations
{
    public class MsSqlController : IMsSqlController
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Строка соединения с базой данных
        /// </summary>
        public string DbConnectionString { get; set; }

        /// <summary>
        /// Путь к папке хранения изображений
        /// </summary>
        public string ImageFolder { get; set; }

        /// <summary>
        /// Контекст базы данных
        /// </summary>
        public MemoRandomDbContext MemoContext { get; set; }

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
        public List<DbReason> GetReasons()
        {
            List<DbReason> reasons = new();
            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    reasons = MemoContext.DbReasons.ToList(); // Читаем контекст базы данных
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
        public bool AddReasonToList(DbReason reason)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    //DbReason record = new DbReason()
                    //{
                    //    ReasonId          = reason.ReasonId,
                    //    ReasonName        = reason.ReasonName,
                    //    ReasonComment     = reason.ReasonComment,
                    //    ReasonDescription = reason.ReasonDescription,
                    //    ReasonParentId    = reason.ReasonParentId
                    //};
                    MemoContext.DbReasons.Add(reason);
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
        public bool UpdateReasonInList(DbReason reason)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var updatedReason = MemoContext.DbReasons.FirstOrDefault(x => x.ReasonId == reason.ReasonId);
                    if (updatedReason != null)
                    {
                        updatedReason.ReasonName        = reason.ReasonName;
                        updatedReason.ReasonComment     = reason.ReasonComment;
                        updatedReason.ReasonDescription = reason.ReasonDescription;
                        updatedReason.ReasonParentId    = reason.ReasonParentId;

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
        public bool DeleteReasonInList(List<Guid> deletedList)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    //TODO Здесь как-то проверить, привязана ли причина к тому или иному человеку
                    foreach(var item in deletedList) // Бежим по списку идентификаторов
                    {
                        var record = MemoContext.DbReasons.FirstOrDefault(x => x.ReasonId == item);
                        MemoContext.Remove(record); // Удаляем запись с таким ID
                    }
                    
                    MemoContext.SaveChanges(); // Сохраняем изменения
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка удаления в файле справочника: {ex.HResult}");
                }
            }

            return successResult;
        }
        #endregion

        #region Блок работы с категориями
        /// <summary>
        /// Получение списка категорий из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public List<DbCategory> GetCategories()
        {
            List<DbCategory> categories = new();
            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    categories = MemoContext.DbCategories.OrderBy(x => x.StartAge).ToList(); // Читаем контекст базы данных
                }
                catch (Exception ex)
                {
                    categories = null; // В случае ошибки чтения обнуляем иерархическую коллекцию
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
        public bool UpdateCategories(DbCategory category)
        {
            var success = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var updatedCategory = MemoContext.DbCategories.FirstOrDefault(x => x.CategoryId == category.CategoryId);

                    if (updatedCategory != null) // Корректировка информации
                    {
                        updatedCategory.CategoryId   = category.CategoryId;
                        updatedCategory.CategoryName = category.CategoryName;
                        updatedCategory.StartAge     = category.StartAge;
                        updatedCategory.StopAge      = category.StopAge;
                        updatedCategory.ColorA       = category.ColorA;
                        updatedCategory.ColorR       = category.ColorR;
                        updatedCategory.ColorG       = category.ColorG;
                        updatedCategory.ColorB       = category.ColorB;

                        MemoContext.SaveChanges();
                    }
                    else // Добавление новой записи в таблицу категорий
                    {
                        DbCategory record = new()
                        {
                            CategoryId   = category.CategoryId,
                            CategoryName = category.CategoryName,
                            StartAge     = category.StartAge,
                            StopAge      = category.StopAge,
                            ColorA       = category.ColorA,
                            ColorR       = category.ColorR,
                            ColorG       = category.ColorG,
                            ColorB       = category.ColorB

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
        public bool DeleteCategory(Guid categoryId)
        {
            var success = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var deletedCategory = MemoContext.DbCategories.FirstOrDefault(x => x.CategoryId == categoryId);

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

        #region <Блок работы с людьми
        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public List<DbHuman> GetHumans()
        {
            List<DbHuman> humansList = new();

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    // Читаем контекст, выбирая только основные поля (без изображений)
                    humansList = MemoContext.DbHumans.OrderBy(x => x.FullYearsLived).ToList();
                    //var newList = MemoContext.DbHumans.Select(h => new
                    //{
                    //    h.HumanId,
                    //    h.LastName,
                    //    h.FirstName,
                    //    h.Patronymic,
                    //    h.BirthDate,
                    //    h.BirthCountry,
                    //    h.BirthPlace,
                    //    h.DeathDate,
                    //    h.DeathCountry,
                    //    h.DeathPlace,
                    //    h.ImageFile,
                    //    h.DeathReasonId,
                    //    h.HumanComments,
                    //    h.DaysLived,
                    //    h.FullYearsLived
                    //}).OrderBy(x => x.FullYearsLived);

                    //// Перегоняем в результирующий список
                    //foreach (var person in newList)
                    //{
                    //    DbHuman human = new()
                    //    {
                    //        HumanId        = person.HumanId,
                    //        LastName       = person.LastName,
                    //        FirstName      = person.FirstName,
                    //        Patronymic     = person.Patronymic,
                    //        BirthDate      = person.BirthDate,
                    //        BirthCountry   = person.BirthCountry,
                    //        BirthPlace     = person.BirthPlace,
                    //        DeathDate      = person.DeathDate,
                    //        DeathCountry   = person.DeathCountry,
                    //        DeathPlace     = person.DeathPlace,
                    //        ImageFile      = person.ImageFile,
                    //        DeathReasonId  = person.DeathReasonId,
                    //        HumanComments  = person.HumanComments,
                    //        DaysLived      = person.DaysLived,
                    //        FullYearsLived = person.FullYearsLived
                    //    };
                    //    humansList.Add(human);
                    //}
                }
                catch (Exception ex)
                {
                    humansList = null; // В случае ошибки чтения обнуляем список людей
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
        public bool UpdateHumans(DbHuman human)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var updatedHuman = MemoContext.DbHumans.FirstOrDefault(x => x.HumanId == human.HumanId);
                    if (updatedHuman != null) // Корректировка информации
                    {
                        updatedHuman.LastName       = human.LastName;
                        updatedHuman.FirstName      = human.FirstName;
                        updatedHuman.Patronymic     = human.Patronymic;
                        updatedHuman.BirthDate      = human.BirthDate;
                        updatedHuman.BirthCountry   = human.BirthCountry;
                        updatedHuman.BirthPlace     = human.BirthPlace;
                        updatedHuman.DeathDate      = human.DeathDate;
                        updatedHuman.DeathCountry   = human.DeathCountry;
                        updatedHuman.DeathPlace     = human.DeathPlace;
                        updatedHuman.DeathReasonId  = human.DeathReasonId;
                        updatedHuman.ImageFile      = human.ImageFile;
                        updatedHuman.HumanComments  = human.HumanComments;
                        updatedHuman.DaysLived      = human.DaysLived;
                        updatedHuman.FullYearsLived = human.FullYearsLived;

                        MemoContext.SaveChanges();
                    }
                    else // Добавление новой записи
                    {
                        DbHuman record = new()
                        {
                            HumanId        = human.HumanId,
                            LastName       = human.LastName,
                            FirstName      = human.FirstName,
                            Patronymic     = human.Patronymic,
                            BirthDate      = human.BirthDate,
                            BirthCountry   = human.BirthCountry,
                            BirthPlace     = human.BirthPlace,
                            DeathDate      = human.DeathDate,
                            DeathCountry   = human.DeathCountry,
                            DeathPlace     = human.DeathPlace,
                            DeathReasonId  = human.DeathReasonId,
                            ImageFile      = human.ImageFile,
                            HumanComments  = human.HumanComments,
                            DaysLived      = human.DaysLived,
                            FullYearsLived = human.FullYearsLived
                        };

                        MemoContext.DbHumans.Add(record);
                        MemoContext.SaveChanges();
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
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        public bool DeleteHuman(Guid humanId, string imageFile)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var deletedHuman = MemoContext.DbHumans.FirstOrDefault(x => x.HumanId == humanId);
                    if (deletedHuman != null)
                    {
                        MemoContext.Remove(deletedHuman);
                        MemoContext.SaveChanges();
                    }

                    if (imageFile != string.Empty)
                    {
                        if (!DeleteImageFile(imageFile))
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

        ///// <summary>
        ///// Получение изображения выбранного человека
        ///// </summary>
        ///// <param name="currentHuman"></param>
        ///// <returns></returns>
        //public BitmapImage GetHumanImage(Human currentHuman)
        //{
        //    // Читаем файл изображения, если выбранный человек существует и у него есть изображение
        //    if (currentHuman == null || currentHuman.ImageFile == string.Empty) return null;

        //    string combinedImagePath = Path.Combine(ImageFolder, currentHuman.ImageFile);
        //    using Stream stream = File.OpenRead(combinedImagePath);
        //    BitmapImage image = new BitmapImage();
        //    image.BeginInit();
        //    image.CacheOption = BitmapCacheOption.OnLoad;
        //    image.StreamSource = stream;
        //    image.EndInit();
        //    stream.Close();

        //    return image;
        //}

        ///// <summary>
        ///// Сохранение изображения в файл
        ///// </summary>
        ///// <param name="human"></param>
        ///// <param name="humanImage"></param>
        //private static void SaveImageToFile(Human human, BitmapSource humanImage)
        //{
        //    string combinedImagePath = Path.Combine(ImageFolder, human.ImageFile);

        //    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(humanImage));

        //    if (File.Exists(combinedImagePath))
        //    {
        //        File.Delete(combinedImagePath);
        //    }

        //    using FileStream fs = new FileStream(combinedImagePath, FileMode.Create);
        //    encoder.Save(fs);
        //}

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

        public string GetImageFolder()
        {
            return ImageFolder;
        }
        #endregion

        #region Блок работы с людьми для сравнения
        /// <summary>
        /// Получение списка людей для сравнения
        /// </summary>
        /// <returns></returns>
        public List<DbComparedHuman> GetComparedHumans()
        {
            List<DbComparedHuman> comparedHumans = new();
            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    comparedHumans = MemoContext.DbComparedHumans.ToList(); // Читаем контекст базы данных
                }
                catch (Exception ex)
                {
                    comparedHumans = null; // В случае ошибки чтения обнуляем связанный список
                    _logger.Error($"Ошибка чтения людей для сравнения: {ex.HResult}");
                }
            }

            return comparedHumans;
        }

        /// <summary>
        /// Добавление человека для сравнения во внешнее хранилище
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <returns></returns>
        public bool UpdateComparedHuman(DbComparedHuman comparedHuman)
        {
            var success = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var updatedComparedHuman = MemoContext.DbComparedHumans.FirstOrDefault(x => x.ComparedHumanId == comparedHuman.ComparedHumanId);
                    
                    if(updatedComparedHuman != null) // Корректировка информации
                    {
                        updatedComparedHuman.ComparedHumanFullName = comparedHuman.ComparedHumanFullName;
                        updatedComparedHuman.ComparedHumanBirthDate = comparedHuman.ComparedHumanBirthDate;

                        MemoContext.SaveChanges();
                    }
                    else // Добавление новой записи в таблицу людей для сравнения
                    {
                        DbComparedHuman record = new()
                        {
                            ComparedHumanId        = comparedHuman.ComparedHumanId,
                            ComparedHumanFullName  = comparedHuman.ComparedHumanFullName,
                            ComparedHumanBirthDate = comparedHuman.ComparedHumanBirthDate
                        };

                        MemoContext.DbComparedHumans.Add(record);
                        MemoContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    _logger.Error($"Ошибка обновления людей для сравнения: {ex.HResult}");
                }
            }

            return success;
        }

        /// <summary>
        /// Удаление человека для сравнения из внешнего хранилища
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <returns></returns>
        public bool DeleteComparedHuman(Guid compHumanId)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(DbConnectionString))
            {
                try
                {
                    var deletedHuman = MemoContext.DbComparedHumans.FirstOrDefault(x => x.ComparedHumanId == compHumanId);
                    if (deletedHuman != null)
                    {
                        MemoContext.Remove(deletedHuman);
                        MemoContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка удаления человека для сравнения: {ex.HResult}");
                }
            }

            return successResult;
        }
        #endregion

        #region Auxiliary methods
        ///// <summary>
        ///// Формирование плоского списка справочника причин смерти
        ///// </summary>
        ///// <param name="reasons"></param>
        ///// <returns></returns>
        //private static List<Reason> FormPlainReasonsList(List<DbReason> reasons)
        //{
        //    List<Reason> plainReasonsList = new();

        //    foreach (var reason in reasons)
        //    {
        //        Reason rsn = new()
        //        {
        //            ReasonId = reason.ReasonId,
        //            ReasonName = reason.ReasonName,
        //            ReasonComment = reason.ReasonComment,
        //            ReasonDescription = reason.ReasonDescription,
        //            ReasonParentId = reason.ReasonParentId
        //        };
        //        plainReasonsList.Add(rsn);
        //    }

        //    return plainReasonsList;
        //}

        ///// <summary>
        ///// Рекурсивный метод удаления дочерних узлов для удаляемой причины смерти
        ///// </summary>
        //private static void DeletingDaughters(DbReason reason, MemoRandomDbContext context)
        //{
        //    var deletedReason = context.DbReasons.FirstOrDefault(x => x.ReasonId == reason.ReasonId);
        //    if (deletedReason == null) return;

        //    context.Remove(deletedReason);


        //    foreach (var child in reason.ReasonChildren) // Если есть дочерние узлы, то выполняем удаление и по ним
        //    {
        //        DeletingDaughters(child, context);
        //    }
        //}
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
