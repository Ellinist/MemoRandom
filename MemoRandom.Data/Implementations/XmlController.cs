using MemoRandom.Data.DtoModels;
using MemoRandom.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MemoRandom.Data.Implementations
{
    public class XmlController : IXmlController
    {
        #region Блок работы со справочником причин смерти
        /// <summary>
        /// Чтение справочника причин смерти из файла XML
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<DtoReason> ReadReasonsFromFile(string filePath)
        {
            List<DtoReason> reasons = new(); // Создание списка DTO данных по причинам смерти
            var xmlReasons = XDocument.Load(filePath); // Чтение из файла

            var root = xmlReasons.Element("Reasons"); // Чтение корневого элемента
            if (root != null) // Если элемент существует
            {
                foreach (var reason in root.Elements("Reason")) // В цикле по всем вложенным элементам с типом "Reason"
                {
                    DtoReason rsn = new() // Создаем новый DTO-объект
                    {
                        ReasonId          = Guid.Parse(reason.Attribute("id")!.Value),
                        ReasonName        = reason.Element("name")!.Value,
                        ReasonComment     = reason.Element("comment")!.Value,
                        ReasonDescription = reason.Element("description")!.Value,
                        ReasonParentId    = Guid.Parse(reason.Element("parent")!.Value)
                    };
                    reasons.Add(rsn);
                }

                return reasons;
            }
            else
            {
                return null; // Если корневого элемента нет, то возвращаем null
            }
        }

        /// <summary>
        /// Добавление причины с общий список
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public void AddReasonToList(DtoReason reason, string filePath)
        {
            var xmlReasons = XDocument.Load(filePath);
            var root = xmlReasons.Element("Reasons");
            if (root != null)
            {
                XElement rsn         = new XElement("Reason");
                XAttribute id        = new XAttribute("id", $"{reason.ReasonId}");
                XElement name        = new XElement("name", $"{reason.ReasonName}");
                XElement comment     = new XElement("comment", $"{reason.ReasonComment}");
                XElement description = new XElement("description", $"{reason.ReasonDescription}");
                XElement parent      = new XElement("parent", $"{reason.ReasonParentId}");

                rsn.Add(id);
                rsn.Add(name);
                rsn.Add(comment);
                rsn.Add(description);
                rsn.Add(parent);

                root.Add(rsn);
            }
            xmlReasons.Save(filePath);
        }

        /// <summary>
        /// Изменение причины в файле
        /// </summary>
        /// <param name="rsn"></param>
        /// <param name="filePath"></param>
        public void ChangeReasonInFile(DtoReason rsn, string filePath)
        {
            var xmlReasons = XDocument.Load(filePath);
            var root = xmlReasons.Element("Reasons");
            if (root != null)
            {
                var rootElements = root.Elements("Reason");
                var element = rootElements.FirstOrDefault(x => Guid.Parse(x.Attribute("id")!.Value) == rsn.ReasonId);
                if (element != null)
                {
                    element.Element("name")!.Value = rsn.ReasonName;
                    element.Element("comment")!.Value = rsn.ReasonComment;
                    element.Element("description")!.Value = rsn.ReasonDescription;
                    
                    xmlReasons.Save(filePath);
                }
            }
        }

        /// <summary>
        /// Удаление причины из файла
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        public void DeleteReasonInFile(string id, string filePath)
        {
            var xmlReasons = XDocument.Load(filePath);
            var root = xmlReasons.Element("Reasons");
            if (root != null)
            {
                var rootElements = root.Elements("Reason");
                var element = rootElements.FirstOrDefault(x => x.Attribute("id")!.Value == id);
                if(element != null)
                {
                    element.Remove();
                    xmlReasons.Save(filePath);
                }
            }
        }

        ///// <summary>
        ///// Временно - Сохранение всех причин смерти в файле XML
        ///// Пока что временно - а там посмотрим
        ///// </summary>
        ///// <param name="reasons"></param>
        ///// <param name="filePath"></param>
        ///// <returns></returns>
        //public void SaveReasonsToFile(List<DtoReason> reasons, string filePath)
        //{
        //    XDocument xmlReasons = new(); // Создаем новый документ
        //    XElement root = new("Reasons"); // Корневой элемент причин смерти

        //    foreach (var item in reasons) // В цикле для всех причин смерти
        //    {
        //        XElement reason = new XElement("Reason"); // Заголовок причины
        //        XAttribute id = new XAttribute("id", $"{item.ReasonId}"); // И ее атрибут

        //        #region Создание вложенных элементов причины смерти
        //        XElement name = new XElement("name", $"{item.ReasonName}");
        //        XElement comment = new XElement("comment", $"{item.ReasonComment}");
        //        XElement description = new XElement("description", $"{item.ReasonDescription}");
        //        XElement parent = new XElement("parent", $"{item.ReasonParentId}");
        //        #endregion

        //        #region Добавление к заголовку его атрибута и дочерних элементов
        //        reason.Add(id);
        //        reason.Add(name);
        //        reason.Add(comment);
        //        reason.Add(description);
        //        reason.Add(parent);
        //        #endregion

        //        root.Add(reason);
        //    }

        //    xmlReasons.Add(root);
        //    xmlReasons.Save(filePath);
        //}
        #endregion

        #region Блок работы с категориями возрастов
        /// <summary>
        /// Чтение категорий возрастов из XML-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<DtoCategory> ReadCategoriesFromFile(string filePath)
        {
            List<DtoCategory> categories = new();
            var xmlCategories = XDocument.Load(filePath);

            var root = xmlCategories.Element("Categories");
            if (root != null)
            {
                foreach (XElement category in root.Elements("Category"))
                {
                    DtoCategory cat = new()
                    {
                        CategoryId   = Guid.Parse(category.Attribute("id")!.Value),
                        CategoryName = category.Element("name")!.Value,
                        StartAge     = category.Element("startage")!.Value,
                        StopAge      = category.Element("stopage")!.Value,
                        StringColor  = category.Element("color")!.Value
                    };
                    categories.Add(cat);
                }
            }

            return categories;
        }

        /// <summary>
        /// Обновление категории в файле
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public void UpdateCategoryInFile(DtoCategory category, string filePath)
        {
            var xmlCategories = XDocument.Load(filePath);
            var root = xmlCategories.Element("Categories");
            if (root != null)
            {
                var rootElements = root.Elements("Category");
                var element = rootElements.FirstOrDefault(x => Guid.Parse(x.Attribute("id")!.Value) == category.CategoryId);
                if (element != null)
                {
                    element.Element("name")!.Value     = category.CategoryName;
                    element.Element("startage")!.Value = category.StartAge;
                    element.Element("stopage")!.Value  = category.StopAge;
                    element.Element("color")!.Value    = category.StringColor;
                }
                else
                {
                    XElement cat      = new XElement("Category");
                    XAttribute id     = new XAttribute("id", $"{category.CategoryId}");
                    XElement name     = new XElement("name", $"{category.CategoryName}");
                    XElement startage = new XElement("startage", $"{category.StartAge}");
                    XElement stopage  = new XElement("stopage", $"{category.StopAge}");
                    XElement color    = new XElement("color", $"{category.StringColor}");

                    cat.Add(id);
                    cat.Add(name);
                    cat.Add(startage);
                    cat.Add(stopage);
                    cat.Add(color);

                    root.Add(cat);
                }

                xmlCategories.Save(filePath);
            }
        }

        /// <summary>
        /// Удаление категории из файла
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public void DeleteCategoryInFile(string id, string filePath)
        {
            var xmlCategories = XDocument.Load(filePath);
            var root = xmlCategories.Element("Categories");
            if (root != null)
            {
                var rootElements = root.Elements("Category");
                var element = rootElements.FirstOrDefault(x => x.Attribute("id")!.Value == id);
                if (element != null)
                {
                    element.Remove();
                    xmlCategories.Save(filePath);
                }
            }
        }

        ///// <summary>
        ///// Временно - Сохранение списка категорий в файле XML
        ///// </summary>
        ///// <param name="categories"></param>
        ///// <param name="filePath"></param>
        ///// <returns></returns>
        //public void SaveCategoriesToFile(List<DtoCategory> categories, string filePath)
        //{
        //    XDocument xmlCategories = new();
        //    XElement root = new("Categories"); // Корневой элемент категорий

        //    foreach (var item in categories)
        //    {
        //        XElement category = new XElement("Category");
        //        XAttribute id     = new XAttribute("id", $"{item.CategoryId}");
        //        XElement name     = new XElement("name", $"{item.CategoryName}");
        //        XElement startage = new XElement("startage", $"{item.StartAge}");
        //        XElement stopage  = new XElement("stopage", $"{item.StopAge}");
        //        XElement color    = new XElement("color", $"{item.StringColor}");

        //        category.Add(id);
        //        category.Add(name);
        //        category.Add(startage);
        //        category.Add(stopage);
        //        category.Add(color);

        //        root.Add(category);
        //    }

        //    xmlCategories.Add(root);
        //    xmlCategories.Save(filePath);
        //}
        #endregion

        #region Блок работы с людьми для сравнения
        /// <summary>
        /// Чтение людей для сравнения из XML-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<DtoComparedHuman> ReadComparedHumansFromFile(string filePath)
        {
            List<DtoComparedHuman> comparedHumans = new();
            var xmlComparedHumans = XDocument.Load(filePath);

            var root = xmlComparedHumans.Element("ComparedHumans");
            if (root != null)
            {
                foreach (XElement comparedHuman in root.Elements("ComparedHuman"))
                {
                    DtoComparedHuman ch = new()
                    {
                        ComparedHumanId           = Guid.Parse(comparedHuman.Attribute("id")!.Value),
                        ComparedHumanFullName     = comparedHuman.Element("name")!.Value,
                        ComparedHumanBirthDate    = DateTime.Parse(comparedHuman.Element("birthdate")!.Value),
                        IsComparedHumanConsidered = bool.Parse(comparedHuman.Element("isconsidered")!.Value)
                    };
                    comparedHumans.Add(ch);
                }
            }

            return comparedHumans;
        }

        /// <summary>
        /// Обновление/добавление человека для сравнения в файле
        /// </summary>
        /// <param name="comparedHuman"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public void UpdateComparedHumanInFile(DtoComparedHuman comparedHuman, string filePath)
        {
            var xmlComparedHumans = XDocument.Load(filePath);
            var root = xmlComparedHumans.Element("ComparedHumans");
            if (root != null)
            {
                var rootElements = root.Elements("ComparedHuman");
                var element = rootElements.FirstOrDefault(x => Guid.Parse(x.Attribute("id")!.Value) == comparedHuman.ComparedHumanId);
                if (element != null)
                {
                    element.Element("name")!.Value         = comparedHuman.ComparedHumanFullName;
                    element.Element("birthdate")!.Value    = comparedHuman.ComparedHumanBirthDate.ToString();
                    element.Element("isconsidered")!.Value = comparedHuman.IsComparedHumanConsidered.ToString();
                }
                else
                {
                    XElement comp = new XElement("ComparedHuman");
                    XAttribute id = new XAttribute("id", $"{comparedHuman.ComparedHumanId}");
                    XElement name = new XElement("name", $"{comparedHuman.ComparedHumanFullName}");
                    XElement birthdate = new XElement("birthdate", $"{comparedHuman.ComparedHumanBirthDate}");
                    XElement isconsidered = new XElement("isconsidered", $"{comparedHuman.IsComparedHumanConsidered}");

                    comp.Add(id);
                    comp.Add(name);
                    comp.Add(birthdate);
                    comp.Add(isconsidered);

                    root.Add(comp);
                }

                xmlComparedHumans.Save(filePath);
            }
        }

        /// <summary>
        /// Удаление человека для сравнения в файле
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public void DeleteComparedHumanInFile(string id, string filePath)
        {
            var xmlComparedHumans = XDocument.Load(filePath);
            var root = xmlComparedHumans.Element("ComparedHumans");
            if (root != null)
            {
                var rootElements = root.Elements("ComparedHuman");
                var element = rootElements.FirstOrDefault(x => x.Attribute("id")!.Value == id);
                if (element != null)
                {
                    element.Remove();
                    xmlComparedHumans.Save(filePath);
                }
            }
        }

        ///// <summary>
        ///// Временно - Сохранение всех людей для сравнения в файле
        ///// </summary>
        ///// <param name="comparedHumans"></param>
        ///// <param name="filePath"></param>
        ///// <returns></returns>
        //public bool SaveComparedHumansToFile(List<DtoComparedHuman> comparedHumans, string filePath)
        //{
        //    XDocument xmlComparedHumans = new();
        //    XElement root = new("ComparedHumans"); // Корневой элемент людей для сравнения

        //    foreach (var item in comparedHumans)
        //    {
        //        XElement comparedHuman = new XElement("ComparedHuman");
        //        XAttribute id          = new XAttribute("id", $"{item.ComparedHumanId}");
        //        XElement name          = new XElement("name", $"{item.ComparedHumanFullName}");
        //        XElement birthdate     = new XElement("birthdate", $"{item.ComparedHumanBirthDate}");
        //        XElement considered    = new XElement("isconsidered", $"{item.IsComparedHumanConsidered}");

        //        comparedHuman.Add(id);
        //        comparedHuman.Add(name);
        //        comparedHuman.Add(birthdate);
        //        comparedHuman.Add(considered);

        //        root.Add(comparedHuman);
        //    }

        //    xmlComparedHumans.Add(root);
        //    xmlComparedHumans.Save(filePath);

        //    return true;
        //}
        #endregion

        #region Блок работы с людьми
        /// <summary>
        /// Чтение основного списка людей из XML-файла
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<DtoHuman> ReadHumansFromFile(string filePath)
        {
            List<DtoHuman> humans = new();
            var xmlHumans = XDocument.Load(filePath);
            var root = xmlHumans.Element("Humans");
            if (root != null)
            {
                foreach (XElement hum in root.Elements("Human"))
                {
                    DtoHuman human = new DtoHuman()
                    {
                        HumanId        = Guid.Parse(hum.Attribute("id")!.Value),
                        FirstName      = hum.Element("firstname")!.Value,
                        LastName       = hum.Element("lastname")!.Value,
                        Patronymic     = hum.Element("patronymic")!.Value,
                        BirthDate      = DateTime.Parse(hum.Element("birthdate")!.Value),
                        BirthCountry   = hum.Element("birthcountry")!.Value,
                        BirthPlace     = hum.Element("birthplace")!.Value,
                        DeathDate      = DateTime.Parse(hum.Element("deathdate")!.Value),
                        DeathCountry   = hum.Element("deathcountry")!.Value,
                        DeathPlace     = hum.Element("deathplace")!.Value,
                        ImageFile      = hum.Element("image")!.Value,
                        DeathReasonId  = Guid.Parse(hum.Element("deathreason")!.Value),
                        HumanComments  = hum.Element("comments")!.Value,
                        DaysLived      = double.Parse(hum.Element("dayslived")!.Value),
                        FullYearsLived = int.Parse(hum.Element("fullyears")!.Value)
                    };
                    humans.Add(human);
                }
            }

            return humans;
        }

        public void UpdateHumanInFile(DtoHuman human, string filePath)
        {
            XDocument xmlHumans = XDocument.Load(filePath);
            XElement? root = xmlHumans.Element("Humans");
            if (root != null)
            {
                var rootElements = root.Elements("Human");
                var element = rootElements.FirstOrDefault(x => Guid.Parse(x.Attribute("id")!.Value) == human.HumanId);
                if (element != null)
                {
                    element.Element("firstname")!.Value    = human.FirstName;
                    element.Element("lastname")!.Value     = human.LastName;
                    element.Element("patronymic")!.Value   = human.Patronymic;
                    element.Element("birthdate")!.Value    = human.BirthDate.ToString();
                    element.Element("birthcountry")!.Value = human.BirthCountry;
                    element.Element("birthplace")!.Value   = human.BirthPlace;
                    element.Element("deathdate")!.Value    = human.DeathDate.ToString();
                    element.Element("deathcountry")!.Value = human.DeathCountry;
                    element.Element("deathplace")!.Value   = human.DeathPlace;
                    element.Element("image")!.Value        = human.ImageFile;
                    element.Element("deathreason")!.Value  = human.DeathReasonId.ToString();
                    element.Element("comments")!.Value     = human.HumanComments;
                    element.Element("dayslived")!.Value    = human.DaysLived.ToString();
                    element.Element("fullyears")!.Value    = human.FullYearsLived.ToString();
                }
                else
                {
                    XElement hum          = new XElement("Human");
                    XAttribute id         = new XAttribute("id", $"{human.HumanId}");
                    XElement firstname    = new XElement("firstname", $"{human.FirstName}");
                    XElement lastname     = new XElement("lastname", $"{human.LastName}");
                    XElement patronymic   = new XElement("patronymic", $"{human.Patronymic}");
                    XElement birthdate    = new XElement("birthdate", $"{human.BirthDate}");
                    XElement birthcountry = new XElement("birthcountry", $"{human.BirthCountry}");
                    XElement birthplace   = new XElement("birthplace", $"{human.BirthPlace}");
                    XElement deathdate    = new XElement("deathdate", $"{human.DeathDate}");
                    XElement deathcountry = new XElement("deathcountry", $"{human.DeathCountry}");
                    XElement deathplace   = new XElement("deathplace", $"{human.DeathPlace}");
                    XElement image        = new XElement("image", $"{human.ImageFile}");
                    XElement deathreason  = new XElement("deathreason", $"{human.DeathReasonId}");
                    XElement comments     = new XElement("comments", $"{human.HumanComments}");
                    XElement dayslived    = new XElement("dayslived", $"{human.DaysLived}");
                    XElement fullyears    = new XElement("fullyears", $"{human.FullYearsLived}");

                    hum.Add(id);
                    hum.Add(firstname);
                    hum.Add(lastname);
                    hum.Add(patronymic);
                    hum.Add(birthdate);
                    hum.Add(birthcountry);
                    hum.Add(birthplace);
                    hum.Add(deathdate);
                    hum.Add(deathcountry);
                    hum.Add(deathplace);
                    hum.Add(image);
                    hum.Add(deathreason);
                    hum.Add(comments);
                    hum.Add(dayslived);
                    hum.Add(fullyears);

                    root.Add(hum);
                }

                xmlHumans.Save(filePath);
            }
        }

        public void DeleteHumanInFile(string id, string filePath)
        {
            var xmlHumans = XDocument.Load(filePath);
            var root = xmlHumans.Element("Humans");
            if (root != null)
            {
                var rootElements = root.Elements("Human");
                var element = rootElements.FirstOrDefault(x => x.Attribute("id")!.Value == id);
                if (element != null)
                {
                    element.Remove();
                    xmlHumans.Save(filePath);
                }
            }
        }



        ///// <summary>
        ///// Временно - Сохранение всего основного списка людей в файле
        ///// </summary>
        ///// <param name="humans"></param>
        ///// <param name="filePath"></param>
        ///// <returns></returns>
        //public bool SaveHumansToFile(List<DtoHuman> humans, string filePath)
        //{
        //    XDocument xmlHumans = new();
        //    XElement root = new("Humans"); // Корневой элемент людей

        //    foreach (var item in humans)
        //    {
        //        XElement human        = new XElement("Human");
        //        XAttribute id         = new XAttribute("id", $"{item.HumanId}");
        //        XElement firstname    = new XElement("firstname", $"{item.FirstName}");
        //        XElement lastname     = new XElement("lastname", $"{item.LastName}");
        //        XElement patronymic   = new XElement("patronymic", $"{item.Patronymic}");
        //        XElement birthdate    = new XElement("birthdate", $"{item.BirthDate}");
        //        XElement birthcountry = new XElement("birthcountry", $"{item.BirthCountry}");
        //        XElement birthplace   = new XElement("birthplace", $"{item.BirthPlace}");
        //        XElement deathdate    = new XElement("deathdate", $"{item.DeathDate}");
        //        XElement deathcountry = new XElement("deathcountry", item.DeathCountry);
        //        XElement deathplace   = new XElement("deathplace", item.DeathPlace);
        //        XElement image        = new XElement("image", item.ImageFile);
        //        XElement deathreason  = new XElement("deathreason", item.DeathReasonId);
        //        XElement comments     = new XElement("comments", $"{item.HumanComments}");
        //        XElement dayslived    = new XElement("dayslived", $"{item.DaysLived}");
        //        XElement fullyears    = new XElement("fullyears", item.FullYearsLived);

        //        human.Add(id);
        //        human.Add(firstname);
        //        human.Add(lastname);
        //        human.Add(patronymic);
        //        human.Add(birthdate);
        //        human.Add(birthcountry);
        //        human.Add(birthplace);
        //        human.Add(deathdate);
        //        human.Add(deathcountry);
        //        human.Add(deathplace);
        //        human.Add(image);
        //        human.Add(deathreason);
        //        human.Add(comments);
        //        human.Add(dayslived);
        //        human.Add(fullyears);

        //        root.Add(human);
        //    }

        //    xmlHumans.Add(root);
        //    xmlHumans.Save(filePath);

        //    return true;
        //}
        #endregion
    }
}

#region Старый код - просто на память
//XElement upElement = new XElement("Reasons");

//for (int i = 0; i < reasons.Count; i++)
//{
//    XElement secElement = new XElement("Reason",
//                          new XElement("ReasonID", reasons[i].ReasonId),
//                          new XElement("ReasonName", reasons[i].ReasonName),
//                          new XElement("ReasonComment", reasons[i].ReasonComment),
//                          new XElement("ReasonDescription", reasons[i].ReasonDescription),
//                          new XElement("ParentReasonID", reasons[i].ReasonParentId));

//    upElement.Add(secElement);
//}

//xml.Add(upElement);
#endregion

//for(int i = 0; i < reasons.Count; i++)
//{
//    XElement reason      = new XElement("Reason");
//    XAttribute id        = new XAttribute("id", $"{reasons[i].ReasonId}");
//    XElement name        = new XElement("name", $"{reasons[i].ReasonName}");
//    XElement comment     = new XElement("comment", $"{reasons[i].ReasonComment}");
//    XElement description = new XElement("description", $"{reasons[i].ReasonDescription}");
//    XElement parent      = new XElement("parent", $"{reasons[i].ReasonParentId}");

//    reason.Add(id);
//    reason.Add(name);
//    reason.Add(comment);
//    reason.Add(description);
//    reason.Add(parent);

//    root.Add(reason);
//}

#region Старый красивый код, но уже устарел
//List<DtoReason> list = new();

//var root = xml.Element("Reasons");
//// Красивый вариант - вместо того, что ниже
//foreach (XElement item in root.Elements("Reason"))
//{
//    DtoReason reason = new()
//    {
//        ReasonId          = item.Element("ReasonID").Value,
//        ReasonName        = item.Element("ReasonName").Value,
//        ReasonComment     = item.Element("ReasonComment").Value,
//        ReasonDescription = item.Element("ReasonDescription").Value,
//        ReasonParentId    = item.Element("ParentReasonID").Value
//    };
//    reasons.Add(reason);
//}
//return reasons;
#endregion