using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public interface iSQL
    {
        Boolean Reset();
        SQLiteConnection GetConnection();
        SQLiteAsyncConnection GetAsyncConnection();

        string GetDBName();

        Byte[] GetBytes();
        void SetBytes(Byte[] bytes);
    }
}
