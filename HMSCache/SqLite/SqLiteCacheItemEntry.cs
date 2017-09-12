using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public partial class SqLiteCache : IDataProvider
    {
        #region Delete
        public async Task DeleteAsync(string url)
        {
            url = this.clearUrl(url);

            await sqlite3.DeleteAsync(new SqLiteCacheItem() { url = url });
        }
        #endregion
       
        #region Get
        public async Task<string> GetStringAsync(string url)
        {
            byte[] data = await GetDataAsync(url);
            if (data == null)
                return null;
            return Encoding.UTF8.GetString(data, 0, data.Length);
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
                    if (!this.isReadonly)
                    {
                        // set the lastRead
                        await this.updateLastReadAsync(sqlEntry.url);
                    }
                    return data;
                }
            }
            return null;
        }
        #endregion

        #region Set
        public async Task SetStringAsync(string url, string data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            await SetDataAsync(url, Encoding.UTF8.GetBytes(data), headers, overwrite, zipped, encrypted);
        }

        public async Task SetDataAsync(string url, Byte[] data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0)
        {
            url = this.clearUrl(url);

            if (await ExistsAsync(url))
            {
                if (!overwrite)
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
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Query
        public async Task<Boolean> ExistsAsync(string url)
        {
            url = this.clearUrl(url);

            var entry = sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url);

            return await entry.CountAsync() > 0;
        }
        public async Task<IEnumerable<SqLiteCacheItem>> GetEntriesAsync(string urlPattern)
        {
            return await sqlite3.Table<SqLiteCacheItem>().Where(i => i.url.Contains(urlPattern)).ToListAsync();
        }

        public async Task<SqLiteCacheItem> GetEntryAsync(string url)
        {
            return await sqlite3.Table<SqLiteCacheItem>().Where(i => i.url == url).FirstOrDefaultAsync();
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
        #endregion

    }
}
