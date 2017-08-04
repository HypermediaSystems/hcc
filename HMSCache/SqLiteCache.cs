using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class SqLiteCacheItemAttribute : Attribute
    {
        /// <summary>
        /// we use this value when we have to migrate an existing database
        /// </summary>
        public string Version { get; set; }
    }
    public class SqLiteCacheItem: iDataItem
    {
        [SqLiteCacheItemAttribute(Version = "1.0")]
        [PrimaryKey]
        public string url { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.0")]
        public byte[] data { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.0")]
        public byte[] header { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.0")]
        public byte zipped { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.0")]
        public byte encrypted { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.1")]
        public DateTime lastWrite { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.1")]
        public DateTime lastRead { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.1")]
        public DateTime expire { get; set; }
        [SqLiteCacheItemAttribute(Version = "1.1")]
        public long size { get; set; }

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
        public string version { get; set; }
        public SqLiteMetadata()
        {
            this.version = "1.1";
        }
    }
    public class SqLiteCache: iDataProvider
    {
        string server;
        SQLiteAsyncConnection sqlite3Async;
        SQLiteConnection sqlite3;
        public SqLiteCache(iSQL SQL, string server)
        {
            try
            {
                this.server = server;
                // open/create the SQLite db            
                sqlite3 = SQL.GetConnection();

                sqlite3.CreateTable<SqLiteCacheItem>();
                sqlite3.CreateTable<SqLiteMetadata>();
                if (sqlite3.Table<SqLiteMetadata>().Count() == 0)
                {
                    sqlite3.Insert(new SqLiteMetadata());
                }

            }
            catch (Exception)
            {

            }
        }
        public Boolean Migrate()
        {
            throw new Exception("not implemented");

            // return false;
        }
        public long Reduce(long maxSize = 0, long maxCount = 0)
        {
            if( maxSize > 0 )
            {

            }
            else if(maxCount > 0)
            {

            }
            throw new Exception("not implemented");

            // return 0;
        }
        public void ClearData()
        {
            sqlite3.Execute("DELETE * FROM " + typeof(SqLiteCacheItem).Name);            
        }
        public long Count()
        {
            return sqlite3.Table<SqLiteCacheItem>().Count();
        }
        public long Size()
        {
            var qry =  sqlite3.ExecuteScalar<long>("Select Sum(Size) as SIZE from " + typeof(SqLiteCacheItem).Name);

            return qry;
        }
        private string clearUrl(string url)
        {
            if (url.StartsWith(this.server))
            {
                return url.Substring(this.server.Length);
            }
            return url;
        }
        public iDataItem GetInfo(string url)
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
                        // data = sqlEntry.data;
                    }
                    if (sqlEntry.zipped == 1)
                    {
                        data = gzip.Decompress(data, 0, data.Length);
                    }

                    // set the lastRead
                    this.updateLastRead(sqlEntry.url);
                    
                    return data;
                }
            }
            return null;
        }
        private void updateLastRead(string url)
        {
            sqlite3.Execute("Update " + typeof(SqLiteCacheItem).Name + " set lastRead = ? Where Url = ?",DateTime.Now,url);
        }
        public hccHttpHeaders GetHeaders(string url)
        {
            hccHttpHeaders httpHeaders = new hccHttpHeaders();
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (entry.Count() > 0)
            {
                SqLiteCacheItem sqlEntry = entry.First();
                if (sqlEntry.header != null)
                {
                    byte[] headerData = sqlEntry.header;
                    headerData = gzip.Decompress(headerData, 0, headerData.Length);
                    string headerString = Encoding.UTF8.GetString(headerData, 0, headerData.Length);
                    string[] lines = headerString.Split(new string[]{ Environment.NewLine, @"\r", @"\n"},StringSplitOptions.RemoveEmptyEntries);
                    foreach(var l in lines)
                    {
                        int pos = l.IndexOf(": ");
                        string key = l.Substring(0, pos);
                        string value = l.Substring(pos +2);
                        string[] values = value.Split(';');
                        httpHeaders.items.Add(key, values);
                    }

                    return httpHeaders;
                }
            }
            return null;
        }
        public void SetString(string url, string data,string headers="", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            SetData(url, Encoding.UTF8.GetBytes(data), headers,overwrite, zipped, encrypted);
        }
        
        public void SetData(string url, Byte[] data,string headers="", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            url = this.clearUrl(url);

            if (Exists(url))
            {
                if (overwrite == false)
                    return;
                Delete(url);
            }
            SqLiteCacheItem ci = new SqLiteCacheItem();
            ci.url = url;
            ci.data = data;
            if (encrypted == 1)
            {
                // ci.data = ci.data;
            }
            if (zipped == 1)
            {
                ci.data = gzip.Compress(ci.data, 0, ci.data.Length);
                ci.zipped = zipped;
            }
            ci.size = ci.data.Length;
            if( !string.IsNullOrEmpty(headers) )
            {
                byte[] headerData = Encoding.UTF8.GetBytes(headers);
                ci.header = gzip.Compress(headerData, 0, headerData.Length);
                ci.size += ci.header.Length;
            }
            try
            {
                sqlite3.Insert(ci);
            }
            catch (Exception ex)
            {
                throw ex;
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
}
