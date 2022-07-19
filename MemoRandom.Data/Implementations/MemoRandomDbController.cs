using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MemoRandom.Models.Models;
using MemoRandom.Data.Controllers;
using MemoRandom.Data.DbModels;
using MemoRandom.Data.Interfaces;
using Microsoft.Data.SqlClient;
using NLog;
using MemoRandom.Data.Repositories;

namespace MemoRandom.Data.Implementations
{
    public class MemoRandomDbController : IMemoRandomDbController
    {
        private static readonly string _dbConfigName = "MsDbConfig";
        private static readonly string _dbFolderName = "MsDbFolder";
        private readonly ILogger _logger;
        public static MemoRandomDbContext MemoContext { get; set; }

        /// <summary>
        /// Плоский список причин смерти для сериализации
        /// </summary>
        private List<DbReason> PlainReasonsList { get; set; } = new();
        /// <summary>
        /// Иерархическая коллекция причин смерти
        /// </summary>
        private ObservableCollection<Reason> ReasonsCollection { get; set; }

        #region Блок справочника причин смерти
        /// <summary>
        /// Получение файла причин смерти
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public ObservableCollection<Reason> GetReasonsList()
        {
            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    PlainReasonsList = MemoContext.DbReasons.ToList();

                    ReasonsCollection = new();
                    FormObservableCollection(PlainReasonsList, null); // Формирование иерархического списка
                }
                catch (Exception ex)
                {
                    ReasonsCollection = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
                    _logger.Error($"Ошибка чтения файла настроек: {ex.HResult}");
                }
            }

            return ReasonsCollection;
        }

        /// <summary>
        /// Добавление причины в общий список в БД
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool AddReasonToList(Reason reason)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    DbReason record = new DbReason()
                    {
                        DbReasonId = reason.ReasonId,
                        DbReasonName = reason.ReasonName,
                        DbReasonComment = reason.ReasonComment,
                        DbReasonDescription = reason.ReasonDescription,
                        DbReasonParentId = reason.ReasonParentId
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

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    var updatedReason = MemoContext.DbReasons.FirstOrDefault(x => x.DbReasonId == reason.ReasonId);
                    if (updatedReason != null)
                    {
                        updatedReason.DbReasonName = reason.ReasonName;
                        updatedReason.DbReasonComment = reason.ReasonComment;
                        updatedReason.DbReasonDescription = reason.ReasonDescription;
                        updatedReason.DbReasonParentId = reason.ReasonParentId;

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

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
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

        #region Блок работы со списком людей
        /// <summary>
        /// Получение списка людей из БД
        /// </summary>
        /// <returns></returns>
        public void GetHumansList()
        {
            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    #region New BLOCK
                    // Читаем контекст, выбирая только основные поля (без изображений)
                    var newList = MemoContext.DbHumans.Select(h => new
                    {
                        DbHumanId = h.DbHumanId,
                        DbLastName = h.DbLastName,
                        DbFirstName = h.DbFirstName,
                        DbPatronymic = h.DbPatronymic,
                        DbBirthDate = h.DbBirthDate,
                        DbBirthCountry = h.DbBirthCountry,
                        DbBirthPlace = h.DbBirthPlace,
                        DbDeathDate = h.DbDeathDate,
                        DbDeathCountry = h.DbDeathCountry,
                        DbDeathPlace = h.DbDeathPlace,
                        DbImageFile = h.DbImageFile,
                        DbDeathReasonId = h.DbDeathReasonId,
                        DbHumanComments = h.DbHumanComments
                    }).OrderBy(x => x.DbLastName);

                    // Перегоняем в результирующий список
                    List<Human> humansList = new();
                    foreach (var person in newList)
                    {
                        Human human = new()
                        {
                            HumanId = person.DbHumanId,
                            LastName = person.DbLastName,
                            FirstName = person.DbFirstName,
                            Patronymic = person.DbPatronymic,
                            BirthDate = person.DbBirthDate,
                            BirthCountry = person.DbBirthCountry,
                            BirthPlace = person.DbBirthPlace,
                            DeathDate = person.DbDeathDate,
                            DeathCountry = person.DbDeathCountry,
                            DeathPlace = person.DbDeathPlace,
                            ImageFile = person.DbImageFile,
                            DeathReasonId = person.DbDeathReasonId,
                            HumanComments = person.DbHumanComments
                        };
                        humansList.Add(human);
                    }

                    HumansRepository.HumansList.Clear();
                    HumansRepository.HumansList = humansList;
                    #endregion

                    //var humansList = MemoContext.DbHumans.ToList();

                    //HumansRepository.HumansList.Clear();
                    //HumansRepository.HumansList = GetInnerHumans(humansList);
                }
                catch (Exception ex)
                {
                    HumansRepository.HumansList = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
                    _logger.Error($"Ошибка чтения файла по людям: {ex.HResult}");
                }
            }
        }

        ///// <summary>
        ///// Чтение изображения из внешнего хранилища
        ///// </summary>
        ///// <param name="human"></param>
        //public void GetPicture(Human human)
        //{
        //    using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
        //    {
        //        try
        //        {
        //            var image = MemoContext.DbHumans.FirstOrDefault(x => x.DbHumanId == human.HumanId).DbHumanImage;
        //            var row = HumansRepository.HumansList.FirstOrDefault(x => x.HumanId == human.HumanId);
        //            row.HumanImage = image;
        //        }
        //        catch(Exception ex)
        //        {
        //            //HumansRepository.HumansList = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
        //            _logger.Error($"Ошибка чтения изображения человека: {ex.HResult}");
        //        }
        //    }
        //}

        /// <summary>
        /// Добавление сущности человека в общий список
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool AddHumanToList(Human human)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    // Создаем новую запись
                    DbHuman record = new DbHuman()
                    {
                        DbHumanId = human.HumanId,
                        DbLastName = human.LastName,
                        DbFirstName = human.FirstName,
                        DbPatronymic = human.Patronymic,
                        DbBirthDate = human.BirthDate,
                        DbBirthCountry = human.BirthCountry,
                        DbBirthPlace = human.BirthPlace,
                        DbDeathDate = human.DeathDate,
                        DbDeathCountry = human.DeathCountry,
                        DbDeathPlace = human.DeathPlace,
                        DbImageFile = human.ImageFile,
                        DbDeathReasonId = human.DeathReasonId,
                        DbHumanComments = human.HumanComments
                    };
                    MemoContext.DbHumans.Add(record);
                    MemoContext.SaveChanges();

                    SaveImageToFile(human); // Сохраняем изображение
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка записи информации по человеку: {ex.HResult}");
                    MessageBox.Show($"Error: {ex.HResult}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            HumansRepository.CurrentHuman = human;
            return successResult;
        }

        /// <summary>
        /// Обновление сущности человека в общем списке
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool UpdateHumanInList(Human human)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    var updatedHuman = MemoContext.DbHumans.FirstOrDefault(x => x.DbHumanId == human.HumanId);
                    if (updatedHuman != null) // Корректировка информации
                    {
                        updatedHuman.DbLastName = human.LastName;
                        updatedHuman.DbFirstName = human.FirstName;
                        updatedHuman.DbPatronymic = human.Patronymic;
                        updatedHuman.DbBirthDate = human.BirthDate;
                        updatedHuman.DbBirthCountry = human.BirthCountry;
                        updatedHuman.DbBirthPlace = human.BirthPlace;
                        updatedHuman.DbDeathDate = human.DeathDate;
                        updatedHuman.DbDeathCountry = human.DeathCountry;
                        updatedHuman.DbDeathPlace = human.DeathPlace;
                        updatedHuman.DbDeathReasonId = human.DeathReasonId;
                        updatedHuman.DbImageFile = human.ImageFile;
                        updatedHuman.DbHumanComments = human.HumanComments;

                        MemoContext.SaveChanges();
                    }
                    else // Добавление новой записи
                    {
                        DbHuman record = new()
                        {
                            DbHumanId = human.HumanId,
                            DbLastName = human.LastName,
                            DbFirstName = human.FirstName,
                            DbPatronymic = human.Patronymic,
                            DbBirthDate = human.BirthDate,
                            DbBirthCountry = human.BirthCountry,
                            DbBirthPlace = human.BirthPlace,
                            DbDeathDate = human.DeathDate,
                            DbDeathCountry = human.DeathCountry,
                            DbDeathPlace = human.DeathPlace,
                            DbDeathReasonId = human.DeathReasonId,
                            DbImageFile = human.ImageFile,
                            DbHumanComments = human.HumanComments
                        };

                        MemoContext.DbHumans.Add(record);

                        MemoContext.SaveChanges();
                    }

                    SaveImageToFile(human); // Сохраняем изображение
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
        /// Удаление сущности человека из общего списка
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool DeleteHumanFromList(Human human)
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    var deletedHuman = MemoContext.DbHumans.FirstOrDefault(x => x.DbHumanId == human.HumanId);
                    if (deletedHuman != null)
                    {
                        MemoContext.Remove(deletedHuman);
                        MemoContext.SaveChanges();
                    }

                    if(human.ImageFile != string.Empty)
                    {
                        if (!DeleteImageFile(human.ImageFile))
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
        #endregion

        #region Auxiliary methods
        /// <summary>
        /// Сохранение изображения в файл
        /// </summary>
        /// <param name="human"></param>
        private void SaveImageToFile(Human human)
        {
            string combinedImagePath = Path.Combine(HumansRepository.ImageFolder, human.ImageFile);
            using (MemoryStream ms = new MemoryStream(human.HumanImage))
            {
                using (var fs = new FileStream(combinedImagePath, FileMode.Create))
                {
                    ms.WriteTo(fs);
                }
            }
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
                string combinedImagePath = Path.Combine(HumansRepository.ImageFolder, fileName);
                File.Delete(combinedImagePath);
            }
            catch(Exception ex)
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
        private void FormObservableCollection(List<DbReason> reasons, Reason headReason)
        {
            for (int i = 0; i < reasons.Count; i++)
            {
                if (reasons[i].DbReasonParentId == Guid.Empty) // Случай корневых узлов
                {
                    Reason rsn = new()
                    {
                        ReasonParentId = reasons[i].DbReasonParentId,
                        ReasonId = reasons[i].DbReasonId,
                        ReasonName = reasons[i].DbReasonName,
                        ReasonComment = reasons[i].DbReasonComment,
                        ReasonDescription = reasons[i].DbReasonDescription
                    };
                    ReasonsCollection.Add(rsn);

                    // Проверка на наличие дочерних узлов
                    List<DbReason> daughters = PlainReasonsList.FindAll(x => x.DbReasonParentId == rsn.ReasonId);
                    if (daughters.Count != 0) // Если дочерние узлы найдены
                    {
                        FormObservableCollection(daughters, rsn); // Вызываем рекурсивно
                    }
                }
                else if (headReason != null)// Случай вложенных узлов
                {
                    Reason rsn = new()
                    {
                        ReasonId = reasons[i].DbReasonId,
                        ReasonName = reasons[i].DbReasonName,
                        ReasonComment = reasons[i].DbReasonComment,
                        ReasonDescription = reasons[i].DbReasonDescription,
                        ReasonParentId = headReason.ReasonId,
                        ReasonParent = headReason
                    };
                    headReason.ReasonChildren.Add(rsn);

                    // Проверка на наличие дочерних узлов
                    List<DbReason> daughters = PlainReasonsList.FindAll(x => x.DbReasonParentId == rsn.ReasonId);
                    if (daughters.Count != 0) // Если дочерние узлы найдены
                    {
                        FormObservableCollection(daughters, rsn); // Вызываем рекурсивно
                    }
                }
            }
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

        #region CTOR
        public MemoRandomDbController(ILogger logger)
        {
            _logger = logger;

            //MemoContext = new MemoRandomDbContext(GetConnectionString());
        }
        #endregion
    }
}
