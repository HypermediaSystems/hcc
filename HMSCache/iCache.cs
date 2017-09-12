using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public interface IDataProvider
    {
        Task<string> GetStringAsync(string url);

        Task<Byte[]> GetDataAsync(string url);

        Task<IEnumerable<SqLiteCacheItem>> GetEntriesAsync(string urlPattern);

        Task<SqLiteCacheItem >GetEntryAsync(string url);

        Task<IDataItem> GetInfoAsync(string url);

        Task<HccHttpHeaders> GetHeadersAsync(string url);

        HccHttpHeaders GetHeadersFromString(string headerString);

        Task<string[]> GetIDsAsync(string pattern, int SqlLimit);

        Task<string> GetMetadataAsync(string tag);

        Task SetMetadataAsync(string tag, string value);
        Task DeleteMetadataAsync(string tag);

        Task<string> GetUrlFromAliasAsync(string aliasUrl);

        Task SetAliasAsync(string aliasUrl, string url);

        Task DeleteAliasAsync(string aliasUrl);

        Task SetStringAsync(string url, string data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0);

        Task SetDataAsync(string url, Byte[] data, string headers = "", Boolean overwrite = true, byte zipped = 1, byte encrypted = 0);

        Task DeleteAsync(string url);


        Task<Boolean> ExistsAsync(string url);

        /// <summary>
        /// remove all entries from the cache
        /// </summary>
        Task DeleteAllDataAsync();

        /// <summary>
        /// get the number of entries
        /// </summary>
        /// <returns></returns>
        Task<int> CountAsync();

        /// <summary>
        /// get the number of bytes in the cache
        /// </summary>
        /// <returns></returns>
        Task<long> SizeAsync();

        string DBName();

        string DBPath();

        /// <summary>
        /// reduce the size of the cache
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        long Reduce(long maxSize = 0, long maxCount = 0);

        Task<Byte[]> GetBytesAsync();

        Task SetBytesAsync(Byte[] bytes);

        Task ResetAsync();
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