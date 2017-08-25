using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HccConfig
{
    public class Rootobject
    {
        public string url { get; set; }
        public string alias { get; set; }
        public string zipped { get; set; }
        public File[] files { get; set; }
        public Externalurl[] externalUrl { get; set; }
        public Externaldata[] externalData { get; set; }
        public Externaljs externalJS { get; set; }
    }

    public class Externaljs
    {
        public string js { get; set; }
        public Var[] vars { get; set; }
    }

    public class Var
    {
        public string name { get; set; }
        public string desc { get; set; }
    }

    public class File
    {
        public string url { get; set; }
        public bool replace { get; set; }
        public string zipped { get; set; }
    }

    public class Externalurl
    {
        public string url { get; set; }
        public bool replace { get; set; }
        public string zipped { get; set; }
    }

    public class Externaldata
    {
        public string url { get; set; }
        public string request { get; set; }
        public bool replace { get; set; }
        public string zipped { get; set; }
    }
}



