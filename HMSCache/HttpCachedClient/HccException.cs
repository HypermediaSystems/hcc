using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public class HccException : Exception
    {
        public HccException() : base()
        {
        }

        public HccException(string message) : base(message)
        {
        }

        public HccException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
