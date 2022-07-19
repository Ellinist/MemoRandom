using System;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MemoRandom.Data.Interfaces;
using MemoRandom.Data.Repositories;
using MemoRandom.Models.Models;
using MemoRandom.Data.Controllers;
using MemoRandom.Data.DbModels;
using System.Windows;

namespace MemoRandom.Data.Implementations
{
    /// <summary>
    /// Контроллер работы с людьми
    /// </summary>
    public class HumansController : IHumansController
    {
        private readonly IMemoRandomDbController _memoRandomDbController;
        private readonly ILogger _logger;

        public static MemoRandomDbContext MemoContext { get; set; }

        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public List<Human> GetHumansList()
        {
            ReadHumansList(); // Формируем репозиторий
            return HumansRepository.HumansList; // Возвращаем список людей из репозитория
        }

        /// <summary>
        /// Получение списка людей из БД
        /// </summary>
        /// <returns></returns>
        public void ReadHumansList()
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
                }
                catch (Exception ex)
                {
                    HumansRepository.HumansList = null; // В случае неуспеха чтения обнуляем иерархическую коллекцию
                    _logger.Error($"Ошибка чтения файла по людям: {ex.HResult}");
                }
            }
        }


        /// <summary>
        /// Установка текущего человека, с которым ведется работа
        /// </summary>
        /// <param name="human"></param>
        public void SetCurrentHuman(Human human)
        {
            HumansRepository.CurrentHuman = human;
        }

        /// <summary>
        /// Сохранение человека во внешнем хранилище
        /// Если человек уже есть, то обновление записи
        /// Если человек новый, то добавление записи
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool UpdateHumans(BitmapImage humanImage)
        {
            var currentHuman = GetCurrentHuman();
            if (currentHuman != null) // Существующая запись
            {
                //TODO здесь вызов обновления записи
                return UpdateHumanInList(currentHuman, humanImage);
            }
            else // Новая запись
            {
                //TODO здесь добавление записи
                return AddHumanToList(currentHuman, humanImage);
            }
        }

        /// <summary>
        /// Обновление сущности человека в общем списке
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool UpdateHumanInList(Human human, BitmapImage humanImage)
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

                        SaveImageToFile(human, humanImage); // Сохраняем изображение
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

                    SaveImageToFile(human, humanImage); // Сохраняем изображение
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
        /// Добавление сущности человека в общий список
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool AddHumanToList(Human human, BitmapImage humanImage)
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

                    SaveImageToFile(human, humanImage); // Сохраняем изображение
                }
                catch (Exception ex)
                {
                    successResult = false;
                    _logger.Error($"Ошибка записи информации по человеку: {ex.HResult}");
                    //MessageBox.Show($"Error: {ex.HResult}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            HumansRepository.CurrentHuman = human;
            return successResult;
        }

        /// <summary>
        /// Удаление человека из внешнего хранилища
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool DeleteHuman()
        {
            bool successResult = true;

            using (MemoContext = new MemoRandomDbContext(HumansRepository.DbConnectionString))
            {
                try
                {
                    var currentHuman = HumansRepository.CurrentHuman;

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
        public BitmapImage GetHumanImage()
        {
            var currentHuman = HumansRepository.CurrentHuman;
            // Читаем файл изображения, если выбранный человек существует и у него есть изображение
            if (currentHuman != null && currentHuman.ImageFile != String.Empty)
            {
                string combinedImagePath = Path.Combine(HumansRepository.ImageFolder, currentHuman.ImageFile);

                //BitmapImage image = new BitmapImage(new Uri(combinedImagePath));
                using (Stream stream = File.OpenRead(combinedImagePath))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    stream.Close();

                    return image;
                }
            }

            return null;
        }

        /// <summary>
        /// Сохранение изображения в файл
        /// </summary>
        /// <param name="human"></param>
        private void SaveImageToFile(Human human, BitmapImage humanImage)
        {
            string combinedImagePath = Path.Combine(HumansRepository.ImageFolder, human.ImageFile);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(humanImage));

            if (File.Exists(combinedImagePath))
            {
                //DeleteImageFile(human.ImageFile);
                File.Delete(combinedImagePath);
            }

            using (FileStream fs = new FileStream(combinedImagePath, FileMode.Create))
            {
                encoder.Save(fs);
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
            catch (Exception ex)
            {
                successResult = false;
                _logger.Error($"Ошибка удаления файла изображения: {ex.HResult}");
            }

            return successResult;
        }

        /// <summary>
        /// Получение текущего человека, с которым ведется работа
        /// </summary>
        /// <returns></returns>
        public Human GetCurrentHuman()
        {
            return HumansRepository.CurrentHuman;
        }


        #region CTOR
        public HumansController(ILogger logger, IMemoRandomDbController memoRandomDbController)
        {
            _memoRandomDbController = memoRandomDbController ?? throw new ArgumentNullException(nameof(memoRandomDbController));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion
    }
}
