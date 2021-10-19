using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyChecker
{
    class Variables
    {
        public static int MaxThreads = 500;
        public static int ProxyTimeout = 7500;
        public static int ProgressTarget;
        public static int BadProxies;
        public static int GoodProxies;
        public static int Progress;
        public static string ProxyType;
        public static List<string> ProxyList = new List<string>();
        public static List<string> WorkingProxies = new List<string>();
    }
}
