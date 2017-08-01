using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class SqLiteCacheItem: iDataItem
    {
        [PrimaryKey]
        public string url { get; set; }
        public byte[] data { get; set; }
        public byte[] header { get; set; }
        public byte zipped { get; set; }
        public byte encrypted { get; set; }
        public DateTime loaded { get; set; }
        public DateTime expire { get; set; }

        public SqLiteCacheItem()
        {
            this.zipped = 1;
            this.encrypted = 0;
            this.loaded = DateTime.Now;
            this.header = null;
        }
        public SqLiteCacheItem(SqLiteCacheItem src)
        {
            this.zipped = src.zipped;
            this.encrypted = src.encrypted;
        }
    }
    public class SqLiteMetadata
    {
        [PrimaryKey]
        public string version { get; set; }
        public SqLiteMetadata()
        {
            this.version = "1.0";
        }
    }
    public class SqLiteCache: iDataProvider
    {
        string server;
        SQLiteAsyncConnection sqlite3Async;
        SQLiteConnection sqlite3;
        public SqLiteCache(iSQL SQL, string server)
        {
            this.server = server;
            // open/create the SQLite db            
            sqlite3 = SQL.GetConnection();

            sqlite3.CreateTable<SqLiteCacheItem>( );
            sqlite3.CreateTable<SqLiteMetadata>();
            if( sqlite3.Table<SqLiteMetadata>().Count() == 0 )
            {
                sqlite3.Insert(new SqLiteMetadata());
            }
        }
        public void ClearData(int remain)
        {
            sqlite3.Execute("DELETE * FROM " + typeof(SqLiteCacheItem).Name);            
        }
        public long Count()
        {
            return sqlite3.Table<SqLiteCacheItem>().Count();
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

                    return data;
                }
            }
            return null;
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
            if( !string.IsNullOrEmpty(headers) )
            {
                byte[] headerData = Encoding.UTF8.GetBytes(headers);
                ci.header = gzip.Compress(headerData, 0, headerData.Length);
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
