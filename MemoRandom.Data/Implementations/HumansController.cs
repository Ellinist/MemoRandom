﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MemoRandom.Data.Interfaces;
using MemoRandom.Data.Repositories;
using MemoRandom.Models.Models;

namespace MemoRandom.Data.Implementations
{
    /// <summary>
    /// Контроллер работы с людьми
    /// </summary>
    public class HumansController : IHumansController
    {
        private readonly IMemoRandomDbController _memoRandomDbController;

        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        public List<Human> GetHumansList()
        {
            _memoRandomDbController.GetHumansList(); // Формируем репозиторий
            return HumansRepository.HumansList; // Возвращаем список людей из репозитория
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
        /// Получение текущего человека, с которым ведется работа
        /// </summary>
        /// <returns></returns>
        public Human GetCurrentHuman()
        {
            return HumansRepository.CurrentHuman;
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
                return _memoRandomDbController.UpdateHumanInList(currentHuman, humanImage);
            }
            else // Новая запись
            {
                //TODO здесь добавление записи
                return _memoRandomDbController.AddHumanToList(currentHuman, humanImage);
            }
        }

        /// <summary>
        /// Удаление человека из внешнего хранилища
        /// </summary>
        /// <param name="human"></param>
        /// <returns></returns>
        public bool DeleteHuman()
        {
            var currentHuman = GetCurrentHuman();
            if(currentHuman != null)
            {
                return _memoRandomDbController.DeleteHumanFromList(currentHuman);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Получение изображения выбранного человека
        /// </summary>
        public BitmapImage GetHumanImage()
        {
            var currentHuman = GetCurrentHuman();
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

        #region CTOR
        public HumansController(IMemoRandomDbController memoRandomDbController)
        {
            _memoRandomDbController = memoRandomDbController ?? throw new ArgumentNullException(nameof(memoRandomDbController));
        }
        #endregion
    }
}
