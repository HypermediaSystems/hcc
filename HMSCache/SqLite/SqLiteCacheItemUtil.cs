using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Net.Http
{
    public partial class SqLiteCache : IDataProvider
    {
        #region Util
        private string clearUrl(string url)
        {
            if (url.StartsWith(this.server, StringComparison.CurrentCulture))
            {
                return url.Substring(this.server.Length);
            }
            return url;
        }
        public HccHttpHeaders GetHeadersFromString(string headerString)
        {
            HccHttpHeaders httpHeaders = new HccHttpHeaders();
            string[] lines = headerString.Split(new string[] { Environment.NewLine, @"\r", @"\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines)
            {
                int pos = l.IndexOf(": ", StringComparison.CurrentCulture);
                string key = l.Substring(0, pos);
                string value = l.Substring(pos + 2);
                string[] values = value.Split(';');
                httpHeaders.items.Add(key, values);
            }

            return httpHeaders;
        }
        

        #endregion
    }
}
