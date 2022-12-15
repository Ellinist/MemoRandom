﻿using MemoRandom.Data.DtoModels;
using MemoRandom.Data.Interfaces;
using System.Collections.Generic;
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
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XDocument xmlReasons = new();
            XElement root = new("Reasons"); // Корневой элемент причин смерти

            foreach (var item in reasons)
            {
                XElement reason      = new XElement("Reason");
                XAttribute id        = new XAttribute("id", $"{item.ReasonId}");
                XElement name        = new XElement("name", $"{item.ReasonName}");
                XElement comment     = new XElement("comment", $"{item.ReasonComment}");
                XElement description = new XElement("description", $"{item.ReasonDescription}");
                XElement parent      = new XElement("parent", $"{item.ReasonParentId}");

                reason.Add(id);
                reason.Add(name);
                reason.Add(comment);
                reason.Add(description);
                reason.Add(parent);

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
            List<DtoReason> reasons = new();
            XDocument xml = XDocument.Load(filePath);

            XElement? root = xml.Element("Reasons");
            if (root != null)
            {
                foreach (XElement reason in root.Elements("Reason"))
                {
                    DtoReason rsn = new DtoReason()
                    {
                        ReasonId = reason.Attribute("id").Value,
                        ReasonName = reason.Element("name").Value,
                        ReasonComment = reason.Element("comment").Value,
                        ReasonDescription = reason.Element("description").Value,
                        ReasonParentId = reason.Element("parent").Value
                    };
                    reasons.Add(rsn);
                }
            }

            return reasons;
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
        }

        public bool AddReasonToList(DtoReason reason)
        {


            return true;
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
        #endregion

        #region Блок работы с людьми для сравнения
        public bool SaveComparedHumansToFile(List<DtoComparedHuman> comparedHumans, string filePath)
        {
            return true;
        }
        #endregion

        #region Блок работы с людьми

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
