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
        iDataItem GetInfo(string id);
        hccHttpHeaders GetHeaders(string id);
        hccHttpHeaders GetHeadersFromString(string headerString);

        string[] GetIDs(string pattern,int SqlLimit);

        void SetString(string id, string data, string headers="", Boolean overwrite=true, byte zipped = 1, byte encrypted = 0);
        void SetData(string id, Byte[] data, string headers="", Boolean overwrite=true, byte zipped = 1, byte encrypted = 0);
        void Delete(string id);
        Boolean Exists(string id);
        /// <summary>
        /// remove all entries from the cache
        /// </summary>
        void ClearData();
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
        
        /// <summary>
        /// reduce the size of the cache
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        long Reduce(long maxSize=0, long maxCount=0);
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
