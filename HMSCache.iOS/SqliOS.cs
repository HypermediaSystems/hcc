using PCLStorage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMS.Net.Http;

using HMS.Net.Http.iOS.SQLImplementation;
using Xamarin.Forms.Internals;

[assembly: Xamarin.Forms.Dependency(typeof(SqliOS))]
namespace HMS.Net.Http.iOS.SQLImplementation
{
    [Preserve]
    public class SqliOS : iSQL
    {
        IFolder folder;
        string SqlConnectionString;
        string SqlDBName;
        public SqliOS()
        {
            string dbName = HttpCachedClient.dbName;

            folder = FileSystem.Current.LocalStorage;

            SqlDBName = PortablePath.Combine(folder.Path, dbName + ".sqlite");

            SqlConnectionString = SqlDBName; // "Data Source=" +
        }
        public Boolean Reset()
        {
            Boolean ret = false;
            if (System.IO.File.Exists(SqlDBName))
            {
                System.IO.File.Delete(SqlDBName);
                ret = true;
            }
            return ret;
        }
        public string GetDBName()
        {
            return SqlDBName;
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex); //  
        }
        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return new SQLiteAsyncConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create );
        }
    }

}
