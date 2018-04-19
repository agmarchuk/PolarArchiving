using OpenArchive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenArchiveMVC.Models
{
    public class PortraitPersonModel
    {
        public string id;
        public string type_id = "http://fogid.net/o/person";
        public string name;
        public string dates;
        public string description;
        public IEnumerable<XElement> titles;
        public IEnumerable<XElement> works;
        public IEnumerable<XElement> notworks;
        public IEnumerable<XElement> livings;
        public IEnumerable<XElement> auth;
        public IEnumerable<XElement> refl_docs;

        private static XElement format = new XElement("record", new XAttribute("type", "http://fogid.net/o/person"),
            // степени, звания, награды
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/has-title"),
                new XElement("record", new XAttribute("type", "http://fogid.net/o/titled"),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/degree")),
                    null)),
            // Участие
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/participant"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/role-classification")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-org"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/org-classification")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            // Проживание
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/something"),
                new XElement("record",
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/location-place"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            // Авторство документов
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/author"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/authority-specificator")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/adoc"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                            null)))),
            // Отражение в документах
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                            null)))),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
            null);


        public PortraitPersonModel(string id)
        {
            this.id = id;

            XElement item = OpenArchive.StaticObjects.engine.GetItemById(id, format);
            if (item == null) { return; }
            // Вытягиваем информацию
            var name_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name");
            this.name = name_el == null ? "noname" : name_el.Value;
            // Описание
            var desc_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/description");
            this.description = desc_el == null ? "" : desc_el.Value;
            // Даты
            string _dates = OpenArchive.StaticObjects.GetDates(item);
            this.dates = string.IsNullOrEmpty(_dates) ? "" : "(" + _dates + ")";
            // Титулы
            titles = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/has-title")
                .Select(inv => inv.Element("record"));
            // Участие и работа
            var participations = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/participant")
                .Select(inv => inv.Element("record"));
            works = participations.Where(re =>
            {
                XElement org = re.Element("direct").Element("record");
                string org_classification = OpenArchive.StaticObjects.GetField(org, "http://fogid.net/o/role-classification");
                return string.IsNullOrEmpty(org_classification) || org_classification == "organization";
            });
            notworks = participations.Where(re =>
            {
                XElement org = re.Element("direct").Element("record");
                string org_classification = OpenArchive.StaticObjects.GetField(org, "http://fogid.net/o/role-classification");
                return !string.IsNullOrEmpty(org_classification) && org_classification != "organization";
            });
            // Проживание
            livings = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/something")
                .Select(inv => inv.Element("record").Element("direct").Element("record"));
            // Авторство
            auth = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/author")
                .Select(inv => inv.Element("record"))
                //.Where(rec => OpenArchive.StaticObjects.GetField(rec, "http://fogid.net/o/authority-specificator") == "author")
                //.Select(rec => rec.Element("direct").Element("record"))
                ;
            // Документы, отражающие 
            refl_docs = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/reflected")
                .Select(inv => inv.Element("record"))
                .Select(rec => rec.Element("direct").Element("record"))
                ;

        }
    }
    public class PortraitCollectionModel
    {
        private static XElement format = new XElement("record", new XAttribute("type", "http://fogid.net/o/collection"),
            // подколлекции и документы
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-collection"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/pageNumbers")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/collection-item"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/forDocument"),
                                new XElement("record",
                                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")))),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                            // Авторы
                            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/adoc"),
                                new XElement("record",
                                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/authority-specificator")),
                                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/author"),
                                        new XElement("record",
                                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                                            null)))),
                            null)))),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")));

        public string id, name, description;
        public IEnumerable<XElement> subcollections, documents, multimedia;
        public XElement look = null;
        public PortraitCollectionModel(string id)
        {
            if (id == null) { return; }
            this.id = id;
            XElement item = StaticObjects.engine.GetItemById(id, format);

            // Вытягиваем информацию
            var name_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name");
            name = name_el == null ? "noname" : name_el.Value;
            // Описание коллекции
            var desc_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/description");
            description = desc_el == null ? "" : desc_el.Value;
            // Нахождение подколлекций и набора документов
            var c_members = item.Elements("inverse")
                .Select(inv => inv.Element("record"))
                .OrderBy(rec =>
                {
                    XElement pages = rec.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/pageNumbers");
                    return pages == null ? "ZZZZZ" : pages.Value;
                })
                .ThenBy(rec =>
                {
                    XElement nm = rec.Element("direct")?.Element("record")?.Elements("field")
                        .FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name");
                    return nm == null ? "" : nm.Value;
                })
                .Select(rec => rec.Element("direct")?.Element("record"));
            subcollections = c_members
                .Where(m => m != null && m.Attribute("type") != null && m.Attribute("type").Value == "http://fogid.net/o/collection");
            documents = c_members
                .Where(m => m != null && m.Attribute("type") != null && m.Attribute("type").Value == "http://fogid.net/o/document");
            multimedia = c_members
                .Where(m => m != null && m.Attribute("type") != null && m.Attribute("type").Value == "http://fogid.net/o/photo-doc");

            //look = item;
        }
    }
    public class PortraitOrgModel
    {
        public string id, name, description, dates;
        public bool isorg;
        public IEnumerable<XElement> participants, locations, auth, reflections;
        public PortraitOrgModel(string id)
        {
            this.id = id;

            XElement item = StaticObjects.engine.GetItemById(id, format);
            if (item == null) { return; }
            // Вытягиваем информацию
            var name_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name");
            name = name_el == null ? "noname" : name_el.Value;
            // Описание
            var desc_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/description");
            description = desc_el == null ? "" : desc_el.Value;
            // Даты
            string _dates = StaticObjects.GetDates(item);
            dates = string.IsNullOrEmpty(_dates) ? "" : "(" + _dates + ")";
            // Классификатор
            var oclassification = StaticObjects.GetField(item, "http://fogid.net/o/rg-classification");
            isorg = oclassification != null && oclassification == "organization";
            // Участие или работа
            participants = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/in-org")
                .Select(inv => inv.Element("record"));
            // Расположение
            locations = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/something")
                .Select(inv => inv.Element("record")?.Element("direct")?.Element("record"));
            // Авторы/абоненты  Авторство
            auth = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/author")
                .Select(inv => inv.Element("record")?.Element("direct")?.Element("record"));
            // Отражения
            reflections = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/reflected")
                .Select(inv => inv.Element("record")?.Element("direct")?.Element("record"));

        }
        private static XElement format = new XElement("record", new XAttribute("type", "http://fogid.net/o/org-sys"),
            // Участники
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-org"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/role-classification")),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/role")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/participant"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            // Расположение
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/something"),
                new XElement("record",
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/location-place"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            // Абонент документов
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/author"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/authority-specificator")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/adoc"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                            null)))),
            // Отражение в документах
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/ground")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                            null)))),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/org-classification")),
            null);
    }
    public class PortraitDocumentModel
    {
        public string id, typeid, name, description, date, doccontent, uri, contenttype;
        public IEnumerable<XElement> parts, reflections, authors, recipients, geoplaces, collections;
        public XElement infosource, descr_infosource;
        public PortraitDocumentModel(string id)
        {
            this.id = id;
            if (id == null) { return; }
            XElement item = StaticObjects.engine.GetItemById(id, format);
            typeid = item.Attribute("id").Value;
            if (item == null) { return; }
            // Вытягиваем информацию
            var name_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name");
            name = name_el == null ? "noname" : name_el.Value;
            // Описание
            var desc_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/description");
            description = desc_el == null ? "" : desc_el.Value;
            // Даты
            date = StaticObjects.GetField(item, "http://fogid.net/o/from-date");
            doccontent = StaticObjects.GetField(item, "http://fogid.net/o/doc-content");
            uri = StaticObjects.GetField(item, "http://fogid.net/o/uri");
            var docmetainfo = StaticObjects.GetField(item, "http://fogid.net/o/docmetainfo");
            contenttype = docmetainfo == null ? "" : (docmetainfo.Split(';')
                .Where(pair => pair.StartsWith("documenttype:"))
                .Select(pair => pair.Substring("documenttype:".Length)).FirstOrDefault());
            // Части документа
            parts = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/inDocument")
                .Select(inv => inv.Element("record"))
                .Select(rec => new
                {
                    docpart = rec.Element("direct")?.Element("record"),
                    page = StaticObjects.GetField(rec, "http://fogid.net/o/pageNumbers")
                })
                .OrderBy(pair => pair.page)
                .ThenBy(pair => pair.docpart == null ? "" : StaticObjects.GetField(pair.docpart, "http://fogid.net/o/name"))
                .Select(pair => pair.docpart);
            // Отражения
            reflections = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/in-doc")
                .Select(inv => inv.Element("record")?.Element("direct")?.Element("record"));
            // Авторы и получатели
            var authorities = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/adoc")
                .Select(inv => inv.Element("record"));
            // Дополнения от Фурсенко
            authors = authorities
                .Where(rec => ((StaticObjects.GetField(rec, "http://fogid.net/o/authority-specificator") == "author")
                || (StaticObjects.GetField(rec, "http://fogid.net/o/authority-specificator") == "out-org")))
                .Select(rec => rec.Element("direct")?.Element("record"));
            recipients = authorities
                .Where(rec => ((StaticObjects.GetField(rec, "http://fogid.net/o/authority-specificator") == "recipient")
                || (StaticObjects.GetField(rec, "http://fogid.net/o/authority-specificator") == "in-org")))
                .Select(rec => rec.Element("direct")?.Element("record"));
            // Геоинформация    
            geoplaces = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/something")
                .Select(inv => inv.Element("record")?.Element("direct")?.Element("record"));
            // Источник поступления
            var fromarchivemember = item.Elements("inverse")
                .FirstOrDefault(inv => inv.Attribute("prop").Value == "http://fogid.net/o/archive-item");
            XElement toinfosource = fromarchivemember == null ? null :
                fromarchivemember.Element("record").Elements("direct")
                .FirstOrDefault(di => di.Attribute("prop").Value == "http://fogid.net/o/info-source");
            infosource = toinfosource == null ? null :
                toinfosource.Element("record");
            // Дополнительная информация про источник поступления
            descr_infosource = fromarchivemember == null ? null :
                fromarchivemember.Element("record")?.Elements("field")
                .FirstOrDefault(di => di.Attribute("prop").Value == "http://fogid.net/o/description");

            // Элемент коллекций    
            collections = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/collection-item")
                .Select(inv => inv.Element("record")?.Element("direct")?.Element("record"));

        }
        private static XElement format = new XElement("record", new XAttribute("type", "http://fogid.net/o/document"),
            // Части документа
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/inDocument"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/pageNumbers")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/partItem"),
                        new XElement("record", new XAttribute("type", "http://fogid.net/o/photo-doc"),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo"))
                            )),
                    null)),
            // Отражения
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                new XElement("record",
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/reflected"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            // Авторство документов
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/adoc"),
                new XElement("record",
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/authority-specificator")),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/author"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            // Геоинформация
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/something"),
                new XElement("record",
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/location-place"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            // Архив, источник
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/archive-item"),
                new XElement("record", new XAttribute("type", "http://fogid.net/o/archive-member"),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-archive"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))),
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/info-source"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))),
                    new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                    null)),
            // В коллекциях
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/collection-item"),
                new XElement("record",
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-collection"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")))))),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/doc-content")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/uri")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/docmetainfo")),
            null);
    }
    public class PortraitGeoModel
    {
        public string name, description, dates;
        public IEnumerable<XElement> reflections, locations, locations_person, locations_org, locations_doc;
        public PortraitGeoModel(string id)
        {
            XElement item = StaticObjects.engine.GetItemById(id, format);
            if (item == null) { return; }
            // Вытягиваем информацию
            var name_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/name");
            name = name_el == null ? "noname" : name_el.Value;
            // Описание
            var desc_el = item.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/description");
            description = desc_el == null ? "" : desc_el.Value;
            // Даты
            string _dates = StaticObjects.GetDates(item);
            dates = string.IsNullOrEmpty(_dates) ? "" : "(" + _dates + ")";
            // Отражения
            reflections = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/reflected")
                .Select(inv => inv.Element("record").Element("direct").Element("record"));
            // Расположение
            locations = item.Elements("inverse")
                .Where(inv => inv.Attribute("prop").Value == "http://fogid.net/o/location-place")
                .Select(inv => inv.Element("record").Element("direct").Element("record"));
            locations_person = locations
                .Where(rec => rec.Attribute("type").Value == "http://fogid.net/o/person");
            locations_org = locations
                .Where(rec => rec.Attribute("type").Value == "http://fogid.net/o/org-sys");
            locations_doc = locations
                .Where(rec => rec.Attribute("type").Value == "http://fogid.net/o/document");

        }
        private static XElement format = new XElement("record",
            // Отражение в документе
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/reflected"),
                new XElement("record",
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/in-doc"),
                        new XElement("record", new XAttribute("type", "http://fogid.net/o/document"),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                            null)))),
            // Расположение сущностей разного типа
            new XElement("inverse", new XAttribute("prop", "http://fogid.net/o/location-place"),
                new XElement("record",
                    new XElement("direct", new XAttribute("prop", "http://fogid.net/o/something"),
                        new XElement("record",
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
                            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
                            null)))),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/name")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/from-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/to-date")),
            new XElement("field", new XAttribute("prop", "http://fogid.net/o/description")),
            null);

    }
}
