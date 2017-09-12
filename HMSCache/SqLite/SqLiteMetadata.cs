using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace HMS.Net.Http
{
    public class SqLiteMetadata
    {
        [PrimaryKey]
        public string tag { get; set; }

        public string value { get; set; }
    }
}
