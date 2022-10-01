using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OAData
{
    public interface IFactographDataService
    {
        void Init(string connectionstring);
        void Close();
        // ============== Основные методы доступа к БД =============
        IEnumerable<XElement> SearchByName(string searchstring);
        IEnumerable<XElement> SearchByWords(string searchwords);
        XElement GetItemByIdBasic(string id, bool addinverse);
        XElement GetItemById(string id, XElement format);
        IEnumerable<XElement> GetAll();

        // ============== Загрузка базы данных ===============
        //void StartFillDb(Action<string> turlog);
        //void LoadXFlow(IEnumerable<XElement> xflow, Dictionary<string, string> orig_ids);
        //void FinishFillDb(Action<string> turlog);

        // ============== Редктирование ===============
        XElement UpdateItem(XElement item);
        XElement PutItem(XElement item);

        // ============== Работа с файлами и кассетами ================
        string CassDirPath(string uri);
        string GetFilePath(string u, string s);
        bool HasWritabeFogForUser(string user);
        OAData.Adapters.CassInfo[] Cassettes { get; }

    }
}
