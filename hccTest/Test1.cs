using HMS.Net.Http;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace hccTest
{
    [TestFixture]
    public static class Test1
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
        [Test]
        public static async Task TestAlias()
        {
            using (HttpCachedClient hcClient = new HttpCachedClient(TestConstant.sqLiteCache))
            {
                string strTestAlias = "TestAlias";
                string strTestUrl = "TestUrl";

                await hcClient.AddCachedAliasUrlAsync(strTestAlias, strTestUrl);
                string url = await hcClient.GetCachedAliasUrlAsync(strTestAlias);
                Assert.AreEqual(url, strTestUrl, "find created entry");

                await hcClient.DeleteCachedAliasAsync(strTestAlias);

                url = await hcClient.GetCachedAliasUrlAsync(strTestAlias);
                Assert.AreEqual(url, strTestAlias, "do not find deleted entry");
            }
        }
    }
}
