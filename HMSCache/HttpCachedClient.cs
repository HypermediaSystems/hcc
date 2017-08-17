using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        /// Include additonal information in the hccInfo object<para/>
        /// Default is true.
        /// </summary>
        public Boolean addInfo = true;
        /// <summary>
        /// Include headers in the hccInfo object<para/>
        /// Default is true.
        /// </summary>
        public Boolean addHeaders = true;
        /// <summary>
        /// <para>list of headers to include in the hccInfo object</para>
        /// <para>null, add no headers,</para>
        /// <para>empty, add all headers</para>
        /// <para>else, add listed headers</para>
        /// </summary>
        public string[] includeHeaders = null;
        /// <summary>
        /// If true, GetCachedString and GetCachedStream do not try to fetch missing data.<para/>
        /// Default is false.
        /// </summary>
        public Boolean isOffline = false;
        public Boolean isReadonly = false;
        /// <summary>
        /// This optional AuthenticationHeaderValue will be assigned to HttpClient.DefaultRequestHeaders.Authorization
        /// </summary>
        public AuthenticationHeaderValue authenticationHeaderValue = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate Byte[] encryptHandler(string urlRequested, Byte[] data);
        public delegate Byte[] decryptHandler(string urlRequested, Byte[] data);
        /// <summary>
        /// This function is called when decryption is required.
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public encryptHandler encryptFunction;
        /// <summary>
        /// This function is called when decryption is required.
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public decryptHandler decryptFunction;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="ttpCachedClient"></param>
        /// <returns></returns>
        public delegate int beforeGetAsyncHandler(string urlRequested, HttpCachedClient httpCachedClient);

        /// <summary>
        /// This function is called before the GetAsync() call.<para/>
        /// You can set additonal headers here.
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="httpCachedClient"></param>
        /// <returns></returns>
        public beforeGetAsyncHandler beforeGetAsyncFunction;
        /// <summary>
        /// The data can be zipped before storing it in the cache.<para/>
        /// 0 = dont zip,
        /// 1 = use gzip (build-in)
        /// </summary>
        public Byte zipped = 1;


        /// <summary>
        /// 
        /// </summary>
        public int SqlLimit = 100;

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
        public async Task<Boolean> BackupAsync(string serverUrl, AuthenticationHeaderValue authentication = null)
        {
            Byte[] bytes = this.cache.GetBytes();
            using (var client = new HttpClient())
            {
                if (authentication != null)
                    client.DefaultRequestHeaders.Authorization = authentication;

                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new MemoryStream(bytes)), "file", HttpCachedClient.dbName + ".sqlite");

                    using (
                       var message =
                           await client.PostAsync(serverUrl, content))
                    {
                        var input = await message.Content.ReadAsStringAsync();

                        return true;
                    }
                }
            }
        }
        public async Task<Boolean> RestoreAsync(string serverUrl, AuthenticationHeaderValue authentication = null)
        {
            using (var client = new HttpClient())
            {
                if (authentication != null)
                    client.DefaultRequestHeaders.Authorization = authentication;

                Byte[] bytes = await client.GetByteArrayAsync(serverUrl);

                this.cache.SetBytes(bytes);
            }
            return true;

        }
        public Boolean Backup(string serverUrl) //, AuthenticationHeaderValue authentication )
        {
            Byte[] bytes = this.cache.GetBytes();
            using (var client = new HttpClient())
            {
              //  if (authentication != null)
              //      client.DefaultRequestHeaders.Authorization = authentication;

                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new MemoryStream(bytes)), "file", HttpCachedClient.dbName + ".sqlite");

                    string resp = "";
                    try
                    {

                        Task.Run(async () =>
                        {
                            using (var message = await client.PostAsync(serverUrl, content))
                            {
                                resp = await message.Content.ReadAsStringAsync();
                            }
                        }).Wait();
                    }
                    catch (Exception)
                    {
                        // throw;
                    }
                    return true;
                }
            }
        }
        public  Boolean Restore(string serverUrl) // , AuthenticationHeaderValue authentication )
        {
            using (var client = new HttpClient())
            {
              //  if (authentication != null)
              //      client.DefaultRequestHeaders.Authorization = authentication;

                Byte[] bytes = null;
                try
                {
                    Task.Run(async () =>
                    {
                        bytes = await client.GetByteArrayAsync(serverUrl);
                    }).Wait();
                    this.cache.SetBytes(bytes);
                }
                catch (Exception)
                {
                    // throw;
                }
            }
            return true;

        }
        /// <summary>
        /// Get the stream for the given url from the cache.<para/>
        /// If there is no entry found in the cache (and this.offline is true), the url is requested with GetAsync().
        /// This can only be successful for absolute urls.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<int> GetCachedStream(string url, Action<Stream, hccInfo> callback)
        {
            hccInfo hi = new hccInfo();

            string useUrl = this.cache.GetUrlFromAlias(url);

            hi.url = url;
            if (useUrl != url)
            {
                hi.aliasUrl = url;
                hi.url = useUrl;
            }
            // check if this is in the cache
            byte[] data = this.cache.GetData(useUrl);
            if (data != null)
            {
                hi.fromDb = true;
                if (this.addInfo == true)
                {
                    iDataItem item = this.cache.GetInfo(useUrl);
                    if (item != null)
                    {
                        hi.withInfo = true;
                        hi.set(item);
                    }
                }
                if (this.addHeaders == true)
                {
                    hi.hhh = this.cache.GetHeaders(useUrl);
                }

                if (this.decryptFunction != null &&hi.encrypted == 1)
                {
                    data = this.decryptFunction(useUrl, data);
                }
                hi.size = data.Length;
                Stream streamToReadFrom = new MemoryStream(data);
                callback(streamToReadFrom,hi);
                return 1;
            }
            if (this.isOffline == false)
            {
                if (this.authenticationHeaderValue != null)
                    this.DefaultRequestHeaders.Authorization = this.authenticationHeaderValue;

                if (this.beforeGetAsyncFunction != null)
                    this.beforeGetAsyncFunction(useUrl, this);

                using (HttpResponseMessage response = await this.GetAsync(useUrl, HttpCompletionOption.ResponseContentRead))
                {
                    string headerString = this.getCachedHeader(response.Headers);

                    Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();

                    Stream strm = new MemoryStream();
                    streamToReadFrom.CopyTo(strm);
                    hi.responseStatus = response.StatusCode;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (this.isReadonly == false)
                        {
                            data = ((MemoryStream)strm).ToArray();
                            if (this.encryptFunction != null)
                            {
                                data = this.encryptFunction(useUrl, data);

                                this.cache.SetData(useUrl, data, headers: headerString, zipped: this.zipped, encrypted: 1);
                            }
                            else
                            {
                                this.cache.SetData(useUrl, data, headers: headerString, zipped: this.zipped);
                            }
                        }
                        strm.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        strm = null;
                    }

                    if (this.addHeaders == true)
                    {
                        hi.hhh = this.cache.GetHeadersFromString(headerString);
                    }
                    callback(strm, hi);
                    return 0;
                }
            }
            hi.size = 0;
            callback(null, hi);
            return 0;
        }
        /// <summary>
        /// Get the string for the given url from the cache.<para/>
        /// If there is no entry found in the cache (and this.offline is true), the url is requested with GetAsync().
        /// This can only be successful for absolute urls.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<int> GetCachedString(string url, Action<string, hccInfo> callback)
        {
            await this.GetCachedStream(url, (strm, hi) => {
                if (strm != null)
                {
                    var bytes = ((MemoryStream)strm).ToArray();
                    string str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    callback(str, hi);
                }
                else
                {
                    callback(null, hi);
                }
            });
            return 0;
        }
        /// <summary>
        /// Get the string for the given url from the cache.<para/>
        /// If there is no entry found in the cache, the url is requested with GetAsync().<para/>
        /// This can only be successful for absolute urls.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<int> GetCachedStringOld(string url, Action<string, hccInfo> callback)
        {
            hccInfo hi = new hccInfo();

            // check if this is in the cache
            string data = this.cache.GetString(url);
            if (data != null)
            {
                hi.fromDb = true;

                if (this.addInfo == true)
                {
                    iDataItem item = this.cache.GetInfo(url);
                    if (item != null)
                    {
                        hi.withInfo = true;
                        hi.set(item);
                    }
                }
                if (this.addHeaders == true)
                {
                    hi.hhh = this.cache.GetHeaders(url);
                }
                if (this.decryptFunction != null && hi.encrypted == 1)
                {
                    Byte[] bytes = Encoding.UTF8.GetBytes(data);
                    bytes = this.decryptFunction(url, bytes);
                    data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                }

                callback(data,hi);
                return 1;
            }
            // ToDo: check for absolute url
            if (this.authenticationHeaderValue != null)
                this.DefaultRequestHeaders.Authorization = this.authenticationHeaderValue;

            using (HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseContentRead))
            {
                string headerString = this.getCachedHeader(response.Headers);

                string responseString = "";
                Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                using (StreamReader theStreamReader = new StreamReader(streamToReadFrom))
                {
                    responseString = theStreamReader.ReadToEnd();
                }
                hi.responseStatus = response.StatusCode;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (this.encryptFunction != null)
                    {
                        Byte[] bytes = Encoding.UTF8.GetBytes(responseString);
                        bytes = this.encryptFunction(url, bytes);
                        this.cache.SetData(url, bytes, headers: headerString, zipped: this.zipped, encrypted:1);
                    }
                    else
                    {
                        this.cache.SetString(url, responseString, headers: headerString, zipped: this.zipped);
                    }
                }
                else
                {
                    responseString = "";
                }

                callback(responseString,hi);
                return 0;
            }
        }
        public string getCachedHeader(System.Net.Http.Headers.HttpResponseHeaders headers)
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
        /// Get the list of cached URLs that contains the text pattern.<para/>
        /// The list is sorted descending by LastRead and limited by this.SqlLimit.<para/>
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public string[] GetCachedUrls(string pattern)
        {
            return this.cache.GetIDs(pattern,this.SqlLimit);
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
        public void AddCachedStream(string id, byte[] data,string headers= "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            this.cache.SetData(id, data, headers: headers, overwrite: overwrite, zipped: zipped, encrypted: encrypted);
        }

        public void AddCachedMetadata(string id, string data)
        {
            this.cache.SetMetadata(id, data);
        }
        public void AddCachedAliasUrl(string aliasUrl, string url)
        {
            this.cache.SetAlias(aliasUrl, url);
        }
        /// <summary>
        /// Delete the entry from the cache.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteCachedData(string id)
        {
            this.cache.Delete(id);
        }
        /// <summary>
        /// remove all entries from the cache
        /// </summary>
        public void DeleteAllCachedData()
        {
            this.cache.DeleteAllData();
        }
        #region DATABAINDING
        public string DBName{ get { return this.cache.DBName(); } }
        public string DBPath { get { return this.cache.DBPath(); } }
        public string DBSize { get { return this.cache.Size().ToString(); } }
        public string DBCount { get { return this.cache.Count().ToString(); } }


        public IEnumerable<SqLiteCacheItem> DBEntries(string urlPattern)
        {
            return this.cache.GetEntries( urlPattern);
        }
        public SqLiteCacheItem DBEntry(string url)
        {
            return this.cache.GetEntry(url);
        }

        #endregion
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
        public Boolean dontRemove { get; set; }
        public hccHttpHeaders hhh { get; set; }


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

        public hccInfo()
        {
            fromDb = false;
            withInfo = false;
            responseStatus = HttpStatusCode.OK;
            url = null;
            aliasUrl = null;
        }
        public void set(iDataItem src)
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
