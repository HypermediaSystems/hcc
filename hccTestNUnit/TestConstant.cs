﻿using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hccTestNUnit
{
    /// <summary>
    /// these constants have to be set in the platform dependent startup
    /// </summary>
    public class TestConstant
    {
        public static iSQL SQL;
        public static SqLiteCache sqLiteCache = null;
    }
}
