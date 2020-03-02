using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoranCore
{
    public class StaticObjects
    {
        private static string path;
        public static void Init(string pth)
        {
            path = pth;
            OAData.OADB.Init(path);
            OAData.OADB.Load();
        }
    }
}
