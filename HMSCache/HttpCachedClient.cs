using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                Stream streamToReadFrom = new MemoryStream(data);
                callback(streamToReadFrom,hi);
                return 1;
            }
          
            using (HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseContentRead))
            {
                Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();

                Stream strm = new MemoryStream();
                streamToReadFrom.CopyTo(strm);
                this.cache.SetData(url, ((MemoryStream)strm).ToArray());
               
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
                callback(data,hi);
                return 1;
            }

            using (HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseContentRead))
            {
                // string responseString = await response.Content.ReadAsStringAsync();
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
                this.cache.SetString(url, responseString);

                callback(responseString,hi);
                return 0;
            }
        }
        public void AddString(string id, string data)
        {            
            this.cache.SetString(id, data,true);
        }
        public void AddStream(string id, byte[] data)
        {
            this.cache.SetData(id, data, true);
        }
        public void Delete(string id)
        {
            this.cache.Delete(id);
        }
    }
    public class hccInfo
    {
        public Boolean fromDb;
        public hccInfo()
        {
            fromDb = false;
        }
    }
}
