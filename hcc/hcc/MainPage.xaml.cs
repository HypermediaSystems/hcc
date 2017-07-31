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
        private SqLiteCache sqLiteCache;

        public MainPage()
        {
            InitializeComponent();
            this.SQL = Xamarin.Forms.DependencyService.Get<iSQL>();
            this.sqLiteCache = new SqLiteCache(SQL, "");

        }

        private async void tbGet_Clicked(object sender, EventArgs e)
        {
            string url = tbUrl.Text.Trim();
            tbInfo.Text = "";

            tbContent.Text = "Loading " + url + "...";
            HttpCachedClient hc = new HttpCachedClient(this.sqLiteCache);
            try
            {
                await hc.GetString(url, (json) =>
                {
                    tbContent.Text = json;
                });

            }
            catch (Exception ex)
            {
                tbContent.Text = ex.Message + Environment.NewLine + ex.InnerException.Message;
            }

        }


        private void tbReload_Clicked(object sender, EventArgs e)
        {

        }
    }
}
