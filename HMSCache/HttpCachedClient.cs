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
        /// This is the basename of the SQLite database.<para/>
        /// We use this hack, since the XF Dependency Service does not support constructors with parameters.<para/>
        /// Default is "hcc".
        /// </summary>
        public static string dbName = "hcc";

        /// <summary>
        /// include additonal information in the hccInfo object<para/>
        /// Default is true.
        /// </summary>
        public static Boolean addInfo = true;
        /// <summary>
        /// include headers in the hccInfo object<para/>
        /// Default is true.
        /// </summary>
        public static Boolean addHeaders = true;
        /// <summary>
        /// <para>list of headers to include in the hccInfo object</para>
        /// <para>null, add no headers,</para>
        /// <para>empty, add all headers</para>
        /// <para>else, add listed headers</para>
        /// </summary>
        public string[] includeHeaders = null;
        /// <summary>
        /// If true, GetCachedString and GetCachedStream do not btry to fetch missing data.<para/>
        /// Default is false.
        /// </summary>
        public Boolean offline = false;

        private iDataProvider cache;
        /// <summary>
        /// Create a new HttpCachedClient object<para/>
        /// Currently the cache must be a SqLiteCache object.
        /// </summary>
        /// <param name="cache">the data provider for this cache,</param>
        public HttpCachedClient(iDataProvider cache) : base()
        {
            this.cache = cache;
        }
        /// <summary>
        /// Get the stream for the given url from the cache.<para/>
        /// If there is no entry found in the cache, the url is requested with GetAsync().<para/>
        /// This can only be successful for absolute urls.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<int> GetCachedStream(string url, Action<Stream, hccInfo> callback)
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
                string headerString = this.getCachedHeader(response.Headers);

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
        /// Get the string for the given url from the cache.<para/>
        /// If there is no entry found in the cache, the url is requested with GetAsync().<para/>
        /// This can only be successful for absolute urls.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<int> GetCachedString(string url, Action<string, hccInfo> callback)
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
            // ToDo: check for absolute url

            using (HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseContentRead))
            {
                string headerString = this.getCachedHeader(response.Headers);

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
        private string getCachedHeader(System.Net.Http.Headers.HttpResponseHeaders headers)
        {
            string headerString = "";
            foreach (var h in headers)
            {
                Boolean skip = false;
                if (this.includeHeaders == null)
                {
                    skip = true;
                }
                else if (this.includeHeaders.Length > 0)
                {
                    skip = !this.includeHeaders.Contains(h.Key);
                }
                if (skip == false)
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
            }
            return headerString;
        }
        /// <summary>
        /// Get the number of bytes stored in the cache.<para/>
        /// This includes only the data and the header.<para/>
        /// </summary>
        /// <returns></returns>
        public long GetCachedSize()
        {
            return this.cache.Size();
        }
        /// <summary>
        /// Get the number of entries stored in the cache.<para/>
        /// </summary>
        /// <returns></returns>
        public long GetCachedCount()
        {
            return this.cache.Count();
        }
        /// <summary>
        /// Add the given string to the cache.<para/>
        /// The id may be any string, especially a relative url.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public void AddCachedString(string id, string data)
        {            
            this.cache.SetString(id, data,overwrite: true);
        }
        /// <summary>
        /// Add the given stream to the cache.<para/>
        /// The id may be any string, especially a relative url.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public void AddCachedStream(string id, byte[] data)
        {
            this.cache.SetData(id, data, zipped: 1);
        }
        /// <summary>
        /// Delete the entry from the cache.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteCachedData(string id)
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
        public DateTime lastWrite { get; set; }
        public DateTime lastRead { get; set; }
        public DateTime expire { get; set; }
        public long size { get; set; }
        public hccHttpHeaders hhh { get; set; }

        public Boolean fromDb { get; set; }
        public hccInfo()
        {
            fromDb = false;
            withInfo = false;
        }
        public void set(iDataItem src)
        {
            this.encrypted = src.encrypted;
            this.expire = src.expire;
            this.lastRead = src.lastRead;
            this.lastWrite = src.lastWrite;
            this.size = src.size;
            this.zipped = src.zipped;
        }
    }
}
