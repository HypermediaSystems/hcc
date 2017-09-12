using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
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
}
