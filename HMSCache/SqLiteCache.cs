using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class SqLiteCacheItem
    {
        [PrimaryKey]
        public string url { get; set; }
        public byte[] data { get; set; }
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
            sqlite3.CreateTable<SqLiteCacheItem>();
        }
        public void ClearData(int remain)
        {
            sqlite3.Execute("DELETE * FROM CacheItem");
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
        public string GetString(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (entry.Count() > 0)
            {
                if(entry.First().data != null )
                    return Encoding.UTF8.GetString(entry.First().data, 0, entry.First().data.Length);
                Delete(url);
            }
            return null;
            
        }
        public Byte[] GetData(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            if (entry.Count() > 0)
            {
                if (entry.First().data != null)
                    return entry.First().data;
                Delete(url);
            }
            return null;
        }
        public void SetString(string url, string data)
        {
            url = this.clearUrl(url);

            SetString(url, data, true);
        }
        public void SetString(string url, string data, Boolean overwrite)
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
            ci.data = Encoding.UTF8.GetBytes(data);

            try
            {
                sqlite3.Insert(ci);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void SetData(string url, Byte[] data)
        {
            SetData(url, data, true);
        }
        public void SetData(string url, Byte[] data, Boolean overwrite)
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
