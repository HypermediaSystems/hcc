using HMS.Net.Http;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace hccTest
{
    [TestFixture]
    public class Test1
    {
        [Test, Order(1)]
        public static void TestStartup()
        {
            Assert.IsNotNull(TestConstant.sqLiteCache);
        }
        [Test]
        public static void TestDBname()
        {
            using (HttpCachedClient hcClient = new HttpCachedClient(TestConstant.sqLiteCache))
            {
                Assert.IsNotNull(hcClient.DBName);
            }
        }
    }
}
