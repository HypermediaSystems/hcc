using HMSCache;
using PCLStorage;
using SQLite;
using System;
using HMS.Net.Http;

using HMS.Net.Http.Android.SQLImplementation;

[assembly: Xamarin.Forms.Dependency(typeof(SqlAndroid))]
namespace HMS.Net.Http.Android.SQLImplementation
{
    public class SqlAndroid : iSQL
    {
        IFolder folder;
        string SqlConnectionString;
        string SqlDBName;
        public SqlAndroid(string dbName)
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
