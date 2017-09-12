using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace HMS.Net.Http
{
    public class SqLiteAlias
    {
        [PrimaryKey]
        public string aliasUrl { get; set; }

        public string url { get; set; }
    }
}
