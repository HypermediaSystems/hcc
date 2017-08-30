using System;
using System.Collections.Generic;

namespace HMS.Net.Http
{
    public interface IDataProvider
    {
        string GetString(string url);

        Byte[] GetData(string url);

        IEnumerable<SqLiteCacheItem> GetEntries(string urlPattern);

        SqLiteCacheItem GetEntry(string url);

        IDataItem GetInfo(string url);

        HccHttpHeaders GetHeaders(string url);

        HccHttpHeaders GetHeadersFromString(string headerString);

        string[] GetIDs(string pattern, int SqlLimit);

        string GetMetadata(string tag);

        string GetUrlFromAlias(string aliasUrl);

        void SetAlias(string aliasUrl, string url);

        void SetMetadata(string tag, string value);

        void SetString(string url, string data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0);

        void SetData(string url, Byte[] data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0);

        void Delete(string url);

        Boolean Exists(string url);

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
        long Reduce(long maxSize = 0, long maxCount = 0);

        Byte[] GetBytes();

        void SetBytes(Byte[] bytes);

        void Reset();
    }

    public interface IDataItem
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