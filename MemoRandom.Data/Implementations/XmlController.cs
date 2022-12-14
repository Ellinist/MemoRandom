using MemoRandom.Data.DtoModels;
using MemoRandom.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MemoRandom.Data.Implementations
{
    public class XmlController : IXmlController
    {
        public bool SaveReasonsToFile(List<DtoReason> reasons, string filePath)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XDocument xml = new XDocument();

            #region Тестирование новой схемы
            XElement root = new XElement("Reasons");

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
            xml.Add(root);
            #endregion

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

            xml.Save(filePath);

            return true;
        }

        public List<DtoReason> ReadReasonsFromFile(string filePath)
        {
            List<DtoReason> reasons = new();
            XDocument xml = XDocument.Load(filePath);

            XElement? root = xml.Element("Reasons");
            if(root != null)
            {
                foreach(XElement reason in root.Elements("Reason"))
                {
                    DtoReason rsn = new DtoReason()
                    {
                        ReasonId          = reason.Attribute("id").Value,
                        ReasonName        = reason.Element("name").Value,
                        ReasonComment     = reason.Element("comment").Value,
                        ReasonDescription = reason.Element("description").Value,
                        ReasonParentId    = reason.Element("parent").Value
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
    }
}
