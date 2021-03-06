﻿using PCLStorage;
using SQLite;
using System;
using System.IO;

// [assembly: Xamarin.Forms.Dependency(typeof(SqlUWP))]
namespace HMS.Net.Http.NET.SQLImplementation
{
    [Foundation.Preserve(AllMembers = true)]
    public class SqlNET : ISql

    {
        private readonly string SqlConnectionString;
        private readonly string SqlDBName;

        // [Preserve]
        public SqlNET()
        {
            string dbName = HttpCachedClient._dbName;

            IFolder folder = FileSystem.Current.LocalStorage;

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