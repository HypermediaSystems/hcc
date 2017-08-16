using HMSCache;
using PCLStorage;
using SQLite;
using System;
using HMS.Net.Http;

using HMS.Net.Http.Android.SQLImplementation;
using Xamarin.Forms.Internals;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(SqlAndroid))]
namespace HMS.Net.Http.Android.SQLImplementation
{
    [Preserve]
    public class SqlAndroid : iSQL
    {
        IFolder folder;
        string SqlConnectionString;
        string SqlDBName;
        public SqlAndroid()
        {
            string dbName = HttpCachedClient.dbName;

            folder = FileSystem.Current.LocalStorage;

            SqlDBName = PortablePath.Combine(folder.Path,  dbName + ".sqlite");

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
        public void SetBytes(Byte[] bytes)
        {
            using (FileStream file = new FileStream(SqlDBName, FileMode.Open, System.IO.FileAccess.Write))
            {
                file.Write(bytes, 0, (int)bytes.Length);
            }
        }
        public Boolean Reset()
        {
            Boolean ret = false;
            if (System.IO.File.Exists(SqlDBName) )
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
            return new SQLiteConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);//| SQLiteOpenFlags.FullMutex
        }
        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return new SQLiteAsyncConnection(SqlConnectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }
    }
}
