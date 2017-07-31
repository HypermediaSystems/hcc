using PCLStorage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMS.Net.Http;

using HMS.Net.Http.UWP.SQLImplementation;

[assembly: Xamarin.Forms.Dependency(typeof(SqlUWP))]
namespace HMS.Net.Http.UWP.SQLImplementation
{
    class SqlUWP : iSQL
    {
        IFolder folder;
        string SqlConnectionString;
        string SqlDBName;
        public SqlUWP(string dbName)
        {
            Boolean fehler = false;
            folder = FileSystem.Current.LocalStorage;

            SqlDBName = PortablePath.Combine(folder.Path, dbName + ".sqlite");

            SqlConnectionString = SqlDBName; // "Data Source=" +
            if (!System.IO.File.Exists(SqlDBName))
            {
                fehler = true;
            }
            else
            {
                fehler = false;

            }
        }
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }
        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return new SQLiteAsyncConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }
    }

}
