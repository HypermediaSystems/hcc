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
                await hc.GetCachedString(url, (json,hi) =>
                {
                    tbInfo.Text = "fromDB:" + hi.fromDb.ToString() + Environment.NewLine;
                    tbInfo.Text += "zipped:" + hi.zipped.ToString() + Environment.NewLine;
                    tbInfo.Text += "encrypted:" + hi.encrypted.ToString() + Environment.NewLine;

                    tbInfo.Text += Environment.NewLine;
                    foreach (var h in hi.hhh.items)
                    {
                        tbInfo.Text += h.Key + ":" + h.Value[0] + Environment.NewLine;

                    }

                    tbContent.Text = json;
                });
            }
            catch (Exception ex)
            {
                tbContent.Text = ex.Message + Environment.NewLine + ex.InnerException.Message;
            }

        }


        private async void tbReload_Clicked(object sender, EventArgs e)
        {
            string url = tbUrl.Text.Trim();
            tbInfo.Text = "";

            tbContent.Text = "Loading " + url + "...";
            HttpCachedClient hc = new HttpCachedClient(this.sqLiteCache);
            hc.DeleteCachedData(url);
            try
            {
                await hc.GetCachedString(url, (json, hi) =>
                {
                    tbInfo.Text = "" + hi.fromDb.ToString();
                    tbContent.Text = json;
                });

            }
            catch (Exception ex)
            {
                tbContent.Text = ex.Message + Environment.NewLine + ex.InnerException.Message;
            }
        }
    }
}
