using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public partial class SqLiteCache : IDataProvider
    {
        #region METADATA
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
        public async Task SetMetadataAsync(string tag, string value)
        {
            await this.DeleteMetadataAsync(tag);
            SqLiteMetadata md = new SqLiteMetadata();
            md.tag = tag;
            md.value = value;
            await sqlite3.InsertAsync(md);
        }
        public async Task DeleteMetadataAsync(string tag)
        {
            SqLiteMetadata md = new SqLiteMetadata();
            var entry = sqlite3.Table<SqLiteMetadata>().Where(i => i.tag == tag);

            md.tag = tag;
            if (await entry.CountAsync() > 0)
            {
                await sqlite3.DeleteAsync(await entry.FirstOrDefaultAsync());
            }
        }
        #endregion
    }
}
