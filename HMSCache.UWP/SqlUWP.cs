using PCLStorage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMS.Net.Http;

using HMS.Net.Http.UWP.SQLImplementation;
using Xamarin.Forms.Internals;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(SqlUWP))]
namespace HMS.Net.Http.UWP.SQLImplementation
{
    [Foundation.Preserve(AllMembers = true)]
    public class SqlUWP : iSQL
    {
        IFolder folder;
        string SqlConnectionString;
        string SqlDBName;
        [Preserve]
        public SqlUWP()
        {
            string dbName = HttpCachedClient.dbName;

            folder = FileSystem.Current.LocalStorage;

            SqlDBName = PortablePath.Combine(folder.Path, dbName + ".sqlite");

            SqlConnectionString = SqlDBName; // "Data Source=" +
        }
        
        public Byte[] GetBytes()
        {
            byte[] bytes = null;
            using (FileStream file = new FileStream(SqlDBName, FileMode.Open, System.IO.FileAccess.Read))
            {
                bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
            }
            return bytes;
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
            return new SQLiteConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }
        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return new SQLiteAsyncConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }
    }

}
