using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net.Http;

namespace hcc
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void tbGet_Clicked(object sender, EventArgs e)
        {
            string url = tbUrl.Text.Trim();
            tbResult.Text = "Loading " + url + "...";
            HttpClient hc = new HttpClient();
            try
            {
                string responseBodyAsText = await hc.GetStringAsync(url);
                tbResult.Text = responseBodyAsText;

            }
            catch (Exception ex)
            {
                tbResult.Text = ex.Message + Environment.NewLine + ex.InnerException.Message;
            }

        }


        private void tbReload_Clicked(object sender, EventArgs e)
        {

        }
    }
}
