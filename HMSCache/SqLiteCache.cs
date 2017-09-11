using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class SqLiteCacheItem : IDataItem
    {
        [PrimaryKey]
        public string url { get; set; }

        public byte[] data { get; set; }
        public byte[] header { get; set; }
        public byte zipped { get; set; }
        public byte encrypted { get; set; }
        public DateTime lastWrite { get; set; }
        public DateTime lastRead { get; set; }
        public DateTime expire { get; set; }
        public long size { get; set; }
        public Boolean dontRemove { get; set; }

        public SqLiteCacheItem()
        {
            this.zipped = 1;
            this.encrypted = 0;
            this.lastWrite = DateTime.Now;
            this.lastRead = DateTime.Now;
            this.header = null;
        }

        public SqLiteCacheItem(SqLiteCacheItem src)
        {
            this.data = src.data;
            this.encrypted = src.encrypted;
            this.expire = src.expire;
            this.header = src.header;
            this.lastRead = src.lastRead;
            this.lastWrite = src.lastWrite;
            this.size = src.size;
            this.zipped = src.zipped;
        }
    }

    public class SqLiteMetadata
    {
        [PrimaryKey]
        public string tag { get; set; }

        public string value { get; set; }
    }

    public class SqLiteAlias
    {
        [PrimaryKey]
        public string aliasUrl { get; set; }

        public string url { get; set; }
    }

    public class SqLiteCache : IDataProvider
    {
        private readonly string server;

        private SQLiteAsyncConnection sqlite3 ;

        private readonly ISql platformSQL;

        private readonly Boolean isReadonly ;

        public string errMsg { get;  set; }

        public SqLiteCache(ISql SQL, string server, Boolean isReadonly = false)
        {
            this.isReadonly = isReadonly;
            this.platformSQL = SQL;
            try
            {
                this.server = server;
                // open/create the SQLite db
                sqlite3 = SQL.GetAsyncConnection();
                // await this.CreateAsync();
            }
            catch (Exception ex)
            {
                // ToDo log this error
                this.errMsg = ex.Message;
            }
        }

        public async Task CreateAsync()
        {
            // ToDo: what about migrating the database?
            /* we may get exceptions here when we have breaking changes
             * this must be logged in some way
             */
            try
            {
                await sqlite3.CreateTableAsync<SqLiteAlias>().ContinueWith(async t =>
                {
                    try
                    {
                        await sqlite3.CreateTableAsync<SqLiteMetadata>().ContinueWith(async t2 => {
                            try
                            {
                                await sqlite3.CreateTableAsync<SqLiteCacheItem>();
                            }
                            catch (Exception ex)
                            {
                                throw new HccException("Error creating table SqLiteCacheItem ", ex);
                            }

                            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.tag == "hcc.version");
                            int anz = await entry.CountAsync();
                            if (anz == 0)
                            {
                                SqLiteMetadata md = new SqLiteMetadata();
                                md.tag = "hcc.version";
                                md.value = "1.2";
                                await sqlite3.InsertAsync(md);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new HccException("Error creating table SqLiteMetadata ", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                throw new HccException("Error creating table SqLiteAlias ", ex);
            }



            
            return ;
        }

        public async Task ResetAsync()
        {
            await this.CloseAsync().ContinueWith((t) =>
            {
                this.platformSQL.Reset();
                this.Reopen();
                this.CreateAsync();
            });
        }

        private void Reopen()
        {
            sqlite3 = platformSQL.GetAsyncConnection();
        }

        private async Task CloseAsync()
        {
            if (sqlite3 != null)
            {
                await Task.Factory.StartNew(() =>
                {
                    SQLite.SQLiteAsyncConnection.ResetPool();
                    // sqlite3.GetConnection().Close();
                    // sqlite3.GetConnection().Dispose();
                    sqlite3 = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }).ConfigureAwait(false);
            }
        }

        public async  Task<Byte[]> GetBytesAsync()
        {
            await this.CloseAsync();
            Byte[] bytes = this.platformSQL.GetBytes();
            this.Reopen();
            return bytes;
        }

        public async Task SetBytesAsync(Byte[] bytes)
        {
            await this.CloseAsync();
            this.platformSQL.SetBytes(bytes);
            this.Reopen();
        }

        public string DBName()
        {
            return HttpCachedClient._dbName;
        }

        public string DBPath()
        {
            return platformSQL.GetDBName();
        }

        public long Reduce(long maxSize = 0, long maxCount = 0)
        {
            if (maxSize > 0)
            {
            }
            else if (maxCount > 0)
            {
            }
            throw new NotImplementedException("not implemented");
        }

        public async Task DeleteAllDataAsync()
        {
            await sqlite3.ExecuteAsync("DELETE FROM " + typeof(SqLiteCacheItem).Name);
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await sqlite3.Table<SqLiteCacheItem>().CountAsync();
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<long> SizeAsync()
        {
            long qry = 0;

            try
            {
                qry = await sqlite3.ExecuteScalarAsync<long>("Select Sum(Size) as SIZE from " + typeof(SqLiteCacheItem).Name);
            }
            catch (Exception)
            {
                // this gets throws when there are no entries in the table
                qry = 0;
            }

            return qry;
        }

        private string clearUrl(string url)
        {
            if (url.StartsWith(this.server, StringComparison.CurrentCulture))
            {
                return url.Substring(this.server.Length);
            }
            return url;
        }

        public async Task<IDataItem> GetInfoAsync(string url)
        {
            SqLiteCacheItem retEntry = null;
            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (await entry.CountAsync() > 0)
            {
                retEntry = new SqLiteCacheItem(await entry.FirstAsync());
            }
            return retEntry;
        }

        public async Task<string> GetStringAsync(string url)
        {
            byte[] data = await GetDataAsync(url);
            if (data == null)
                return null;
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public async Task<IEnumerable<SqLiteCacheItem>> GetEntriesAsync(string urlPattern)
        {
            return await sqlite3.Table<SqLiteCacheItem>().Where(i => i.url.Contains(urlPattern)).ToListAsync();
        }

        public async Task<SqLiteCacheItem> GetEntryAsync(string url)
        {
            return await sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url).FirstOrDefaultAsync();
        }

        public async Task<string> GetMetadataAsync(string tag)
        {
            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.tag == tag);

            int anz = await entry.CountAsync();
            if (anz > 0)
            {
                return (await entry.FirstAsync()).value;
            }

            return null;
        }

        public async Task SetAliasAsync(string aliasUrl, string url)
        {
            SqLiteAlias alias = new SqLiteAlias();
            alias.url = url;
            var entry = sqlite3.Table<SqLiteAlias>().Where(i => i.aliasUrl == aliasUrl);

            if (await entry.CountAsync() > 0)
            {
                await sqlite3.DeleteAsync(alias);
            }
            alias.aliasUrl = aliasUrl;
            await sqlite3.InsertAsync(alias);
        }

        public async Task<string> GetUrlFromAliasAsync(string aliasUrl)
        {
            string url = aliasUrl;
            var entry = await sqlite3.Table<SqLiteAlias>().Where(i => i.aliasUrl == aliasUrl).FirstOrDefaultAsync();

            if ( entry?.url != null)
            {
                url = entry.url;
            }
            return url;
        }

        public async Task<Byte[]> GetDataAsync(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (await entry.CountAsync() > 0)
            {
                SqLiteCacheItem sqlEntry = await entry.FirstAsync();
                if (sqlEntry.data != null)
                {
                    byte[] data = sqlEntry.data;
                    if (sqlEntry.encrypted == 1)
                    {
                        data = sqlEntry.data;
                    }
                    if (sqlEntry.zipped == 1)
                    {
                        data = GZip.Decompress(data, 0, data.Length);
                    }
                    if ( !this.isReadonly )
                    {
                        // set the lastRead
                        await this.updateLastReadAsync(sqlEntry.url);
                    }
                    return data;
                }
            }
            return null;
        }

        private async Task updateLastReadAsync(string url)
        {
            await sqlite3.ExecuteAsync("Update " + typeof(SqLiteCacheItem).Name + " set lastRead = ? Where Url = ?", DateTime.Now, url);
        }

        public async Task<HccHttpHeaders> GetHeadersAsync(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (await entry.CountAsync() > 0)
            {
                SqLiteCacheItem sqlEntry = await entry.FirstAsync();
                if (sqlEntry.header != null)
                {
                    byte[] headerData = sqlEntry.header;
                    headerData = GZip.Decompress(headerData, 0, headerData.Length);
                    string headerString = Encoding.UTF8.GetString(headerData, 0, headerData.Length);
                    return GetHeadersFromString(headerString);
                }
            }
            return null;
        }

        public HccHttpHeaders GetHeadersFromString(string headerString)
        {
            HccHttpHeaders httpHeaders = new HccHttpHeaders();
            string[] lines = headerString.Split(new string[] { Environment.NewLine, @"\r", @"\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines)
            {
                int pos = l.IndexOf(": ",StringComparison.CurrentCulture);
                string key = l.Substring(0, pos);
                string value = l.Substring(pos + 2);
                string[] values = value.Split(';');
                httpHeaders.items.Add(key, values);
            }

            return httpHeaders;
        }

        public async Task<string[]> GetIDsAsync(string pattern, int SqlLimit)
        {
            List<string> ret = new List<string>();
            string SQL = "SELECT url from " + typeof(SqLiteCacheItem).Name + " where url LIKE '%" + pattern + "%'";
            SQL += " ORDER BY LastRead DESC ";
            SQL += " LIMIT " + SqlLimit.ToString();
            var entries = (await sqlite3.QueryAsync<SqLiteCacheItem>(SQL, new string[] { })).ToArray();
            foreach (var entry in entries)
            {
                ret.Add(entry.url);
            }

            return ret.ToArray();
        }

        public async Task SetMetadataAsync(string tag, string value)
        {
            SqLiteMetadata md = new SqLiteMetadata();
            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.tag == tag);

            md.tag = tag;
            if (await entry.CountAsync() > 0)
            {
                await sqlite3.DeleteAsync(md);
            }
            md.value = value;
            await sqlite3.InsertAsync(md);
        }

        public async Task SetStringAsync(string url, string data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            await SetDataAsync(url, Encoding.UTF8.GetBytes(data), headers, overwrite, zipped, encrypted);
        }

        public async Task SetDataAsync(string url, Byte[] data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            url = this.clearUrl(url);

            if (await ExistsAsync(url))
            {
                if ( !overwrite )
                    return;
                await DeleteAsync(url);
            }
            SqLiteCacheItem ci = new SqLiteCacheItem();
            ci.url = url;
            ci.data = data;
            ci.encrypted = encrypted;

            if (zipped == 1)
            {
                ci.data = GZip.Compress(ci.data, 0, ci.data.Length);
            }
            ci.zipped = zipped;
            ci.size = ci.data.Length;
            if (!string.IsNullOrEmpty(headers))
            {
                byte[] headerData = Encoding.UTF8.GetBytes(headers);
                ci.header = GZip.Compress(headerData, 0, headerData.Length);
                ci.size += ci.header.Length;
            }
            try
            {
                await sqlite3.InsertAsync(ci);
            }
            catch (Exception )
            {
                throw;
            }
        }

        public async Task DeleteAsync(string url)
        {
            url = this.clearUrl(url);

            await sqlite3.DeleteAsync(new SqLiteCacheItem() { url = url });
        }

        public async Task<Boolean> ExistsAsync(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            return await entry.CountAsync() > 0;
        }
    }

    internal class SqLiteScalar
    {
        public object value;

        public string asString()
        {
            return value.ToString();
        }
    }

    public class HccException:Exception
    {
        public HccException() : base()
        {
        }

        public HccException(string message) : base(message)
        {
        }

        public HccException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}