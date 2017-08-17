using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HMS.Net.Http
{
    public interface iDataProvider
    {
        string GetString(string id);
        Byte[] GetData(string id);
        IEnumerable<SqLiteCacheItem> GetEntries(string urlPattern);
        SqLiteCacheItem GetEntry(string url);
        iDataItem GetInfo(string id);
        hccHttpHeaders GetHeaders(string id);
        hccHttpHeaders GetHeadersFromString(string headerString);

        string[] GetIDs(string pattern,int SqlLimit);
        string GetMetadata(string id);
        string GetUrlFromAlias(string aliasUrl);
        void SetAlias(string aliasUrl, string url);
        void SetMetadata(string id, string data);
        void SetString(string id, string data, string headers="", Boolean overwrite=true, byte zipped = 1, byte encrypted = 0);
        void SetData(string id, Byte[] data, string headers="", Boolean overwrite=true, byte zipped = 1, byte encrypted = 0);
        void Delete(string id);
        Boolean Exists(string id);
        /// <summary>
        /// remove all entries from the cache
        /// </summary>
        void DeleteAllData();
        /// <summary>
        /// get the number of entries
        /// </summary>
        /// <returns></returns>
        long Count();
        /// <summary>
        /// get the number of bytes in the cache
        /// </summary>
        /// <returns></returns>
        long Size();

        string DBName();
        string DBPath();
        /// <summary>
        /// reduce the size of the cache
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        long Reduce(long maxSize=0, long maxCount=0);

        Byte[] GetBytes();
        void SetBytes(Byte[] bytes);

    }
    public interface iDataItem
    {        
        byte zipped { get; set; }
        byte encrypted { get; set; }
        DateTime lastWrite { get; set; }
        DateTime lastRead { get; set; }
        DateTime expire { get; set; }
        long size { get; set; }
        Boolean dontRemove { get; set; }
    }
}
