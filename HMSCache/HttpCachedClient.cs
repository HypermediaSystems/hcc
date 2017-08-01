using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace HMS.Net.Http
{
    public class HttpCachedClient: HttpClient
    {
        /// <summary>
        /// This is the basename of the SQLite database.
        /// We use this hack, since the XF Dependency Service does not support constructors with parameters.
        /// </summary>
        public static string dbName = "hcc";

        /// <summary>
        /// 
        /// </summary>
        public static Boolean addInfo = true;
        public static Boolean addHeaders = true;

        public string[] includeHeaders = null;

        private iDataProvider cache;
        /// <summary>
        /// HMSCache
        /// </summary>
        /// <param name="cache"></param>
        public HttpCachedClient(iDataProvider cache) : base()
        {
            this.cache = cache;
        }
        /// <summary>
        /// HMSCache
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<int> GetStream(string url, Action<Stream, hccInfo> callback)
        {
            hccInfo hi = new hccInfo();

            // check if this is in the cache
            byte[] data = this.cache.GetData(url);
            if (data != null)
            {
                hi.fromDb = true;
                if (HttpCachedClient.addInfo == true)
                {
                    iDataItem item = this.cache.GetInfo(url);
                    if (item != null)
                    {
                        hi.withInfo = true;
                        hi.set(item);
                    }
                }
                if (HttpCachedClient.addHeaders == true)
                {
                    hi.hhh = this.cache.GetHeaders(url);
                }
                Stream streamToReadFrom = new MemoryStream(data);
                callback(streamToReadFrom,hi);
                return 1;
            }
          
            using (HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseContentRead))
            {
                string headerString = this.getHeader(response.Headers);

                Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();

                Stream strm = new MemoryStream();
                streamToReadFrom.CopyTo(strm);
                this.cache.SetData(url, ((MemoryStream)strm).ToArray(),headers: headerString,zipped:1);
               
                strm.Seek(0, SeekOrigin.Begin);

                callback(strm,hi);
                return 0;
            }
        }
        /// <summary>
        /// HMSCache
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<int> GetString(string url, Action<string, hccInfo> callback)
        {
            hccInfo hi = new hccInfo();

            // check if this is in the cache
            string data = this.cache.GetString(url);
            if (data != null)
            {
                hi.fromDb = true;

                if (HttpCachedClient.addInfo == true)
                {
                    iDataItem item = this.cache.GetInfo(url);
                    if (item != null)
                    {
                        hi.withInfo = true;
                        hi.set(item);
                    }
                }
                if (HttpCachedClient.addHeaders == true)
                {
                    hi.hhh = this.cache.GetHeaders(url);
                }

                callback(data,hi);
                return 1;
            }

            using (HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseContentRead))
            {
                string headerString = this.getHeader(response.Headers);

                string responseString = "";
                Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                using (StreamReader theStreamReader = new StreamReader(streamToReadFrom))
                {
                    responseString = theStreamReader.ReadToEnd();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                }
                else
                {
                    responseString = "";
                }
                this.cache.SetString(url, responseString,headers:headerString, zipped:1);

                callback(responseString,hi);
                return 0;
            }
        }
        private string getHeader(System.Net.Http.Headers.HttpResponseHeaders headers)
        {
            string headerString = "";
            foreach (var h in headers)
            {
                headerString += h.Key + ": ";
                string del = "";
                foreach (var v in h.Value)
                {
                    headerString += del + v;
                    del = "; ";
                }
                headerString += Environment.NewLine;
            }
            return headerString;
        }
        public void AddString(string id, string data)
        {            
            this.cache.SetString(id, data,overwrite: true);
        }
        public void AddStream(string id, byte[] data)
        {
            this.cache.SetData(id, data, zipped: 1);
        }
        public void Delete(string id)
        {
            this.cache.Delete(id);
        }
    }
    public class hccHttpHeaders
    {
        public Dictionary<string, string[]> items;
        public hccHttpHeaders()
        {
            items = new Dictionary<string, string[]>();
        }
    }
    public class hccInfo: iDataItem
    {
        public Boolean withInfo { get; set; }
        public byte zipped { get; set; }
        public byte encrypted { get; set; }
        public DateTime loaded { get; set; }
        public DateTime expire { get; set; }
        public hccHttpHeaders hhh { get; set; }

        public Boolean fromDb { get; set; }
        public hccInfo()
        {
            fromDb = false;
            withInfo = false;
        }
        public void set(iDataItem src)
        {
            this.zipped = src.zipped;
            this.encrypted = src.encrypted;
            this.loaded = src.loaded;
            this.expire = src.expire;
        }
    }
}
