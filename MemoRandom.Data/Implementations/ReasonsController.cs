using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MemoRandom.Models.Models;
using MemoRandom.Data.Controllers;
using MemoRandom.Data.DbModels;
using MemoRandom.Data.Interfaces;
using NLog;
using MemoRandom.Data.Repositories;
using MemoRandom.Models.Interfaces;

namespace MemoRandom.Data.Implementations
{
    public class ReasonsController : IReasonsController
    {
        private readonly ILogger _logger;
        public static MemoRandomDbContext MemoContext { get; set; }

        /// <summary>
        /// Плоский список причин смерти для сериализации
        /// </summary>
        private List<DbReason> PlainReasonsList { get; set; } = new();

        #region Блок справочника причин смерти
        /// <summary>
        /// Получение справочника причин смерти
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<Reason> GetReasons()
        {
            List<Reason> reasons = new();
            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
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

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
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

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
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

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
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








        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        public ReasonsController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion
    }
}
