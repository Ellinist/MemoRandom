﻿using MemoRandom.Data.DtoModels;
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
        /// Сохранение всех причин смерти в файле XML
        /// Пока что временно - а там посмотрим
        /// </summary>
        /// <param name="reasons"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool SaveReasonsToFile(List<DtoReason> reasons, string filePath)
        {
            XDocument xmlReasons = new(); // Создаем нвоый документ
            XElement root = new("Reasons"); // Корневой элемент причин смерти

            foreach (var item in reasons) // В цикле для всех причин смерти
            {
                XElement reason      = new XElement("Reason"); // Заголовок причины
                XAttribute id        = new XAttribute("id", $"{item.ReasonId}"); // И ее атрибут

                #region Создание вложенных элементов причины смерти
                XElement name        = new XElement("name", $"{item.ReasonName}");
                XElement comment     = new XElement("comment", $"{item.ReasonComment}");
                XElement description = new XElement("description", $"{item.ReasonDescription}");
                XElement parent      = new XElement("parent", $"{item.ReasonParentId}");
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

        public bool AddReasonToList(DtoReason reason, string filePath)
        {
            XDocument xmlReasons = XDocument.Load(filePath);
            XElement? root = xmlReasons.Element("Reasons");
            if (root != null)
            {
                XElement rsn = new XElement("Reason");
                XAttribute id = new XAttribute("id", $"{reason.ReasonId}");
                XElement name = new XElement("name", $"{reason.ReasonName}");
                XElement comment = new XElement("comment", $"{reason.ReasonComment}");
                XElement description = new XElement("description", $"{reason.ReasonDescription}");
                XElement parent = new XElement("parent", $"{reason.ReasonParentId}");

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
                    var name = r1.Element("name");
                    name.Value = rsn.ReasonName;
                    var comment = r1.Element("comment");
                    comment.Value = rsn.ReasonComment;
                    var description = r1.Element("description");
                    description.Value = rsn.ReasonDescription;

                    xmlReasons.Save(filePath);
                }

                //r1.Element("name") = rsn.ReasonName;
            }
        }

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
        #endregion

        #region Блок работы с категориями возрастов
        /// <summary>
        /// Сохранение списка категорий в файле XML
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
        #endregion

        #region Блок работы с людьми для сравнения
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
        #endregion

        #region Блок работы с людьми
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