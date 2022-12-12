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

            XDocument xDocument = new XDocument();
            XElement upElement = new XElement("Reasons");

            for (int i = 0; i < reasons.Count; i++)
            {
                XElement secElement = new XElement("Reason",
                                      new XElement("ReasonID", reasons[i].ReasonId),
                                      new XElement("ReasonName", reasons[i].ReasonName),
                                      new XElement("ReasonComment", reasons[i].ReasonComment),
                                      new XElement("ReasonDescription", reasons[i].ReasonDescription),
                                      new XElement("ParentReasonID", reasons[i].ReasonParentId));

                upElement.Add(secElement);
            }

            xDocument.Add(upElement);

            xDocument.Save(filePath);

            return true;
        }

        public List<DtoReason> ReadReasonsFromFile(string filePath)
        {
            List<DtoReason> reasons = new();
            XDocument xDocument = XDocument.Load(filePath);

            var xmlList = (from s in xDocument.Descendants("Reason") select new
            {
                id = s.Descendants("ReasonId").SingleOrDefault(),
                name = s.Descendants("ReasonName").SingleOrDefault()
            }).ToList();

            foreach (var item in xmlList)
            {
                var d = item.id;
                var dd = item.name;
            }

            //var rr = xDocument.Elements("Reasons");

            //var pp = rr.Nodes().ToList();

            //foreach (var item in pp)
            //{
            //    //var g = item.Annotation;
            //}


            //foreach (var item in rr)
            //{
            //    var z = item.Name;

            //    var x = item.Elements("Reason");

            //    foreach (var ii in x)
            //    {
            //        var zz = ii.Name;
            //        var zzz = ii;
            //        var xxx = ii.Element(zz);
            //        var ccc = ii.Attribute("ReasonId");





            //        var q1 = ii.Element("ReasonId").Value;
            //        var q2 = ii.Element("ReasonName").Value;
            //        var q3 = ii.Element("ReasonComment").Value;

            //        //reas.ReasonId = ii.Element("ReasonId").Value;
            //        //reas.ReasonName = ii.Element("ReasonName").Value;
            //        //reas.ReasonComment = ii.Element("ReasonComment").Value;
            //    }
            //}

            return null;
        }

        public bool AddReasonToList(DtoReason reason)
        {


            return true;
        }
    }
}
