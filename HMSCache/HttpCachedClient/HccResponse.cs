using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class HccResponse
    {
        public string json;
        public Stream stream;
        public HccInfo hccInfo;
        public HccResponse(string json, HccInfo hcInfo)
        {
            this.json = json;
            this.hccInfo = hcInfo;
        }
        public HccResponse(Stream stream, HccInfo hcInfo)
        {
            this.stream = stream;
            this.hccInfo = hcInfo;
        }
    }

}
