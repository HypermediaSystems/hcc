using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public partial class SqLiteCache : IDataProvider
    {
        #region Alias
        public async Task SetAliasAsync(string aliasUrl, string url)
        {
            SqLiteAlias alias = new SqLiteAlias();
            alias.aliasUrl = aliasUrl;
            var entry = sqlite3.Table<SqLiteAlias>().Where(i => i.aliasUrl == aliasUrl);

            if (await entry.CountAsync() > 0)
            {
                await sqlite3.DeleteAsync(await entry.FirstOrDefaultAsync());
            }
            alias.url = url;
            await sqlite3.InsertAsync(alias);
        }
        public async Task DeleteAliasAsync(string aliasUrl)
        {
            SqLiteAlias alias = new SqLiteAlias();
            alias.aliasUrl = aliasUrl;

            var entry = sqlite3.Table<SqLiteAlias>().Where(i => i.aliasUrl == aliasUrl);

            if (await entry.CountAsync() > 0)
            {
                await sqlite3.DeleteAsync(await entry.FirstOrDefaultAsync());
            }
        }
        public async Task<string[]> ListAliasAsync()
        {
            var list = await sqlite3.QueryAsync<SqLiteAlias>("select aliasUrl from SqLiteAlias").ConfigureAwait(false);

            return list.Select<SqLiteAlias,string>( x => x.aliasUrl).ToArray<string>();

        }
        public async Task<string> GetUrlFromAliasAsync(string aliasUrl)
        {
            string url = aliasUrl;
            var entry = await sqlite3.Table<SqLiteAlias>().Where(i => i.aliasUrl == aliasUrl).FirstOrDefaultAsync();

            if (entry?.url != null)
            {
                url = entry.url;
            }
            return url;
        }
        #endregion

    }
}
