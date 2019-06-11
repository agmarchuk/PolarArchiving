using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Polar.DB;
using Polar.TripleStore;

namespace TestConsoleApp
{
    public class TripleRecordStore
    {
        private Nametable32 nt;
        //private UniversalSequenceBase table;
        private BearingDeletable table;
        private IndexKey32Comp s_index;
        private IndexKey32CompVector inv_index;
        private IndexView name_index;
        private Comparer<object> comp_like;
        private string[] preload_names = { "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://fogid.net/o/name" };
        public int Code_rdftype { get; set; }
        public int Code_fogname { get; set; }
        internal void PreloadFognames()
        {
            foreach (string s in preload_names) nt.GetSetStr(s);
            Code_rdftype = nt.GetSetStr("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
            Code_fogname = nt.GetSetStr("http://fogid.net/o/name");
            nt.Flush();
        }
        public TripleRecordStore(Func<Stream> stream_gen, string tmp_dir_path)
        {
            // сначала таблица имен
            nt = new Nametable32(stream_gen);
            // Предзагрузка должна быть обеспечена даже для пустой таблицы имен
            PreloadFognames();
            // Тип записи
            PType tp_record = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.integer)),
                new NamedType("fields",
                    new PTypeSequence(new PTypeRecord(
                        new NamedType("prop", new PType(PTypeEnumeration.integer)),
                        new NamedType("value", new PType(PTypeEnumeration.sstring))))),
                new NamedType("directs",
                    new PTypeSequence(new PTypeRecord(
                        new NamedType("prop", new PType(PTypeEnumeration.integer)),
                        new NamedType("entity", new PType(PTypeEnumeration.integer))))));
            // Главная последовательность: множестов кодированных записей
            table = new BearingDeletable(tp_record, stream_gen);
            // прямой ключевой индекс: по задаваемому ключу получаем запись
            s_index = new IndexKey32Comp(stream_gen, table, ob => true,
                ob => (int)((object[])ob)[0], null);
            // Обратный ссылочный индекс
            inv_index = new IndexKey32CompVector(stream_gen, table,
                obj =>
                {
                    object[] directs = (object[])((object[])obj)[2];
                    return directs.Cast<object[]>()
                        .Select(pair => (int)pair[1]);
                }, null);


            //// Индекс по текстам полей записей триплетов с предикатом http://fogid.net/o/name
            //// компаратор надо поменять!!!!!!
            //Comparer<object> comp = Comparer<object>.Create(new Comparison<object>((object a, object b) =>
            //{
            //    return string.Compare(
            //        (string)((object[])((object[])a)[2])[1],
            //        (string)((object[])((object[])b)[2])[1], StringComparison.OrdinalIgnoreCase);
            //}));
            //int cod_name = nt.GetSetStr("http://fogid.net/o/name");
            //// name-индекс пока будет скалярным и будет индексировать первое (!) name-поле в записи
            //name_index = new IndexView(stream_gen, table, 
            //    ob => (int)((object[])ob)[1] == cod_name, comp, tmp_dir_path, 20_000_000);

            //comp_like = Comparer<object>.Create(new Comparison<object>((object a, object b) =>
            //{
            //    int len = ((string)((object[])((object[])b)[2])[1]).Length;
            //    return string.Compare(
            //        (string)((object[])((object[])a)[2])[1], 0,
            //        (string)((object[])((object[])b)[2])[1], 0, len, StringComparison.OrdinalIgnoreCase);
            //}));

            //table.Indexes = new IIndex[] { s_index, inv_index, name_index };
            table.Indexes = new IIndex[] { s_index, inv_index };

        }
        public void Build()
        {
            s_index.Build();
            inv_index.Build();
            //name_index.Build();
            nt.Build();
            nt.Flush();
        }
        public void Clear()
        {
            table.Clear();
            s_index.Clear();
            inv_index.Clear();
            //name_index.Clear();
            nt.Clear();
            // Предзагрузка
            PreloadFognames();
        }
        public void Load(IEnumerable<object> records)
        {
            foreach (object[] record in records)
            {
                table.AppendItem(record);
            }
        }


    }
}
