using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hccTest
{
    /// <summary>
    /// these constants have to be set in the platform dependent startup
    /// </summary>
    public class TestConstant
    {
        public static ISql SQL;
        public static SqLiteCache sqLiteCache = null;
    }
}
