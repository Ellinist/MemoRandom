using MemoRandom.Data.DbModels;
using MemoRandom.Data.DtoModels;
using MemoRandom.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
            XDocument xmlReasons = XDocument.Load(filePath); // Чтение из файла

            XElement? root = xmlReasons.Element("Reasons"); // Чтение корневого элемента
            if (root != null) // Если элемент существует
            {
                foreach (XElement reason in root.Elements("Reason")) // В цикле по всем вложенным элементам с типом "Reason"
                {
                    DtoReason rsn = new() // Создаем новый DTO-объект
                    {
                        ReasonId          = Guid.Parse(reason.Attribute("id").Value),
                        ReasonName        = reason.Element("name").Value,
                        ReasonComment     = reason.Element("comment").Value,
                        ReasonDescription = reason.Element("description").Value,
                        ReasonParentId    = Guid.Parse(reason.Element("parent").Value)
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
        public bool AddReasonToList(DtoReason reason, string filePath)
        {
            XDocument xmlReasons = XDocument.Load(filePath);
            XElement? root = xmlReasons.Element("Reasons");
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
            return true;
        }

        /// <summary>
        /// Изменение причины в файле
        /// </summary>
        /// <param name="rsn"></param>
        /// <param name="filePath"></param>
        public void ChangeReasonInFile(DtoReason rsn, string filePath)
        {
            XDocument xmlReasons = XDocument.Load(filePath);
            XElement? root = xmlReasons.Element("Reasons");
            if (root != null)
            {
                var l = root.Elements("Reason");
                var r1 = l.FirstOrDefault(x => Guid.Parse(x.Attribute("id").Value) == rsn.ReasonId);
                if(r1 != null)
                {
                    var name          = r1.Element("name");
                    name.Value        = rsn.ReasonName;
                    var comment       = r1.Element("comment");
                    comment.Value     = rsn.ReasonComment;
                    var description   = r1.Element("description");
                    description.Value = rsn.ReasonDescription;

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
            XDocument xmlReasons = XDocument.Load(filePath);
            XElement? root = xmlReasons.Element("Reasons");
            if (root != null)
            {
                var l = root.Elements("Reason");
                var r1 = l.FirstOrDefault(x => x.Attribute("id").Value == id);
                if(r1 != null)
                {
                    r1.Remove();
                    xmlReasons.Save(filePath);
                }
            }
        }

        /// <summary>
        /// Временно - Сохранение всех причин смерти в файле XML
        /// Пока что временно - а там посмотрим
        /// </summary>
        /// <param name="reasons"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool SaveReasonsToFile(List<DtoReason> reasons, string filePath)
        {
            XDocument xmlReasons = new(); // Создаем новый документ
            XElement root = new("Reasons"); // Корневой элемент причин смерти

            foreach (var item in reasons) // В цикле для всех причин смерти
            {
                XElement reason = new XElement("Reason"); // Заголовок причины
                XAttribute id = new XAttribute("id", $"{item.ReasonId}"); // И ее атрибут

                #region Создание вложенных элементов причины смерти
                XElement name = new XElement("name", $"{item.ReasonName}");
                XElement comment = new XElement("comment", $"{item.ReasonComment}");
                XElement description = new XElement("description", $"{item.ReasonDescription}");
                XElement parent = new XElement("parent", $"{item.ReasonParentId}");
                #endregion

                #region Добавление к заголовку его атрибута и дочерних элементов
                reason.Add(id);
                reason.Add(name);
                reason.Add(comment);
                reason.Add(description);
                reason.Add(parent);
                #endregion

                root.Add(reason);
            }

            xmlReasons.Add(root);
            xmlReasons.Save(filePath);

            return true;
        }
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
            XDocument xmlCategories = XDocument.Load(filePath);

            XElement? root = xmlCategories.Element("Categories");
            if (root != null)
            {
                foreach (XElement category in root.Elements("Category"))
                {
                    DtoCategory cat = new()
                    {
                        CategoryId   = Guid.Parse(category.Attribute("id").Value),
                        CategoryName = category.Element("name").Value,
                        StartAge     = category.Element("startage").Value,
                        StopAge      = category.Element("stopage").Value,
                        StringColor  = category.Element("color").Value
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
        public bool UpdateCategoryInFile(DtoCategory category, string filePath)
        {
            bool success = true;

            XDocument xmlCategories = XDocument.Load(filePath);
            XElement? root = xmlCategories.Element("Categories");
            if (root != null)
            {
                var l = root.Elements("Category");
                var r1 = l.FirstOrDefault(x => Guid.Parse(x.Attribute("id").Value) == category.CategoryId);
                if (r1 != null)
                {
                    var name       = r1.Element("name");
                    name.Value     = category.CategoryName;
                    var startage   = r1.Element("startage");
                    startage.Value = category.StartAge;
                    var stopage    = r1.Element("stopage");
                    stopage.Value  = category.StopAge;
                    var color      = r1.Element("color");
                    color.Value    = category.StringColor;
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

            return success;
        }

        /// <summary>
        /// Удаление категории из файла
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool DeleteCategoryInFile(string id, string filePath)
        {
            bool success = true;

            XDocument xmlCategories = XDocument.Load(filePath);
            XElement? root = xmlCategories.Element("Categories");
            if (root != null)
            {
                var l = root.Elements("Category");
                var r1 = l.FirstOrDefault(x => x.Attribute("id").Value == id);
                if (r1 != null)
                {
                    r1.Remove();
                    xmlCategories.Save(filePath);
                }
            }

            return success;
        }

        /// <summary>
        /// Временно - Сохранение списка категорий в файле XML
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool SaveCategoriesToFile(List<DtoCategory> categories, string filePath)
        {
            XDocument xmlCategories = new();
            XElement root = new("Categories"); // Корневой элемент категорий

            foreach (var item in categories)
            {
                XElement category = new XElement("Category");
                XAttribute id     = new XAttribute("id", $"{item.CategoryId}");
                XElement name     = new XElement("name", $"{item.CategoryName}");
                XElement startage = new XElement("startage", $"{item.StartAge}");
                XElement stopage  = new XElement("stopage", $"{item.StopAge}");
                XElement color    = new XElement("color", $"{item.StringColor}");

                category.Add(id);
                category.Add(name);
                category.Add(startage);
                category.Add(stopage);
                category.Add(color);

                root.Add(category);
            }

            xmlCategories.Add(root);
            xmlCategories.Save(filePath);

            return true;
        }
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
            XDocument xmlComparedHumans = XDocument.Load(filePath);

            XElement? root = xmlComparedHumans.Element("ComparedHumans");
            if (root != null)
            {
                foreach (XElement comparedHuman in root.Elements("ComparedHuman"))
                {
                    DtoComparedHuman ch = new()
                    {
                        ComparedHumanId           = Guid.Parse(comparedHuman.Attribute("id").Value),
                        ComparedHumanFullName     = comparedHuman.Element("name").Value,
                        ComparedHumanBirthDate    = DateTime.Parse(comparedHuman.Element("birthdate").Value),
                        IsComparedHumanConsidered = bool.Parse(comparedHuman.Element("isconsidered").Value)
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
        public bool UpdateComparedHumanInFile(DtoComparedHuman comparedHuman, string filePath)
        {
            bool success = true;

            XDocument xmlComparedHumans = XDocument.Load(filePath);
            XElement? root = xmlComparedHumans.Element("ComparedHumans");
            if (root != null)
            {
                var l = root.Elements("ComparedHuman");
                var r1 = l.FirstOrDefault(x => Guid.Parse(x.Attribute("id").Value) == comparedHuman.ComparedHumanId);
                if (r1 != null)
                {
                    var name = r1.Element("name");
                    name.Value = comparedHuman.ComparedHumanFullName;
                    var birthdate = r1.Element("birthdate");
                    birthdate.Value = comparedHuman.ComparedHumanBirthDate.ToString();
                    var isconsidered = r1.Element("isconsidered");
                    isconsidered.Value = comparedHuman.IsComparedHumanConsidered.ToString();
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

            return success;
        }

        /// <summary>
        /// Удаление человека для сравнения в файле
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool DeleteComparedHumanInFile(string id, string filePath)
        {
            bool success = true;

            XDocument xmlComparedHumans = XDocument.Load(filePath);
            XElement? root = xmlComparedHumans.Element("ComparedHumans");
            if (root != null)
            {
                var l = root.Elements("ComparedHuman");
                var r1 = l.FirstOrDefault(x => x.Attribute("id").Value == id);
                if (r1 != null)
                {
                    r1.Remove();
                    xmlComparedHumans.Save(filePath);
                }
            }

            return success;
        }

        /// <summary>
        /// Временно - Сохранение всех людей для сравнения в файле
        /// </summary>
        /// <param name="comparedHumans"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool SaveComparedHumansToFile(List<DtoComparedHuman> comparedHumans, string filePath)
        {
            XDocument xmlComparedHumans = new();
            XElement root = new("ComparedHumans"); // Корневой элемент людей для сравнения

            foreach (var item in comparedHumans)
            {
                XElement comparedHuman = new XElement("ComparedHuman");
                XAttribute id          = new XAttribute("id", $"{item.ComparedHumanId}");
                XElement name          = new XElement("name", $"{item.ComparedHumanFullName}");
                XElement birthdate     = new XElement("birthdate", $"{item.ComparedHumanBirthDate}");
                XElement considered    = new XElement("isconsidered", $"{item.IsComparedHumanConsidered}");

                comparedHuman.Add(id);
                comparedHuman.Add(name);
                comparedHuman.Add(birthdate);
                comparedHuman.Add(considered);

                root.Add(comparedHuman);
            }

            xmlComparedHumans.Add(root);
            xmlComparedHumans.Save(filePath);

            return true;
        }
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
            XDocument xmlHumans = XDocument.Load(filePath);

            XElement? root = xmlHumans.Element("Humans");
            if (root != null)
            {
                foreach (XElement hum in root.Elements("Human"))
                {
                    DtoHuman human = new DtoHuman()
                    {
                        HumanId        = Guid.Parse(hum.Attribute("id").Value),
                        FirstName      = hum.Element("firstname").Value,
                        LastName       = hum.Element("lastname").Value,
                        Patronymic     = hum.Element("patronymic").Value,
                        BirthDate      = DateTime.Parse(hum.Element("birthdate").Value),
                        BirthCountry   = hum.Element("birthcountry").Value,
                        BirthPlace     = hum.Element("birthplace").Value,
                        DeathDate      = DateTime.Parse(hum.Element("deathdate").Value),
                        DeathCountry   = hum.Element("deathcountry").Value,
                        DeathPlace     = hum.Element("deathplace").Value,
                        ImageFile      = hum.Element("image").Value,
                        DeathReasonId  = Guid.Parse(hum.Element("deathreason").Value),
                        HumanComments  = hum.Element("comments").Value,
                        DaysLived      = double.Parse(hum.Element("dayslived").Value),
                        FullYearsLived = int.Parse(hum.Element("fullyears").Value)
                    };
                    humans.Add(human);
                }
            }

            return humans;
        }

        public bool UpdateHumanInFile(DtoHuman human, string filePath)
        {
            bool success = true;

            XDocument xmlHumans = XDocument.Load(filePath);
            XElement? root = xmlHumans.Element("Humans");
            if (root != null)
            {
                var l = root.Elements("Human");
                var r1 = l.FirstOrDefault(x => Guid.Parse(x.Attribute("id").Value) == human.HumanId);
                if (r1 != null)
                {
                    var firstname = r1.Element("firstname");
                    firstname.Value = human.FirstName;
                    var lastname = r1.Element("lastname");
                    lastname.Value = human.LastName;
                    var patronymic = r1.Element("patronymic");
                    patronymic.Value = human.Patronymic;
                    var birthdate = r1.Element("birthdate");
                    birthdate.Value = human.BirthDate.ToString();
                    var birthcountry = r1.Element("birthcountry");
                    birthcountry.Value = human.BirthCountry;
                    var birthplace = r1.Element("birthplace");
                    birthplace.Value = human.BirthPlace;
                    var deathdate = r1.Element("deathdate");
                    deathdate.Value = human.DeathDate.ToString();
                    var deathcountry = r1.Element("deathcountry");
                    deathcountry.Value = human.DeathCountry;
                    var deathplace = r1.Element("deathplace");
                    deathplace.Value = human.DeathPlace;
                    var image = r1.Element("image");
                    image.Value = human.ImageFile;
                    var deathreason = r1.Element("deathreason");
                    deathreason.Value = human.DeathReasonId.ToString();
                    var comments = r1.Element("comments");
                    comments.Value = human.HumanComments;
                    var dayslived = r1.Element("dayslived");
                    dayslived.Value = human.DaysLived.ToString();
                    var fullyears = r1.Element("fullyears");
                    fullyears.Value = human.FullYearsLived.ToString();
                }
                else
                {
                    XElement hum = new XElement("Human");
                    XAttribute id = new XAttribute("id", $"{human.HumanId}");
                    XElement firstname = new XElement("firstname", $"{human.FirstName}");
                    XElement lastname = new XElement("lastname", $"{human.LastName}");
                    XElement patronymic = new XElement("patronymic", $"{human.Patronymic}");
                    XElement birthdate = new XElement("birthdate", $"{human.BirthDate}");
                    XElement birthcountry = new XElement("birthcountry", $"{human.BirthCountry}");
                    XElement birthplace = new XElement("birthplace", $"{human.BirthPlace}");
                    XElement deathdate = new XElement("deathdate", $"{human.DeathDate}");
                    XElement deathcountry = new XElement("deathcountry", $"{human.DeathCountry}");
                    XElement deathplace = new XElement("deathplace", $"{human.DeathPlace}");
                    XElement image = new XElement("image", $"{human.ImageFile}");
                    XElement deathreason = new XElement("deathreason", $"{human.DeathReasonId}");
                    XElement comments = new XElement("comments", $"{human.HumanComments}");
                    XElement dayslived = new XElement("dayslived", $"{human.DaysLived}");
                    XElement fullyears = new XElement("fullyears", $"{human.FullYearsLived}");

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

            return success;
        }

        public bool DeleteHumanInFile(string id, string filePath)
        {
            bool success = true;

            XDocument xmlHumans = XDocument.Load(filePath);
            XElement? root = xmlHumans.Element("Humans");
            if (root != null)
            {
                var l = root.Elements("Human");
                var r1 = l.FirstOrDefault(x => x.Attribute("id").Value == id);
                if (r1 != null)
                {
                    r1.Remove();
                    xmlHumans.Save(filePath);
                }
            }

            return success;
        }



        /// <summary>
        /// Временно - Сохранение всего основного списка людей в файле
        /// </summary>
        /// <param name="humans"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool SaveHumansToFile(List<DtoHuman> humans, string filePath)
        {
            XDocument xmlHumans = new();
            XElement root = new("Humans"); // Корневой элемент людей

            foreach (var item in humans)
            {
                XElement human        = new XElement("Human");
                XAttribute id         = new XAttribute("id", $"{item.HumanId}");
                XElement firstname    = new XElement("firstname", $"{item.FirstName}");
                XElement lastname     = new XElement("lastname", $"{item.LastName}");
                XElement patronymic   = new XElement("patronymic", $"{item.Patronymic}");
                XElement birthdate    = new XElement("birthdate", $"{item.BirthDate}");
                XElement birthcountry = new XElement("birthcountry", $"{item.BirthCountry}");
                XElement birthplace   = new XElement("birthplace", $"{item.BirthPlace}");
                XElement deathdate    = new XElement("deathdate", $"{item.DeathDate}");
                XElement deathcountry = new XElement("deathcountry", item.DeathCountry);
                XElement deathplace   = new XElement("deathplace", item.DeathPlace);
                XElement image        = new XElement("image", item.ImageFile);
                XElement deathreason  = new XElement("deathreason", item.DeathReasonId);
                XElement comments     = new XElement("comments", $"{item.HumanComments}");
                XElement dayslived    = new XElement("dayslived", $"{item.DaysLived}");
                XElement fullyears    = new XElement("fullyears", item.FullYearsLived);

                human.Add(id);
                human.Add(firstname);
                human.Add(lastname);
                human.Add(patronymic);
                human.Add(birthdate);
                human.Add(birthcountry);
                human.Add(birthplace);
                human.Add(deathdate);
                human.Add(deathcountry);
                human.Add(deathplace);
                human.Add(image);
                human.Add(deathreason);
                human.Add(comments);
                human.Add(dayslived);
                human.Add(fullyears);

                root.Add(human);
            }

            xmlHumans.Add(root);
            xmlHumans.Save(filePath);

            return true;
        }
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