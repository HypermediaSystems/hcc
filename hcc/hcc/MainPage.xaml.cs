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
        readonly internal ISql SQL;
        readonly internal SqLiteCache sqLiteCache;

        public MainPage()
        {
            InitializeComponent();
            this.SQL = Xamarin.Forms.DependencyService.Get<ISql>();
            this.sqLiteCache = new SqLiteCache( SQL, "");
        }

        private async void tbGet_ClickedAsync(object sender, EventArgs e)
        {
            string url = tbUrl.Text.Trim();
            tbInfo.Text = "";

            tbContent.Text = "Loading " + url + "...";
            HttpCachedClient hc = new HttpCachedClient(this.sqLiteCache);
            try
            {
                string user = tbUser.Text.Trim();
                string pwd = tbPWD.Text.Trim();

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                {
                    hc.authenticationHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            Encoding.UTF8.GetBytes(
                                string.Format("{0}:{1}", user, pwd))));
                }
                else
                {
                    hc.authenticationHeaderValue = null;
                }
                if (cbEncrypted.IsToggled)
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

                if (cbZipped.IsToggled)
                {
                    hc.zipped = 1;
                }
                else
                {
                    hc.zipped = 0;
                }

                if (cbAddHeaders.IsToggled)
                {
                    hc.addHeaders = true;
                    hc.includeHeaders = new string[] { };
                }
                else
                {
                    hc.addHeaders = false;
                    hc.includeHeaders = null;
                }
                await hc.GetCachedStringAsync(url, (json, hi) =>
                {
                    StringBuilder bld = new StringBuilder();
                    bld.Append("responseStatus: ").Append(hi.responseStatus.ToString()).Append(Environment.NewLine);
                    bld.Append("fromDB:   ").Append(hi.fromDb.ToString()).Append(Environment.NewLine);
                    bld.Append("zipped:   ").Append(hi.zipped.ToString()).Append(Environment.NewLine);
                    bld.Append("encrypted:").Append(hi.encrypted.ToString()).Append(Environment.NewLine);
                    bld.Append("size:     ").Append(hi.size.ToString()).Append(Environment.NewLine);
                    bld.Append("url:      ").Append(hi.url).Append(Environment.NewLine);
                    if (!string.IsNullOrEmpty(hi.aliasUrl))
                        bld.Append("aliasUrl: ").Append(hi.aliasUrl).Append(Environment.NewLine);

                    bld.Append(Environment.NewLine);
                    if (hi.hhh != null)
                    {
                        bld.Append("Header-Info:").Append(Environment.NewLine);
                        foreach (var h in hi.hhh.items)
                        {
                            bld.Append("    ").Append(h.Key).Append(": ").Append(h.Value[0]).Append(Environment.NewLine);
                        }
                    }
                    bld.Append("Cache-Info:").Append(Environment.NewLine);
                    bld.Append("    Size: ").Append(hc.GetCachedSize().ToString()).Append(Environment.NewLine);
                    bld.Append("    Count:").Append(hc.GetCachedCount().ToString()).Append(Environment.NewLine);
                    tbInfo.Text = bld.ToString();
                    tbContent.Text = json;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => {
                    tbContent.Text = ex.Message + Environment.NewLine;
                    if (ex.InnerException != null)
                        tbContent.Text += ex.InnerException.Message;
                });
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

        private async void btnManager_ClickedAsync(object sender, EventArgs e)
        {
            HttpCachedClient hc = new HttpCachedClient(this.sqLiteCache);
            await Navigation.PushAsync(new HccManagerPage
            {
                BindingContext = hc
            }).ConfigureAwait(false);
        }
    }
}
