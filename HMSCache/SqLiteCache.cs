﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class SqLiteCacheItem : iDataItem
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
        public Boolean dontRemove { get;set; }

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
        public string id { get; set; }
        public string data { get; set; }
        public string test { get; set; }
        public SqLiteMetadata()
        {
            
        }
    }
    public class SqLiteCache: iDataProvider
    {
        string server;
        SQLiteAsyncConnection sqlite3Async = null;
        SQLiteConnection sqlite3 = null;

        iSQL platformSQL;


        Boolean isReadonly = false;
        public SqLiteCache(iSQL SQL, string server, Boolean isReadonly = false)
        {
            this.isReadonly = isReadonly;
            platformSQL = SQL;
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
            return HttpCachedClient.dbName;
        }
        public string DBPath()
        {
            return platformSQL.GetDBName();
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
            }

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
        public IEnumerable<SqLiteCacheItem> GetEntries(string urlPattern)
        {
            return sqlite3.Table<SqLiteCacheItem>().Where(i => i.url.Contains(urlPattern));
        }
        public SqLiteCacheItem GetEntry(string url)
        {
            return sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url).FirstOrDefault();
        }
        public string GetMetadata(string id)
        {
            return sqlite3.Table<SqLiteMetadata>().Where(i => i.id == id).FirstOrDefault().data;
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
                    if (this.isReadonly == false)
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
            sqlite3.Execute("Update " + typeof(SqLiteCacheItem).Name + " set lastRead = ? Where Url = ?",DateTime.Now,url);
        }
        public hccHttpHeaders GetHeaders(string url)
        {
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
                    return GetHeadersFromString(headerString);
                }
            }
            return null;

        }
        public hccHttpHeaders GetHeadersFromString(string headerString)
        {
            hccHttpHeaders httpHeaders = new hccHttpHeaders();
            string[] lines = headerString.Split(new string[] { Environment.NewLine, @"\r", @"\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines)
            {
                int pos = l.IndexOf(": ");
                string key = l.Substring(0, pos);
                string value = l.Substring(pos + 2);
                string[] values = value.Split(';');
                httpHeaders.items.Add(key, values);
            }

            return httpHeaders;
        }
        public string[] GetIDs(string pattern,int SqlLimit)
        {
            List<string> ret = new List<string>();
            string SQL = "SELECT url from " + typeof(SqLiteCacheItem).Name + " where url LIKE '%" + pattern + "%'";
            SQL += " ORDER BY LastRead DESC ";
            SQL += " LIMIT " + SqlLimit.ToString();
            var entries = sqlite3.Query<SqLiteCacheItem>(SQL,new string[] { });
            foreach (var entry in entries)
            {
                ret.Add(entry.url);
            }

            return ret.ToArray();
        }
        public void SetMetadata(string id, string data)
        {
            SqLiteMetadata md = new SqLiteMetadata();
            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.id == id);

            if( entry.Count() > 0 )
            {
                sqlite3.Delete<SqLiteMetadata>(id);
            }
            md.id = id;
            md.data = data;
            sqlite3.Insert(md);
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
            ci.encrypted = encrypted;

            if (zipped == 1)
            {
                ci.data = gzip.Compress(ci.data, 0, ci.data.Length);
            }
            ci.zipped = zipped;
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
    class SqLiteScalar
    {
        public object value;
        public string asString()
        {
            return value.ToString();
        }
    }
}
