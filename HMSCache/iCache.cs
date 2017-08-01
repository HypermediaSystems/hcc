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
        void SetString(string id, string data, string headers="", Boolean overwrite=true, byte zipped = 1, byte encrypted = 0);
        void SetData(string id, Byte[] data, string headers="", Boolean overwrite=true, byte zipped = 1, byte encrypted = 0);
        void Delete(string id);
        Boolean Exists(string id);
        void ClearData(int remain);
        long Count();
    }
    public interface iDataItem
    {
        byte zipped { get; set; }
        byte encrypted { get; set; }
        DateTime loaded { get; set; }
        DateTime expire { get; set; }
        
    }
}
