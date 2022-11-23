using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MemoRandom.Data.DbModels;
using MemoRandom.Models.Models;

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
            PlainReasonsList = ConvertFromDbSet(_msSqlController.GetReasons());
            FormObservableCollection(PlainReasonsList, null);
            #endregion

            #region Чтение списка категорий
            AgeCategories = _msSqlController.GetCategories();
            #endregion

            #region Чтение списка людей для сравнения
            ComparedHumansCollection = _msSqlController.GetComparedHumans();
            #endregion

            return successResult;
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

        private List<Reason> ConvertFromDbSet(List<DbReason> dbList)
        {
            List<Reason> reasons = new();
            
            foreach(DbReason dbReason in dbList)
            {
                Reason reason = new()
                {
                     ReasonId = dbReason.ReasonId,
                     ReasonName = dbReason.ReasonName,
                     ReasonComment = dbReason.ReasonComment,
                     ReasonDescription = dbReason.ReasonDescription,
                     ReasonParentId = dbReason.ReasonParentId
                };
                reasons.Add(reason);
            }

            return reasons;
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
