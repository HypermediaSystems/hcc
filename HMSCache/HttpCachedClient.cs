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
    public class HttpCachedClient : HttpClient
    {
        /// <summary>
        /// This is the basename of the SQLite database.<para/>
        /// We use this hack, since the XF Dependency Service does not support constructors with parameters.<para/>
        /// Default is "hcc".
        /// </summary>
        public static string _dbName { get; set; } = "hcc";

        /// <summary>
        /// Include additonal information in the hccInfo object<para/>
        /// Default is true.
        /// </summary>
        public Boolean addInfo { get; set; } = true;

        /// <summary>
        /// Include headers in the hccInfo object<para/>
        /// Default is true.
        /// </summary>
        public Boolean addHeaders { get; set; } = true;

        /// <summary>
        /// <para>list of headers to include in the hccInfo object</para>
        /// <para>null, add no headers,</para>
        /// <para>empty, add all headers</para>
        /// <para>else, add listed headers</para>
        /// </summary>
        public string[] includeHeaders { get; set; }

        /// <summary>
        /// If true, GetCachedString and GetCachedStream do not try to fetch missing data.<para/>
        /// Default is false.
        /// </summary>
        public Boolean isOffline { get; set; }

        public Boolean isReadonly { get; set; }

        /// <summary>
        /// This optional AuthenticationHeaderValue will be assigned to HttpClient.DefaultRequestHeaders.Authorization
        /// </summary>
        public AuthenticationHeaderValue authenticationHeaderValue { get; set; }

        /// <summary>
        /// This function is called when decryption is required.
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate Byte[] encryptHandler(string urlRequested, Byte[] data);

        public delegate Byte[] decryptHandler(string urlRequested, Byte[] data);

        public encryptHandler encryptFunction { get; set; }

        /// <summary>
        /// This function is called when decryption is required.
        /// </summary>        
        public decryptHandler decryptFunction { get; set; }

        /// <summary>
        /// This function is called before the GetAsync() call.<para/>
        /// You can set additonal headers here.
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="httpCachedClient"></param>
        /// <returns></returns>
        public delegate int beforeGetAsyncHandler(string urlRequested, HttpCachedClient httpCachedClient);

        public beforeGetAsyncHandler beforeGetAsyncFunction { get; set; }

        /// <summary>
        /// The data can be zipped before storing it in the cache.<para/>
        /// 0 = dont zip,
        /// 1 = use gzip (build-in)
        /// </summary>
        public Byte zipped { get; set; } = 1;

        public string errMsg { get; set; }
        /// <summary>
        /// the limit of records returned
        /// </summary>
        public int SqlLimit { get; set; } = 100;

        private readonly IDataProvider cache;

        /// <summary>
        /// Create a new HttpCachedClient object<para/>
        /// Currently the cache must be a SqLiteCache object.
        /// </summary>
        /// <param name="cache">the data provider for this cache,</param>
        public HttpCachedClient(IDataProvider cache) // : base()
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
                    content.Add(new StreamContent(new MemoryStream(bytes)), "file", HttpCachedClient._dbName + ".sqlite");

                    using (
                       var message =
                           await client.PostAsync(serverUrl, content).ConfigureAwait(false))
                    {
                        this.errMsg = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

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

                Byte[] bytes = await client.GetByteArrayAsync(serverUrl).ConfigureAwait(false);

                this.cache.SetBytes(bytes);
            }
            return true;
        }

        public Boolean Backup(string serverUrl) //, AuthenticationHeaderValue authentication )
        {
            Byte[] bytes = this.cache.GetBytes();
            using (var client = new HttpClient())
            {
                /*  if (authentication != null)
                *      client.DefaultRequestHeaders.Authorization = authentication;
                */
                string boundary = "Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture);
                boundary = boundary.Replace(" ", "-");
                using (var content = new MultipartFormDataContent(boundary))
                {
                    content.Add(new StreamContent(new MemoryStream(bytes)), "file", HttpCachedClient._dbName + ".sqlite");

                    string resp = "";
                    try
                    {
                        Task.Run(async () =>
                        {
                            using (var message = await client.PostAsync(serverUrl, content).ConfigureAwait(false))
                            {
                                resp = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                            }
                        }).Wait();
                    }
                    catch (Exception ex)
                    {
                        // ToDo log this error
                        this.errMsg = ex.Message;
                    }
                    return true;
                }
            }
        }

        public Boolean Reset()
        {
            this.cache.Reset();

            return true;
        }

        public Boolean Restore(string serverUrl) // , AuthenticationHeaderValue authentication )
        {
            using (var client = new HttpClient())
            {
                //  if (authentication != null)
                //      client.DefaultRequestHeaders.Authorization = authentication;

                Byte[] bytes = null;
                try
                {
                    Task.Run(async () =>
                        bytes = await client.GetByteArrayAsync(serverUrl).ConfigureAwait(false)
                    ).Wait();
                    this.cache.SetBytes(bytes);
                }
                catch (Exception)
                {
                    throw;
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
        public async Task<int> GetCachedStreamAsync(string url, Action<Stream, HccInfo> callback)
        {
            HccInfo hi = new HccInfo();

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
                if (this.addInfo )
                {
                    IDataItem item = this.cache.GetInfo(useUrl);
                    if (item != null)
                    {
                        hi.withInfo = true;
                        hi.set(item);
                    }
                }
                if (this.addHeaders )
                {
                    hi.hhh = this.cache.GetHeaders(useUrl);
                }

                if (this.decryptFunction != null && hi.encrypted == 1)
                {
                    data = this.decryptFunction(useUrl, data);
                }
                hi.size = data.Length;
                Stream streamToReadFrom = new MemoryStream(data);
                callback(streamToReadFrom, hi);
                return 1;
            }
            if ( !this.isOffline )
            {
                if (this.authenticationHeaderValue != null)
                    this.DefaultRequestHeaders.Authorization = this.authenticationHeaderValue;

                if (this.beforeGetAsyncFunction != null)
                    this.beforeGetAsyncFunction(useUrl, this);

                using (HttpResponseMessage response = await this.GetAsync(useUrl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string headerString = this.GetCachedHeader(response.Headers);

                    Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    Stream strm = new MemoryStream();
                    streamToReadFrom.CopyTo(strm);
                    hi.responseStatus = response.StatusCode;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if ( !this.isReadonly )
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

                    if (this.addHeaders )
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
        public async Task<int> GetCachedStringAsync(string url, Action<string, HccInfo> callback)
        {
            await this.GetCachedStreamAsync(url, (strm, hi) =>
            {
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
            }).ConfigureAwait(false);
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
        public async Task<int> GetCachedStringOldAsync(string url, Action<string, HccInfo> callback)
        {
            HccInfo hi = new HccInfo();

            // check if this is in the cache
            string data = this.cache.GetString(url);
            if (data != null)
            {
                hi.fromDb = true;

                if (this.addInfo )
                {
                    IDataItem item = this.cache.GetInfo(url);
                    if (item != null)
                    {
                        hi.withInfo = true;
                        hi.set(item);
                    }
                }
                if (this.addHeaders )
                {
                    hi.hhh = this.cache.GetHeaders(url);
                }
                if (this.decryptFunction != null && hi.encrypted == 1)
                {
                    Byte[] bytes = Encoding.UTF8.GetBytes(data);
                    bytes = this.decryptFunction(url, bytes);
                    data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                }

                callback(data, hi);
                return 1;
            }
            // ToDo: check for absolute url
            if (this.authenticationHeaderValue != null)
                this.DefaultRequestHeaders.Authorization = this.authenticationHeaderValue;

            using (HttpResponseMessage response = await this.GetAsync(url, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
            {
                string headerString = this.GetCachedHeader(response.Headers);

                string responseString = "";
                Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
                        this.cache.SetData(url, bytes, headers: headerString, zipped: this.zipped, encrypted: 1);
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

                callback(responseString, hi);
                return 0;
            }
        }

        public string GetCachedHeader(System.Net.Http.Headers.HttpResponseHeaders headers)
        {
            StringBuilder bld = new StringBuilder();

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
                if ( !skip )
                {
                    bld.Append(h.Key).Append(": ");
                    string del = "";
                    foreach (var v in h.Value)
                    {
                        bld.Append(del).Append(v);
                        del = "; ";
                    }
                    bld.Append(Environment.NewLine);
                }
            }
            return bld.ToString();
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
            return this.cache.GetIDs(pattern, this.SqlLimit);
        }

        /// <summary>
        /// Add the given string to the cache.<para/>
        /// The id may be any string, especially a relative url.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public void AddCachedString(string id, string data)
        {
            this.cache.SetString(id, data, overwrite: true);
        }
        /// <summary>
        /// Add the given stream to the cache.<para/>
        /// The id may be any string, especially a relative url./// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        /// <param name="overwrite"></param>
        /// <param name="zipped"></param>
        /// <param name="encrypted"></param>
        public void AddCachedStream(string id, byte[] data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
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

        public string DBName { get { return this.cache.DBName(); } }
        public string DBPath { get { return this.cache.DBPath(); } }
        public string DBSize { get { return this.cache.Size().ToString(); } }
        public string DBCount { get { return this.cache.Count().ToString(); } }

        public IEnumerable<SqLiteCacheItem> DBEntries(string urlPattern)
        {
            return this.cache.GetEntries(urlPattern);
        }

        public SqLiteCacheItem DBEntry(string url)
        {
            return this.cache.GetEntry(url);
        }

        #endregion DATABAINDING
    }

    public class HccHttpHeaders

    {
        public Dictionary<string, string[]> items { get; set; }

        public HccHttpHeaders()
        {
            items = new Dictionary<string, string[]>();
        }
    }

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