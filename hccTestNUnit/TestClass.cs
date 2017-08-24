using HMS.Net.Http;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hccTestNUnit
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestiSQL()
        {

            // TODO: Add your test code here
            iSQL SQL = null;

            var namespaceFound = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                  from type in assembly.GetTypes()
                                  where type.Namespace == "HMS.Net.Http.UWP.SQLImplementation"
                                  select type).Any();
            if(namespaceFound == true )
            {
                SQL = null;
            }

            Assert.IsNotNull(SQL, "SQL created with Xamarin.Forms.DependencyService.Get ");

            SqLiteCache sqLiteCache = new SqLiteCache(SQL, "");

            HttpCachedClient hcClient = new HttpCachedClient(TestConstant.sqLiteCache);
            Assert.IsNotNull(hcClient.DBName);

        }
    }
}
