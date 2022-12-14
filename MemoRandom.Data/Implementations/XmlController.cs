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
            for(int i = 0; i < reasons.Count; i++)
            {
                XElement reason = new XElement("Reason");
                XAttribute id = new XAttribute("id", $"{reasons[i].ReasonId}");
                XElement name = new XElement("name", $"{reasons[i].ReasonName}");
                XElement comment = new XElement("comment", $"{reasons[i].ReasonComment}");
                XElement description = new XElement("description", $"{reasons[i].ReasonDescription}");
                XElement parent = new XElement("parent", $"{reasons[i].ReasonParentId}");

                reason.Add(id);
                reason.Add(name);
                reason.Add(comment);
                reason.Add(description);
                reason.Add(parent);

                root.Add(reason);
            }
            xml.Add(root);
            #endregion

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

            xml.Save(filePath);

            return true;
        }

        public List<DtoReason> ReadReasonsFromFile(string filePath)
        {
            List<DtoReason> reasons = new();
            XDocument xml = XDocument.Load(filePath);

            #region Тестирование
            List<DtoReason> list = new();

            var root = xml.Element("Reasons");
            // Красивый вариант - вместо того, что ниже
            foreach (XElement item in root.Elements("Reason"))
            {
                DtoReason reason = new()
                {
                    ReasonId          = item.Element("ReasonID").Value,
                    ReasonName        = item.Element("ReasonName").Value,
                    ReasonComment     = item.Element("ReasonComment").Value,
                    ReasonDescription = item.Element("ReasonDescription").Value,
                    ReasonParentId    = item.Element("ParentReasonID").Value
                };
                reasons.Add(reason);
            }
            return reasons;



            #endregion

            DtoReason dtoReason = new();
            foreach(var node in xml.DescendantNodes())
            {
                if(node is XElement)
                {
                    var xElement = (XElement)node;

                    if (xElement.Name == "Reasons") continue;
                    if (xElement.Name == "Reason") continue;
                    if (xElement.Name == "ReasonID")
                    {
                        dtoReason.ReasonId = xElement.Value;
                        continue;
                    }
                    if (xElement.Name == "ReasonName")
                    {
                        dtoReason.ReasonName = xElement.Value;
                        continue;
                    }
                    if (xElement.Name == "ReasonComment")
                    {
                        dtoReason.ReasonComment = xElement.Value;
                        continue;
                    }
                    if (xElement.Name == "ReasonDescription")
                    {
                        dtoReason.ReasonDescription = xElement.Value;
                        continue;
                    }
                    if (xElement.Name == "ParentReasonID")
                    {
                        dtoReason.ReasonParentId = xElement.Value;
                        list.Add(dtoReason);
                        dtoReason = new();
                    }
                }
            }

            var r = 0;

            return list;
        }

        public bool AddReasonToList(DtoReason reason)
        {


            return true;
        }
    }
}
