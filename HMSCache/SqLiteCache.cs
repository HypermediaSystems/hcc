using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

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

        private SQLiteConnection sqlite3 ;

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
                sqlite3 = SQL.GetConnection();
                this.Create();
            }
            catch (Exception ex)
            {
                // ToDo log this error
                this.errMsg = ex.Message;
            }
        }

        private void Create()
        {
            // ToDo: what about migrating the database?
            sqlite3.CreateTable<SqLiteAlias>();
            sqlite3.CreateTable<SqLiteMetadata>();
            sqlite3.CreateTable<SqLiteCacheItem>();

            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.tag == "hcc.version");
            if (entry.Count() == 0)
            {
                SqLiteMetadata md = new SqLiteMetadata();
                md.tag = "hcc.version";
                md.value = "1.2";
                sqlite3.Insert(md);
            }
        }

        public void Reset()
        {
            this.Close();
            this.platformSQL.Reset();
            this.Reopen();
            this.Create();
        }

        private void Reopen()
        {
            sqlite3 = platformSQL.GetConnection();
        }

        private void Close()
        {
            if (sqlite3 != null)
            {
                sqlite3.Close();
                sqlite3.Dispose();
            }
        }

        public Byte[] GetBytes()
        {
            this.Close();
            Byte[] bytes = this.platformSQL.GetBytes();
            this.Reopen();
            return bytes;
        }

        public void SetBytes(Byte[] bytes)
        {
            this.Close();
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
            throw new Exception("not implemented");
        }

        public void DeleteAllData()
        {
            sqlite3.Execute("DELETE FROM " + typeof(SqLiteCacheItem).Name);
        }

        public long Count()
        {
            return sqlite3.Table<SqLiteCacheItem>().Count();
        }

        public long Size()
        {
            long qry = 0;

            try
            {
                qry = sqlite3.ExecuteScalar<long>("Select Sum(Size) as SIZE from " + typeof(SqLiteCacheItem).Name);
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

        public IDataItem GetInfo(string url)
        {
            SqLiteCacheItem retEntry = null;
            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (entry.Count() > 0)
            {
                retEntry = new SqLiteCacheItem(entry.First());
            }
            return retEntry;
        }

        public string GetString(string url)
        {
            byte[] data = GetData(url);
            if (data == null)
                return null;
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public IEnumerable<SqLiteCacheItem> GetEntries(string urlPattern)
        {
            return sqlite3.Table<SqLiteCacheItem>().Where(i => i.url.Contains(urlPattern));
        }

        public SqLiteCacheItem GetEntry(string url)
        {
            return sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url).FirstOrDefault();
        }

        public string GetMetadata(string tag)
        {
            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.tag == tag);

            if (entry.Count() > 0)
            {
                return entry.First().value;
            }

            return null;
        }

        public void SetAlias(string aliasUrl, string url)
        {
            SqLiteAlias alias = new SqLiteAlias();
            alias.url = url;
            alias.aliasUrl = aliasUrl;
            var entry = sqlite3.Table<SqLiteAlias>().Where(i => i.aliasUrl == aliasUrl);

            if (entry.Count() > 0)
            {
                sqlite3.Delete<SqLiteAlias>(aliasUrl);
            }
            sqlite3.Insert(alias);
        }

        public string GetUrlFromAlias(string aliasUrl)
        {
            string url = aliasUrl;
            var entry = sqlite3.Table<SqLiteAlias>().Where(i => i.aliasUrl == aliasUrl).FirstOrDefault();

            if ( entry?.url != null)
            {
                url = entry.url;
            }
            return url;
        }

        public Byte[] GetData(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (entry.Count() > 0)
            {
                SqLiteCacheItem sqlEntry = entry.First();
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
                        this.updateLastRead(sqlEntry.url);
                    }
                    return data;
                }
            }
            return null;
        }

        private void updateLastRead(string url)
        {
            sqlite3.Execute("Update " + typeof(SqLiteCacheItem).Name + " set lastRead = ? Where Url = ?", DateTime.Now, url);
        }

        public HccHttpHeaders GetHeaders(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (entry.Count() > 0)
            {
                SqLiteCacheItem sqlEntry = entry.First();
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

        public string[] GetIDs(string pattern, int SqlLimit)
        {
            List<string> ret = new List<string>();
            string SQL = "SELECT url from " + typeof(SqLiteCacheItem).Name + " where url LIKE '%" + pattern + "%'";
            SQL += " ORDER BY LastRead DESC ";
            SQL += " LIMIT " + SqlLimit.ToString();
            var entries = sqlite3.Query<SqLiteCacheItem>(SQL, new string[] { });
            foreach (var entry in entries)
            {
                ret.Add(entry.url);
            }

            return ret.ToArray();
        }

        public void SetMetadata(string tag, string value)
        {
            SqLiteMetadata md = new SqLiteMetadata();
            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.tag == tag);

            if (entry.Count() > 0)
            {
                sqlite3.Delete<SqLiteMetadata>(tag);
            }
            md.tag = tag;
            md.value = value;
            sqlite3.Insert(md);
        }

        public void SetString(string url, string data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            SetData(url, Encoding.UTF8.GetBytes(data), headers, overwrite, zipped, encrypted);
        }

        public void SetData(string url, Byte[] data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            url = this.clearUrl(url);

            if (Exists(url))
            {
                if ( !overwrite )
                    return;
                Delete(url);
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
                sqlite3.Insert(ci);
            }
            catch (Exception )
            {
                throw;
            }
        }

        public void Delete(string url)
        {
            url = this.clearUrl(url);

            sqlite3.Delete<SqLiteCacheItem>(url);
        }

        public Boolean Exists(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            return entry.Count() > 0;
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
}