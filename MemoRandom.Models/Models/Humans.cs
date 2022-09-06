using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MemoRandom.Models.Models
{
    /// <summary>
    /// Статический класс, содержащий в себе список людей
    /// </summary>
    public static class Humans
    {
        /// <summary>
        /// Статический класс, получаемый из внешнего хранилища
        /// </summary>
        public static ObservableCollection<Human> HumansList { get; set; } = new();

        /// <summary>
        /// Текущий человек, с которым ведется работа
        /// </summary>
        public static Human CurrentHuman { get; set; }

        /// <summary>
        /// Метод получения списка людей
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Human> GetHumans()
        {
            ObservableCollection<Human> humans = new();
            foreach (var item in HumansList)
            {
                humans.Add(item);
            }

            return humans;
        }

        /// <summary>
        /// Обновление человека в списке местного хранилища
        /// </summary>
        /// <param name="human"></param>
        public static void UpdateHuman(Human human)
        {
            var updatedHuman = HumansList.FirstOrDefault(x => x.HumanId == human.HumanId);
            
            updatedHuman.LastName = human.LastName;
            updatedHuman.FirstName = human.FirstName;
            updatedHuman.Patronymic = human.Patronymic;
            updatedHuman.BirthDate = human.BirthDate;
            updatedHuman.BirthCountry = human.BirthCountry;
            updatedHuman.BirthPlace = human.BirthPlace;
            updatedHuman.DeathDate = human.DeathDate;
            updatedHuman.DeathCountry = human.DeathCountry;
            updatedHuman.DeathPlace = human.DeathPlace;
            updatedHuman.DeathReasonId = human.DeathReasonId;
            updatedHuman.DaysLived = human.DaysLived;
            updatedHuman.FullYearsLived = human.FullYearsLived;
            updatedHuman.HumanComments = human.HumanComments;
            updatedHuman.ImageFile = human.ImageFile;
        }
    }
}
