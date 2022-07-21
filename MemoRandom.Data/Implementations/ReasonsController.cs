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
        private readonly IReasonsHelper _reasonsHelper;
        public static MemoRandomDbContext MemoContext { get; set; }

        /// <summary>
        /// Плоский список причин смерти для сериализации
        /// </summary>
        private List<DbReason> PlainReasonsList { get; set; } = new();

        /// <summary>
        /// Иерархическая коллекция причин смерти
        /// </summary>
        private ObservableCollection<Reason> ReasonsCollection { get; set; } = new();

        #region Блок справочника причин смерти
        /// <summary>
        /// Получение файла причин смерти
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool GetReasonsList()
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    PlainReasonsList = MemoContext.DbReasons.ToList(); // Читаем контекст базы данных

                    FormObservableCollection(PlainReasonsList, null); // Формирование иерархического списка
                    _reasonsHelper.SetReasonsHierarchicalCollection(ReasonsCollection); // Занесение полученного списка в статический класс
                    _reasonsHelper.SetReasonsPlainList(FormPlainReasonsList(PlainReasonsList)); // Занесение плоского списка в статический класс
                }
                catch (Exception ex)
                {
                    ReasonsCollection = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
                    successResult = false; // Устанавливаем флаг неуспеха операции
                    _logger.Error($"Ошибка чтения файла настроек: {ex.HResult}");
                }
            }

            return successResult;
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
                    //ReasonsCollection.Add(reason);
                    //_reasonsHelper.SetReasonsHierarchicalCollection(ReasonsCollection); // Занесение полученного списка в статический класс
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

                        //var correctedReason = ReasonsRepository.ReasonsList.FirstOrDefault(x => x.ReasonId == reason.ReasonId);
                        //correctedReason.ReasonName = reason.ReasonName;
                        //correctedReason.ReasonComment = reason.ReasonComment;
                        //correctedReason.ReasonDescription = reason.ReasonDescription;

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
        private List<Reason> FormPlainReasonsList(List<DbReason> reasons)
        {
            List<Reason> plainReasonsList = new();

            foreach (var reason in reasons)
            {
                // Формируем только те поля, которые нужны для отображения
                Reason rsn = new()
                {
                    ReasonId = reason.DbReasonId,
                    ReasonName = reason.DbReasonName,
                    ReasonComment = reason.DbReasonComment, // Под вопросом
                    ReasonDescription = reason.DbReasonDescription // Под вопросом
                };
                plainReasonsList.Add(rsn);
            }

            return plainReasonsList;
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
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="reasonsHelper"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReasonsController(ILogger logger, IReasonsHelper reasonsHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reasonsHelper = reasonsHelper ?? throw new ArgumentNullException(nameof(reasonsHelper));
        }
        #endregion
    }
}
