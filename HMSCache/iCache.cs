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
        void SetString(string id, string data);
        void SetString(string id, string data, Boolean overwrite);
        void SetData(string id, Byte[] data);
        void SetData(string id, Byte[] data, Boolean overwrite);
        void Delete(string id);
        Boolean Exists(string id);
        void ClearData(int remain);
        long Count();
    }
    
}
