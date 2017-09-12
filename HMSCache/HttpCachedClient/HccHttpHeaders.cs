using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class HccHttpHeaders

    {
        public Dictionary<string, string[]> items { get; set; }

        public HccHttpHeaders()
        {
            items = new Dictionary<string, string[]>();
        }
    }

}
