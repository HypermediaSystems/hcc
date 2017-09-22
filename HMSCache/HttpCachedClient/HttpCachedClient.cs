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
        #region static
        /// <summary>
        /// This is the basename of the SQLite database.<para/>
        /// We use this hack, since the XF Dependency Service does not support constructors with parameters.<para/>
        /// Default is "hcc".
        /// </summary>
        public static string _dbName { get; set; } = "hcc";
        #endregion
        #region public properties
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

        #endregion
        #region delegates
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
        #endregion
        #region private properties
        private readonly IDataProvider cache;
        #endregion
        #region constructor
        /// <summary>
        /// Create a new HttpCachedClient object<para/>
        /// Currently the cache must be a SqLiteCache object.
        /// </summary>
        /// <param name="cache">the data provider for this cache,</param>
        public HttpCachedClient(IDataProvider cache) // : base()
        {
            this.cache = cache;
        }
        #endregion
        #region db management
        public async Task<Boolean> BackupAsync(string serverUrl, AuthenticationHeaderValue authentication = null)
        {
            Byte[] bytes = await this.cache.GetBytesAsync();
            using (var client = new HttpClient())
            {
                if (authentication != null)
                    client.DefaultRequestHeaders.Authorization = authentication;

                using (var content = new MultipartFormDataContent()) // "Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
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

        public async Task<Boolean> Backup(string serverUrl, AuthenticationHeaderValue authentication=null )
        {
            Byte[] bytes = await this.cache.GetBytesAsync();
            using (var client = new HttpClient())
            {
                if (authentication != null)
                   client.DefaultRequestHeaders.Authorization = authentication;

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
        /// <summary>
        /// Replace the SqLite database by an empty one, containing only the required tables<para/>
        /// This will delete all data.
        /// </summary>
        /// <returns></returns>
        public Task ResetAsync()
        {
            return this.cache.ResetAsync();
        }

        public async Task<Boolean> xxRestoreAsync(string serverUrl, AuthenticationHeaderValue authentication = null)
        {
            using (var client = new HttpClient())
            {
                if (authentication != null)
                    client.DefaultRequestHeaders.Authorization = authentication;

                Byte[] bytes = await client.GetByteArrayAsync(serverUrl).ConfigureAwait(false);

                await this.cache.SetBytesAsync(bytes);
            }
            return true;
        }
        /// <summary>
        /// Replace the current database with the one stored under the given URL.<para/>
        /// This will delete all data.
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="authentication"></param>
        /// <returns></returns>
        public async Task<Boolean> RestoreAsync(string serverUrl , AuthenticationHeaderValue authentication = null )
        {
            using (var client = new HttpClient())
            {
                if (authentication != null)
                    client.DefaultRequestHeaders.Authorization = authentication;

                Byte[] bytes = null;
                try
                {
                    Task.Run(async () =>
                        bytes = await client.GetByteArrayAsync(serverUrl).ConfigureAwait(false)
                    ).Wait();
                    await this.cache.SetBytesAsync(bytes);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return true;
        }


        /// <summary>
        /// Get the number of bytes stored in the cache.<para/>
        /// This includes only the data and the header.<para/>
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetCachedSizeAsync()
        {
            return await this.cache.SizeAsync();
        }
        public long GetCachedSize()
        {
            long lret = 0;
            Task.Run(async () =>
            {
                lret = await this.cache.SizeAsync();
            }).Wait();
            return lret;
        }
        /// <summary>
        /// Get the number of entries stored in the cache.<para/>
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetCachedCountAsync()
        {
            return await this.cache.CountAsync();
        }
        public long GetCachedCount()
        {
            long lret = 0;
            Task.Run(async () =>
            {
                lret = await this.cache.CountAsync();

            }).Wait();
            return lret;
        }

        #endregion
        #region entry
        /// <summary>
        /// Get the stream for the given url from the cache.<para/>
        /// If there is no entry found in the cache (and this.offline is true), the url is requested with GetAsync().
        /// This can only be successful for absolute urls.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HccResponse> GetCachedStreamAsync(string url)
        {
            HccInfo hi = new HccInfo();

            string useUrl = await this.cache.GetUrlFromAliasAsync(url);
            System.Diagnostics.Debug.WriteLine("GetCachedStreamAsync:"+ url + " " + useUrl);
            
            hi.url = url;
            if (useUrl != url)
            {
                hi.aliasUrl = url;
                hi.url = useUrl;
            }
            // check if this is in the cache
            byte[] data = await this.cache.GetDataAsync(useUrl);
            if (data != null)
            {
                hi.fromDb = true;
                if (this.addInfo )
                {
                    IDataItem item = await this.cache.GetInfoAsync(useUrl);
                    if (item != null)
                    {
                        hi.withInfo = true;
                        hi.set(item);
                    }
                }
                if (this.addHeaders )
                {
                    hi.hhh = await this.cache.GetHeadersAsync(useUrl);
                }

                if (this.decryptFunction != null && hi.encrypted == 1)
                {
                    data = this.decryptFunction(useUrl, data);
                }
                hi.size = data.Length;
                Stream streamToReadFrom = new MemoryStream(data);

                return new HccResponse(streamToReadFrom, hi);
            }
            if ( !this.isOffline )
            {
                if (this.authenticationHeaderValue != null)
                    this.DefaultRequestHeaders.Authorization = this.authenticationHeaderValue;

                if (this.beforeGetAsyncFunction != null)
                    this.beforeGetAsyncFunction(useUrl, this);

                using (HttpResponseMessage response = await this.GetAsync(useUrl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(true))
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

                                await this.cache.SetDataAsync(useUrl, data, headers: headerString, zipped: this.zipped, encrypted: 1);
                            }
                            else
                            {
                                await this.cache.SetDataAsync(useUrl, data, headers: headerString, zipped: this.zipped);
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
                    return new HccResponse(strm, hi);
                }
            }
            hi.size = 0;
            return new HccResponse((Stream)null, hi);
        }

        /// <summary>
        /// Get the string for the given url from the cache.<para/>
        /// If there is no entry found in the cache (and this.offline is true), the url is requested with GetAsync().
        /// This can only be successful for absolute urls.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HccResponse> GetCachedStringAsync(string url)
        {
            HccResponse hccResponse = await this.GetCachedStreamAsync(url);
            if (hccResponse.stream != null)
            {
                var bytes = ((MemoryStream)hccResponse.stream).ToArray();
                string str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                return new HccResponse(str, hccResponse.hccInfo);
            }
            else
            {
                return new HccResponse((string)null, hccResponse.hccInfo);
            }
        }

        /// <summary>
        /// Add the given string to the cache.<para/>
        /// The id may be any string, especially a relative url.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public async Task AddCachedStringAsync(string id, string data)
        {
            await this.cache.SetStringAsync(id, data, overwrite: true);
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
        public async Task AddCachedStreamAsync(string id, byte[] data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            await this.cache.SetDataAsync(id, data, headers: headers, overwrite: overwrite, zipped: zipped, encrypted: encrypted);
        }

        /// <summary>
        /// Delete the entry from the cache.
        /// </summary>
        /// <param name="id"></param>
        public async Task DeleteCachedDataAsync(string id)
        {
            await this.cache.DeleteAsync(id);
        }

        /// <summary>
        /// remove all entries from the cache
        /// </summary>
        public async Task DeleteAllCachedDataAsync()
        {
            await this.cache.DeleteAllDataAsync();
        }
        #endregion
        #region util
        // TODO: this should be moved to HttpCachedResponse.
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
        /// Get the list of cached URLs that contains the text pattern.<para/>
        /// The list is sorted descending by LastRead and limited by this.SqlLimit.<para/>
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public async Task<string[]> GetCachedUrlsAsync(string pattern)
        {
            return await this.cache.GetIDsAsync(pattern, this.SqlLimit);
        }
        #endregion

        #region METADATA
        /// <summary>
        /// Add an entry the Metadata table.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task AddCachedMetadataAsync(string id, string data)
        {
            await this.cache.SetMetadataAsync(id, data);
        }
        public async Task<string> GetCachedMetadataAsync(string id)
        {
            return await this.cache.GetMetadataAsync(id);
        }
        public async Task DeleteCachedMetadataAsync(string id)
        {
            await this.cache.DeleteMetadataAsync(id);
        }
        #endregion
        #region ALIAS
        /// <summary>
        /// Add an entry to the Alias table.<para/>
        /// The aliasUrl is like a virtuell Url, the data is stored under url only. 
        /// </summary>
        /// <param name="aliasUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task AddCachedAliasUrlAsync(string aliasUrl, string url)
        {
            await this.cache.SetAliasAsync(aliasUrl, url);
        }
        public async Task<string> GetCachedAliasUrlAsync(string aliasUrl)
        {
            return await this.cache.GetUrlFromAliasAsync(aliasUrl);
        }
        public async Task DeleteCachedAliasAsync(string aliasUrl)
        {
            await this.cache.DeleteAliasAsync(aliasUrl);
        }
        #endregion


        #region DATABAINDING

        public string DBName { get { return this.cache.DBName(); } }
        public string DBPath { get { return this.cache.DBPath(); } }
        public string DBSize
        {
            get
            {
                long ret = 0;
                Task.Run(async () =>
                {
                    ret = await this.cache.SizeAsync();
                }).Wait();
                return ret.ToString();
            }
        }

        public string DBCount { get {
                int ret = 0;
                Task.Run(async () =>
                {
                    ret = await this.cache.CountAsync();
                }).Wait();
                return ret.ToString();
            } }

        public async Task<IEnumerable<SqLiteCacheItem>> DBEntriesAsync(string urlPattern)
        {
            return await this.cache.GetEntriesAsync(urlPattern);
        }

        public async Task<SqLiteCacheItem> DBEntryAsync(string url)
        {
            return await this.cache.GetEntryAsync(url);
        }

        #endregion DATABAINDING
    }

   
}