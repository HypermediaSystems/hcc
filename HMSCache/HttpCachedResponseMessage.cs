using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HMSCache
{
    class HttpCachedResponseMessage : HttpResponseMessage
    {
        string json;
        HccInfo hi;
        public HttpCachedResponseMessage(HttpResponseMessage response, Boolean addHeaders)
        {
            this.hi = new HccInfo();

        }
    }
}
