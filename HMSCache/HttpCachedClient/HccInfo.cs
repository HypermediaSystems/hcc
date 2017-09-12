using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class HccInfo : IDataItem
    {
        public Boolean withInfo { get; set; }
        public byte zipped { get; set; }
        public byte encrypted { get; set; }
        public DateTime lastWrite { get; set; }
        public DateTime lastRead { get; set; }
        public DateTime expire { get; set; }
        public long size { get; set; }
        public Boolean dontRemove { get; set; }
        public HccHttpHeaders hhh { get; set; }

        public System.Net.HttpStatusCode responseStatus { get; set; }
        public Boolean fromDb { get; set; }

        /// <summary>
        /// This is the url used to lookup the data in the database
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// This is the requested url
        /// </summary>
        public string aliasUrl { get; set; }

        public HccInfo()
        {
            fromDb = false;
            withInfo = false;
            responseStatus = HttpStatusCode.OK;
            url = null;
            aliasUrl = null;
        }

        public void set(IDataItem src)
        {
            this.dontRemove = src.dontRemove;
            this.encrypted = src.encrypted;
            this.expire = src.expire;
            this.lastRead = src.lastRead;
            this.lastWrite = src.lastWrite;
            this.size = src.size;
            this.zipped = src.zipped;
        }
    }
}
