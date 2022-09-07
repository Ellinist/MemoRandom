using MemoRandom.Data.DbModels;
using MemoRandom.Models.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MemoRandom.Data.Interfaces
{
    /// <summary>
    /// Интерфейс работы с БД
    /// </summary>
    public interface IMsSqlController
    {
        /// <summary>
        /// Установка путей сохранения и сроки подключения к БД
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="imagesFilePath"></param>
        /// <param name="serverName"></param>
        /// <returns></returns>
        bool SetPaths(string fileName, string filePath, string imagesFilePath, string serverName);

        #region Блок работы со справочником причин смерти
        /// <summary>
        /// Получение древовидной коллекции списка причин смерти
        /// </summary>
        /// <returns></returns>
        List<Reason> GetReasons();

        /// <summary>
        /// Добавление причины с общий список
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool AddReasonToList(Reason reason);

        /// <summary>
        /// Обновление измененных данных причины смерти
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool UpdateReasonInList(Reason reason);

        /// <summary>
        /// Удаление выбранной причины смерти и всех ее дочек
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        bool DeleteReasonInList(Reason reason);
        #endregion

        #region Блок работы с категориями
        /// <summary>
        /// Получение списка категорий из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        ObservableCollection<Category> GetCategories();

        /// <summary>
        /// Обновление категории во внешнем хранилище
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        bool UpdateCategories(Category category);

        /// <summary>
        /// Удаление категории из внешнего хранилища
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        bool DeleteCategory(Category category);
        #endregion

        #region Блок работы с людьми
        /// <summary>
        /// Получение списка людей из внешнего хранилища
        /// </summary>
        /// <returns></returns>
        ObservableCollection<Human> GetHumans();

        /// <summary>
        /// Обновление (добавление или редактирование) списка людей
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <param name="humanImage"></param>
        /// <returns></returns>
        bool UpdateHumans(Human currentHuman, BitmapImage humanImage);

        /// <summary>
        /// Удаление человека из списка
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        bool DeleteHuman(Human currentHuman);

        /// <summary>
        /// Получение изображения человека
        /// </summary>
        /// <param name="currentHuman"></param>
        /// <returns></returns>
        BitmapImage GetHumanImage(Human currentHuman);
        #endregion

        #region Блок работы с людьми для сравнения
        List<ComparedHuman> GetComparedHumans();

        bool AddComparedHuman(ComparedHuman comparedHuman);

        bool DeleteComparedHuman(ComparedHuman comparedHuman);
        #endregion
    }
}
