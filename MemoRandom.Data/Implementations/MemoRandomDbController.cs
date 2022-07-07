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

namespace MemoRandom.Data.Implementations
{
    public class MemoRandomDbController : IMemoRandomDbController
    {
        private static readonly string _configName = "MsDbConfig";
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
            return GettingReasons().Result;
        }

        /// <summary>
        /// Добавление причины в общий список в БД
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool AddReasonToList(Reason reason)
        {
            return AddingReason(reason).Result;
        }

        /// <summary>
        /// Обновление измененных данных причины смерти
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool UpdateReasonInList(Reason reason)
        {
            return UpdatingReason(reason).Result;
        }

        /// <summary>
        /// Удаление причины смерти и всех ее дочерних узлов
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool DeleteReasonInList(Reason reason)
        {
            return DeletingReasons(reason).Result;
        }
        #endregion

        //#region Блок работы со списком людей
        ///// <summary>
        ///// Получение пути к файлу списка людей
        ///// </summary>
        ///// <param name="file"></param>
        ///// <returns></returns>
        //public async Task<string> GetXmlHumansDataFile(string file)
        //{
        //    using var stream = File.OpenRead(file);
        //    XmlSerializer deserializer = new(typeof(string));
        //    var resultHumansPath = (string)deserializer.Deserialize(stream);
        //    stream.Dispose();

        //    return await Task<string>.FromResult(resultHumansPath); // Временно
        //}

        ///// <summary>
        ///// Установка пути к файлу со списком людей
        ///// </summary>
        ///// <param name="file"></param>
        ///// <param name="humansFile"></param>
        //public async Task<bool> SetXmlHumansDataFile(string file, string humansFile)
        //{
        //    bool successResult = true;
        //    try
        //    {
        //        using var stream = File.Create(file);
        //        XmlSerializer serializer = new(typeof(string));
        //        serializer.Serialize(stream, humansFile);
        //        stream.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        successResult = false;
        //        _logger.Error($"Ошибка формирования пути к файлу информации по людям: {ex.HResult}");
        //    }

        //    return await Task<bool>.FromResult(successResult);
        //}

        //public async Task<List<Human>> GetXmlHumansList(string file)
        //{
        //    return null;
        //}

        //public async Task<bool> SaveXmlHumansList(List<Human> humansList, string humansFile)
        //{
        //    return true;
        //}
        //#endregion

        #region Auxiliary methods
        /// <summary>
        /// Формирование иерархической коллекции
        /// </summary>
        /// <param name="reasons">Плоский список</param>
        /// <param name="headReason">Головной элемент (экземпляр класса)</param>
        private void FormObservableCollection(List<DbReason> reasons, Reason? headReason)
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
        /// Асинхронный метод получения справочника причин смерти
        /// </summary>
        /// <returns></returns>
        private async Task<ObservableCollection<Reason>> GettingReasons()
        {
            await using (MemoContext = new MemoRandomDbContext(GetConnectionString()))
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
        /// Асинхронный метод добавления новой причины смерти в справочник
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task<bool> AddingReason(Reason reason)
        {
            bool successResult = true;

            await using (MemoContext = new MemoRandomDbContext(GetConnectionString()))
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
        /// Асинхронный метод обновления причины смерти
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task<bool> UpdatingReason(Reason reason)
        {
            bool successResult = true;

            await using (MemoContext = new MemoRandomDbContext(GetConnectionString()))
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
        /// Асинхронный метод удаления причины смерти и всех ее дочерних узлов
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task<bool> DeletingReasons(Reason reason)
        {
            bool successResult = true;

            await using (MemoContext = new MemoRandomDbContext(GetConnectionString()))
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

        /// <summary>
        /// Получение построителя соединения
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {

            string filename = ConfigurationManager.AppSettings[_configName];
            if (filename == null) return null;
            string combinedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = @"Kotarius\KotariusServer",
                AttachDBFilename = combinedPath,
                InitialCatalog = Path.GetFileNameWithoutExtension(combinedPath),
                IntegratedSecurity = true
            };

            return connectionStringBuilder.ConnectionString;
        }

        #region CTOR
        public MemoRandomDbController(ILogger logger)
        {
            _logger = logger;

            //MemoContext = new MemoRandomDbContext(GetConnectionString());
        }
        #endregion
    }
}
