﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public partial class SqLiteCache : IDataProvider
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
        #region SqLite Layer
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
        /// <summary>
        /// Replace the SqLite database by an empty one, containing only the required tables<para/>
        /// This will delete all data.
        /// </summary>
        /// <returns></returns>
        public async Task ResetAsync()
        {
            await this.CloseAsync();

            this.platformSQL.Reset();
            this.Reopen();

            await this.CreateAsync();
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

        #endregion

       
        #region EntryInfo
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


        #endregion
        
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