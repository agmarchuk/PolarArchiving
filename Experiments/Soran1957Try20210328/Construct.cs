using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace Soran1957Try20210328
{
    public class Construct
    {
        public string look = "?";
        string home_id = "w20070417_7_1744";
        string soran_id = "w20070417_3_6186";
        string database_id = "w20070417_4_1010";
        string about_id = "w20070417_3_4658";
        string photocollections = "w20070417_4_1012";
        string sitecollection = "w20070417_4_1027";
        string usefullLinks_id = "usefullLinks";
        string newspaper_id = "newspaper_cassetteId";

        IQueryCollection pars; ISession session;
        public Construct(IQueryCollection pars, ISession session)
        { this.pars = pars; this.session = session; }

        public XElement ConstructPage()
        {
            string id = pars["sdirection"];

            string sdirection = pars["sdirection"];
            if (sdirection == null) sdirection = session.GetString("sdirection");
            if (sdirection == null) sdirection = "person";
            session.SetString("sdirection", sdirection);

            XElement centralPanel = new XElement("div");
            XElement rightPanel = new XElement("div");

            string searchstring = (string)pars["searchstring"];
            if (searchstring != null) BuildSearchResults(centralPanel, rightPanel, searchstring, sdirection);
            else BuildItem(centralPanel, rightPanel, string.IsNullOrWhiteSpace(pars["id"]) ? home_id : pars["id"]);

            XElement logo =
    new XElement("div", new XAttribute("id", "logo"), //new XAttribute("style", style_logo),
        new XAttribute("width", "500"), new XAttribute("height", "99"),
        new XElement("a", new XAttribute("href", ""),
            new XElement("img", new XAttribute("src", "/img/logo2.gif"),
                new XAttribute("width", "391"), new XAttribute("height", "61"),
                new XAttribute("vspace", "12"), new XAttribute("alt", "СО РАН с 1957 года"),
                new XAttribute("border", "0"))),
        null);

            XElement login =
    new XElement("div", new XAttribute("id", "login"), //new XAttribute("style", style_logi),
        new XElement("table", new XAttribute("cellpadding", "0"), new XAttribute("cellspacing", "0"),
            new XAttribute("border", "0"), new XAttribute("width", "100%"),
            new XElement("tr",
                new XElement("td", new XAttribute("class", "text_arial_small"), "имя: "),
                new XElement("td", new XAttribute("width", "40%"),
                    new XElement("input", new XAttribute("type", "text"), new XAttribute("class", "form_login"))),
                new XElement("td", new XAttribute("class", "text_arial_small"), "пароль: "),
                new XElement("td", new XAttribute("width", "40%"),
                    new XElement("input", new XAttribute("type", "password"), new XAttribute("class", "form_login"))),
                new XElement("td",
                    new XElement("input", new XAttribute("type", "image"), new XAttribute("width", "19"), new XAttribute("height", "19"), new XAttribute("src", "/img/btn_login.gif")))),
            new XElement("tr", new XElement("td"),
                new XElement("td", new XAttribute("colspan", "4"), new XAttribute("class", "text_arial_small"),
                    new XElement("a", "регистрация"),
                    new XText(" | "),
                    new XElement("a", "забыли пароль?"))),
            null),
        null);

            XElement menu =
    new XElement("div", new XAttribute("id", "menu"), //new XAttribute("style", style_menu),
        new XElement("a", new XAttribute("href", "?id=" + home_id),
            new XElement("img",
                new XAttribute("src", "/img/i_home.gif"),
                new XAttribute("width", "10"), new XAttribute("height", "10"),
                new XAttribute("hspace", "4"), new XAttribute("border", "0"),
                new XAttribute("alt", "Главная страница"))),
        new XElement("a", new XAttribute("href", "mailto:pavl@iis.nsk.su?subject=soran1957.ru%20 " + (string.IsNullOrWhiteSpace(id) ? home_id : id)),
            new XElement("img", new XAttribute("src", "/img/i_mail.gif"), new XAttribute("width", "10"),
                new XAttribute("height", "10"), new XAttribute("hspace", "11"),
                new XAttribute("border", "0"), new XAttribute("alt", "Написать письмо"))),
        new XElement("br"),
        new XElement("hr", new XAttribute("noshade", "1"), new XAttribute("size", "1"), new XAttribute("color", "#cccccc")),
        new XElement("div", new XAttribute("class", "text_menu"),
            new XElement("div", new XAttribute("class", "p_menu"),
                new XElement("a", new XElement("b", "Начало"), id == home_id ? null : new XAttribute("href", "?id=" + home_id)),
                new XElement("hr", new XAttribute("noshade", "1"), new XAttribute("size", "3"), new XAttribute("color", "#cccccc")),
                new XElement("a", new XElement("b", "СО РАН"), id == soran_id ? null : new XAttribute("href", "?id=" + soran_id)),
                new XElement("hr", new XAttribute("noshade", "1"), new XAttribute("size", "3"), new XAttribute("color", "#cccccc")),
                new XElement("a", new XElement("b", "Первичные фотоматериалы"), pars["p"] == database_id ? null : new XAttribute("href", "?id=" + database_id)),
                new XElement("hr", new XAttribute("noshade", "1"), new XAttribute("size", "3"), new XAttribute("color", "#cccccc")),
                new XElement("a", new XElement("b", "Архив газет \"За науку в Сибири\""), new XAttribute("href", "?id=" + newspaper_id)),// new XAttribute("target", "external")
                new XElement("hr", new XAttribute("noshade", "1"), new XAttribute("size", "3"), new XAttribute("color", "#cccccc")),
                new XElement("a", new XElement("b", "О проекте"), pars["p"] == about_id ? null : new XAttribute("href", "?id=" + about_id)),
                new XElement("hr", new XAttribute("noshade", "1"), new XAttribute("size", "3"), new XAttribute("color", "#cccccc")),
                new XElement("a", new XElement("b", "Полезные ссылки"), id == soran_id ? null : new XAttribute("href", "?id=" + usefullLinks_id)),
                null),
            null),
        new XElement("hr", new XAttribute("noshade", "1"), new XAttribute("size", "3"), new XAttribute("color", "#cccccc")),
        null);

            XElement search =
    new XElement("div", new XAttribute("id", "search"), //new XAttribute("style", style_sear),
        new XElement("form", new XAttribute("action", ""),
            new XElement("input", new XAttribute("type", "hidden"), new XAttribute("name", "p"), new XAttribute("value", "typed-item-list")),
            new XElement("input", new XAttribute("type", "hidden"), new XAttribute("name", "ch"), new XAttribute("value", "search-form")),
            new XElement("table", new XAttribute("cellpadding", "0"), new XAttribute("cellspacing", "0"), new XAttribute("border", "0"),
                new XElement("tr",
                    new XElement("td", new XAttribute("class", sdirection == "photo-doc" ? "search_option_s" : "search_option"),
                        new XElement("a", sdirection == "photo-doc" ? null : new XAttribute("href", "?sdirection=photo-doc"), "Фотографии")),
                    new XElement("td", new XAttribute("class", sdirection == "person" ? "search_option_s" : "search_option"),
                        new XElement("a", sdirection == "person" ? null : new XAttribute("href", "?sdirection=person"), "Персоны")),
                    new XElement("td", new XAttribute("class", sdirection == "org-sys" ? "search_option_s" : "search_option"),
                        new XElement("a", sdirection == "org-sys" ? null : new XAttribute("href", "?sdirection=org-sys"), "Организации")))),
            new XElement("table", new XAttribute("cellpadding", "0"), new XAttribute("cellspacing", "0"), new XAttribute("border", "0"),
                new XAttribute("width", "100%"), new XAttribute("height", "55"),
                new XElement("tr",
                    new XElement("td", new XAttribute("width", "100%"), new XAttribute("class", "search_form"),
                        new XElement("input", new XAttribute("name", "searchstring"),
                            new XAttribute("type", "text"), new XAttribute("class", "form_search"))),
                    new XElement("td", new XAttribute("class", "search_form"),
                        new XElement("input",
                            new XAttribute("type", "image"),
                            new XAttribute("width", "63"), new XAttribute("height", "19"),
                            new XAttribute("src", "/img/btn_search.gif"),
                            new XAttribute("style", "margin-right:5px;")))),
                new XElement("tr", new XAttribute("valign", "top"),
                    new XElement("td", new XAttribute("colspan", "2"), new XAttribute("class", "search_form"),
                        new XElement("input", new XAttribute("type", "checkbox"), new XAttribute("id", "search_all"),
                            new XAttribute("disabled", "disabled"), new XAttribute("style", "border:0px solid #036;")),
                        new XElement("label", new XAttribute("for", "search_all"), "искать по отдельным словам"),
                        " | ",
                        new XElement("a", "расширенный поиск")))),
            null),
        null);

            XElement html = new XElement("html",
                new XElement("head", // <meta charset="utf-8" />
                    new XElement("meta", new XAttribute("charset", "utf-8"), ""),
                    new XElement("link", new XAttribute("rel", "stylesheet"), new XAttribute("href", "/css/zh.css"), new XAttribute("type", "text/css")),
                    new XElement("link", new XAttribute("rel", "stylesheet"),
                      new XAttribute("type", "text/css"), new XAttribute("href", "/css/Styles.css")),
                    new XElement("script",
                      new XAttribute("language", "javascript"),
                      new XAttribute("type", "text/JScript"),
                      new XAttribute("src", "PublicuemCommon/PublicuemScript.js"),
                      "."),
                    null),
                new XElement("body",
                    new XElement("table", new XAttribute("width", "100%"), new XAttribute("style", "table-layout:fixed;"),
                        new XElement("col", new XAttribute("width", "150")),
                        new XElement("col", new XAttribute("width", "100%")),
                        new XElement("col", new XAttribute("width", "410")),
                        new XElement("tr", new XAttribute("valign", "top"),
                            new XElement("td", null),
                            new XElement("td", logo),
                            new XElement("td", login)),
                        new XElement("tr", new XAttribute("valign", "top"),
                            new XElement("td", menu, new XAttribute("rowspan", "2")),
                            new XElement("td", search),
                            new XElement("td", null)),
                        new XElement("tr", new XAttribute("valign", "top"),
                            //new XElement("td", null),
                            new XElement("td", centralPanel),
                            new XElement("td", rightPanel)),
                        new XElement("tr", new XAttribute("valign", "top"),
                            new XElement("td", new XAttribute("colspan", "3"), new XElement("hr"))),
                        new XElement("tr", new XAttribute("valign", "top"),
                            new XElement("td", null),
                            new XElement("td", new XAttribute("colspan", "2"), "(c) Фотоархив Сибирского отделения Российской академии наук")))));
            return html;

            //return new XElement("html",
            //    new XElement("head", new XElement("meta", new XAttribute("charset", "utf-8"), ".")),
            //    new XElement("body",
            //        new XElement("div", "pars:",
            //            pars.Select(p => new XElement("div", p.Key, " ", p.Value))),
            //        new XElement("div", "session:",
            //            look),
            //        null));
        }

        private void BuildItem(XElement centralPanel, XElement rightPanel, string id)
        { 
        }
        private void BuildSearchResults(XElement centralPanel, XElement rightPanel, string searchstring, string sdirection)
        {
        }

        }
}
