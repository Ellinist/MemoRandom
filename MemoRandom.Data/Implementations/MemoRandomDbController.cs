using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MemoRandom.Models.Models;
using MemoRandom.Data.Controllers;
using MemoRandom.Data.DbModels;
using MemoRandom.Data.Interfaces;
using Microsoft.Data.SqlClient;
using NLog;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic;
using static System.Net.Mime.MediaTypeNames;

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

        #region Блок работы со списком людей
        /// <summary>
        /// Получение списка людей из БД
        /// </summary>
        /// <returns></returns>
        public List<Human> GetHumasList()
        {
            return GettingHumans().Result;
        }

        /// <summary>
        /// Добавление сущности человека в общий список
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool AddHumanToList(Human human)
        {
            return AddingHuman(human).Result;
        }
        #endregion

        #region Auxiliary methods
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

        /// <summary>
        /// Получение списка людей
        /// </summary>
        /// <returns></returns>
        private async Task<List<Human>> GettingHumans()
        {
            await using (MemoContext = new MemoRandomDbContext(GetConnectionString()))
            {
                try
                {
                    var humansList = MemoContext.DbHumans.ToList();

                    return GetInnerHumans(humansList);
                }
                catch (Exception ex)
                {
                    ReasonsCollection = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
                    _logger.Error($"Ошибка чтения файла настроек: {ex.HResult}");
                    return null;
                }
            }
        }

        private List<Human> GetInnerHumans(List<DbHuman> humans)
        {
            List<Human> resultList = new();
            foreach (var person in humans)
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
                    // Эти две строки - думать, как переместить в асинхронность иного рода
                    HumanImage = person.DbHumanImage,
                    //ImageFilePath = person.DbImageFilePath,
                    DeathReasonId = person.DbDeathReasonId,
                    HumanComments = person.DbHumanComments
                };
                resultList.Add(human);
            }

            return resultList;
        }

        /// <summary>
        /// Асинхронный метод добавления человека в список БД
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        private async Task<bool> AddingHuman(Human human)
        {
            bool successResult = true;

            await using (MemoContext = new MemoRandomDbContext(GetConnectionString()))
            {
                try
                {
                    using Stream bms = File.Open(@"d:\Couple\Котлета2.jpg", FileMode.Open);

                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = bms;
                    bm.CacheOption = BitmapCacheOption.OnLoad;
                    bm.EndInit();

                    byte[] res;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bm));
                    using MemoryStream ms = new MemoryStream();
                    encoder.Save(ms);
                    res = ms.ToArray();

                    //BitmapImage img = new BitmapImage();

                    //using (var ms = new MemoryStream())
                    //{
                    //    byte[] res = /*ms.ToArray();*/
                    //}


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
                        //DbHumanImage = human.HumanImage,
                        DbHumanImage = res,
                        DbImageFilePath = human.ImageFilePath,
                        DbDeathReasonId = human.DeathReasonId,
                        DbHumanComments = human.HumanComments
                    };
                    MemoContext.DbHumans.Add(record);

                    MemoContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка записи информации по человеку: {ex.HResult}");
                    //MessageBox.Show($"Error: {ex.HResult}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return successResult;
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
