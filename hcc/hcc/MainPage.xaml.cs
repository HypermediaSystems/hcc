using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net.Http;
using HMS.Net.Http;

namespace hcc
{
    public partial class MainPage : ContentPage
    {
        private iSQL SQL;
        private SqLiteCache sqLiteCache = null;

        public MainPage()
        {
            InitializeComponent();
            this.SQL = Xamarin.Forms.DependencyService.Get<iSQL>();
            this.sqLiteCache = new SqLiteCache( SQL, "");

        }

        private async void tbGet_Clicked(object sender, EventArgs e)
        {
            string url = tbUrl.Text.Trim();
            tbInfo.Text = "";

            tbContent.Text = "Loading " + url + "...";
            HttpCachedClient hc = new HttpCachedClient(this.sqLiteCache);
            try
            {
                string user = tbUser.Text.Trim();
                string pwd = tbPWD.Text.Trim();

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(user))
                {
                    hc.authenticationHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.UTF8Encoding.UTF8.GetBytes(
                            string.Format("{0}:{1}", user, pwd))));
                }
                else
                {
                    hc.authenticationHeaderValue = null;
                }
                if (cbEncrypted.IsToggled == true)
                {
                    hc.encryptFunction = (urlRequested, data) =>
                    {
                        Array.Reverse(data);
                        return data;
                    };
                    hc.decryptFunction = (urlRequested, data) =>
                    {
                        Array.Reverse(data);
                        return data;
                    };
                }
                else
                {
                    hc.encryptFunction = null;
                    hc.decryptFunction = null;
                }

                // if we have to set additional headers we can that by setting beforeGetAsyncFunction
                hc.beforeGetAsyncFunction = (urlRequested, httpCachedClient) => {
                    httpCachedClient.DefaultRequestHeaders.Add("X-Clacks-Overhead", "GNU Terry Pratchett");
                    return 0;
                };

                if (cbZipped.IsToggled == true)
                { 
                    hc.zipped = 1;                
                }
                else
                {
                    hc.zipped = 0;
                }

                if (cbAddHeaders.IsToggled == true)
                {
                    hc.addHeaders= true;
                    hc.includeHeaders = new string[] { };                    
                }
                else
                {
                    hc.addHeaders = false;
                    hc.includeHeaders = null;
                }
                await hc.GetCachedString(url, (json,hi) =>
                {
                    tbInfo.Text =  "responseStatus: " + hi.responseStatus.ToString() + Environment.NewLine;
                    tbInfo.Text += "fromDB:   " + hi.fromDb.ToString() + Environment.NewLine;
                    tbInfo.Text += "zipped:   " + hi.zipped.ToString() + Environment.NewLine;
                    tbInfo.Text += "encrypted:" + hi.encrypted.ToString() + Environment.NewLine;
                    tbInfo.Text += "size:     " + hi.size.ToString() + Environment.NewLine;

                    tbInfo.Text += Environment.NewLine;
                    if (hi.hhh != null)
                    {
                        tbInfo.Text += "Header-Info:" + Environment.NewLine;
                        foreach (var h in hi.hhh.items)
                        {
                            tbInfo.Text += "    " + h.Key + ": " + h.Value[0] + Environment.NewLine;

                        }
                    }
                    tbInfo.Text += "Cache-Info:" + Environment.NewLine;
                    tbInfo.Text += "    Size: " + hc.GetCachedSize().ToString() +  Environment.NewLine;
                    tbInfo.Text += "    Count:" + hc.GetCachedCount().ToString() + Environment.NewLine;
                    tbContent.Text = json;
                });
            }
            catch (Exception ex)
            {
                tbContent.Text = ex.Message + Environment.NewLine;
                if(ex.InnerException != null )
                    tbContent.Text += ex.InnerException.Message;
            }

        }


        private void tbDelete_Clicked(object sender, EventArgs e)
        {
            string url = tbUrl.Text.Trim();
            tbInfo.Text = "";

            tbContent.Text = url +  " deleted from cache.";
            HttpCachedClient hc = new HttpCachedClient(this.sqLiteCache);
            hc.DeleteCachedData(url);
            
        }

        private void tbList_Clicked(object sender, EventArgs e)
        {
            HttpCachedClient hc = new HttpCachedClient(this.sqLiteCache);
            string[] ids = hc.GetCachedUrls("");

            tbInfo.Text = string.Join(Environment.NewLine, ids);

        }
    }
}
