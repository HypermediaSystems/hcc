using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace hcc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HccManagerPage : TabbedPage
    {
        public HccManagerPage()
        {
            InitializeComponent();

            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            SqLiteCacheItem sqLiteCacheItem = null;
            Task.Run(async () =>
            {
                sqLiteCacheItem = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntryAsync(((SqLiteCacheItem)e.Item).url);
            }).Wait();
            detailGrid.BindingContext = sqLiteCacheItem;
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SqLiteCacheItem sqLiteCacheItem = null;
            Task.Run(async () =>
            {
                sqLiteCacheItem = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntryAsync(((SqLiteCacheItem)e.SelectedItem).url);
            }).Wait();
            detailGrid.BindingContext = sqLiteCacheItem;
        }

        private void btnRefresh_Clicked(object sender, EventArgs e)
        {
            refreshList();
        }

        private void refreshList()
        {
            listView.ItemTapped -= ListView_ItemTapped;
            listView.ItemSelected -= listView_ItemSelected;
            BindingObj bo = new BindingObj();
            listView.BindingContext = bo;

            IEnumerable<SqLiteCacheItem> list = null;
            Task.Run( async () =>
            {
                list = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntriesAsync(tbUrl.Text);
            }).Wait();
            listView.ItemsSource = list;

            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            listView.SelectedItem = ((IEnumerable<SqLiteCacheItem>)listView.ItemsSource).FirstOrDefault();
        }

        private void btnSelect_Clicked(object sender, EventArgs e)
        {
            SqLiteCacheItem sqLiteCacheItem = null;
            Task.Run(async () =>
            {
                sqLiteCacheItem = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntryAsync(((Button)sender).Text);
            }).Wait();
            detailGrid.BindingContext = sqLiteCacheItem;
        }

        private void btnEntryDelete_Clicked(object sender, EventArgs e)
        {
            string url = HccTag.GetTag((Button)sender);
            Task.Run(async () =>
            {
                await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DeleteCachedDataAsync(url);
            }).Wait();
            refreshList();
        }

        private void btnBackup_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string serverUrl = tbServer.Text.Trim();

            serverUrl = hcc.HccUtil.url_join(serverUrl, "upload");

            server_status_set( "Backuping to " + serverUrl + " ...");
            try
            {
                Task.Run(async () =>
                {
                    await hcClient.BackupAsync(serverUrl);
                }).Wait();
                server_status_set("Backuping to " + serverUrl + " done.");
            }
            catch (Exception ex)
            {
                server_status_set("Error Backuping to " + serverUrl + " :" + ex.ToString());
            }
        }

        private void btnRestore_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string serverUrl = tbServer.Text.Trim();

            serverUrl = hcc.HccUtil.url_join(serverUrl, "download?url=" + HttpCachedClient._dbName + ".sqlite");

            server_status_set("Restoring from " + serverUrl + " ...");

            try
            {
                Task.Run(async () =>
                {
                    await hcClient.RestoreAsync(serverUrl);
                }).Wait();
                server_status_set("Restoring from " + serverUrl + " done.");
            }
            catch (Exception ex)
            {
                server_status_set("Error Restoring from " + serverUrl + " :" + ex.ToString());
            }
        }
        private void btnReset_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);
            try
            {
                Task.Run(async () =>
                {
                    await hcClient.ResetAsync().ContinueWith(async t1 =>
                    {
                        await hcClient.GetCachedCountAsync().ContinueWith(t =>
                        {
                            long cnt = t.Result;
                            server_status_set("Reseting done: " + cnt);
                        });
                    });
            }).Wait();
            // server_status_set("Reseting done.");
        }
            catch (Exception ex)
            {
                server_status_set("Error Reseting:" + ex.ToString());
            }
        }


        private void btnDeleteAll_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);
            Task.Run(async () =>
            {
                await hcClient.DeleteAllCachedDataAsync();
            }).Wait();
        }

        private void btnLoop_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            const string debugUrl = "debugUrl";
            Task.Run(async () =>
            {
                await hcClient.AddCachedStringAsync(debugUrl, "DebugData");
            }).Wait();
            int i1 = 0;
            int i2 = 0;
            Task.Run(async () =>
            {
                for (i1 = 0; i1 < 100; i1++)
                {
                    HccResponse hccResponse = await hcClient.GetCachedStringAsync(debugUrl);
                    System.Diagnostics.Debug.WriteLine("tbLoop_Clicked1 " + i1.ToString() + "  " + i2.ToString());
                    Task.Delay(100).Wait();
                    Device.BeginInvokeOnMainThread(() => btnLoop.Text = "Loop " + i1.ToString() + "  " + i2.ToString());
                }
            });
            Task.Run(async () =>
            {
                for (i2 = 0; i2 < 200; i2++)
                {
                    HccResponse hccResponse =  await hcClient.GetCachedStringAsync(debugUrl);
                    //  , (hccResponse) => System.Diagnostics.Debug.WriteLine("tbLoop_Clicked2 " + i1.ToString() + "  " + i2.ToString())).ConfigureAwait(false)
                    ;
                    Task.Delay(50).Wait();
                    Device.BeginInvokeOnMainThread(() => btnLoop.Text = "Loop " + i1.ToString() + "  " + i2.ToString());
                }
            });
        }

        private async void btnImport_ClickedAsync(object sender, EventArgs e)
        {
            string server = tbImportServer.Text.Trim();
            string site = tbImportSite.Text.Trim();

            if (!server.EndsWith("/",StringComparison.CurrentCulture))
                server += "/";

            import_status_set("getting list from " + server + " ... ");
            // get the list
            HttpClient httpClient = new HttpClient();

            HccConfig.Rootobject hccConfig;
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(server + "config?site=" + site, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    hccConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<HccConfig.Rootobject>(json);
                }
            }
            catch (Exception ex)
            {
                import_status_set("ERROR: " + ex.ToString());

                return;
            }
            import_status_set("got list from " + server + " with " + hccConfig.files.Length.ToString() + " entries");

            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            await hcClient.AddCachedMetadataAsync("url", hccConfig.url);

            for (int i = 0; i < hccConfig.files.Length; i++)
            {
                import_status_set("get entry " + (i + 1).ToString() + " - " + hccConfig.files.Length.ToString());
                using (HttpResponseMessage response = await httpClient.GetAsync(server + "entry?site=" + site +"&url=" + hccConfig.files[i].url, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string headerString = hcClient.GetCachedHeader(response.Headers);

                    Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    Stream strm = new MemoryStream();
                    streamToReadFrom.CopyTo(strm);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        byte[] data = ((MemoryStream)strm).ToArray();
                        // we have to remove the BOM, since we want to store only text
                        int bomEnd = Bom.GetCursor(data);
                        if( bomEnd > 0 )
                        {
                            Byte[] datax = new Byte[data.Length - bomEnd];
                            Array.Copy(data, bomEnd, datax, 0, data.Length - bomEnd);
                            data = datax;
                        }

                        string[] externalURLs = { "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" };
                        if(hccConfig.files[i].replace  )
                        {
                            string html = Encoding.UTF8.GetString(data,0,data.Length);
                            foreach(var pat in externalURLs)
                            {
                                html = html.Replace(pat, "external/" + pat.Substring(7));
                            }
                            data = Encoding.UTF8.GetBytes(html);
                        }

                        if (hcClient.encryptFunction != null)
                        {
                            data = hcClient.encryptFunction(hccConfig.files[i].url, data);

                            await hcClient.AddCachedStreamAsync(HccUtil.url_join(hccConfig.url,hccConfig.files[i].url),
                                data,
                                headers: headerString,
                                zipped: hcClient.zipped,
                                encrypted: 1);
                        }
                        else
                        {
                            await hcClient.AddCachedStreamAsync(HccUtil.url_join(hccConfig.url, hccConfig.files[i].url),
                                data,
                                headers: headerString,
                                zipped: 0);
                        }
                    }
                }
            }

            import_status_set("got list of external from " + server + " with " + hccConfig.externalUrl.Length.ToString() + " entries");
            for (int i = 0; i < hccConfig.externalUrl.Length; i++)
            {
                import_status_set("get entry " + (i + 1).ToString() + " - " + hccConfig.externalUrl.Length.ToString());
                using (HttpResponseMessage response = await httpClient.GetAsync(hccConfig.externalUrl[i].url, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string headerString = hcClient.GetCachedHeader(response.Headers);

                    Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    Stream strm = new MemoryStream();
                    streamToReadFrom.CopyTo(strm);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        byte[] data = ((MemoryStream)strm).ToArray();
                        // we have to remove the BOM, since we want to store only text
                        int bomEnd = Bom.GetCursor(data);
                        if (bomEnd > 0)
                        {
                            Byte[] datax = new Byte[data.Length - bomEnd];
                            Array.Copy(data, bomEnd, datax, 0, data.Length - bomEnd);
                            data = datax;
                        }
                        Byte zipped = hcClient.zipped;
                        if( hccConfig.zipped != null)
                        {
                            zipped = Byte.Parse(hccConfig.zipped);
                        }
                        if (hccConfig.externalUrl[i].zipped != null)
                        {
                            zipped = Byte.Parse(hccConfig.externalUrl[i].zipped);
                        }

                        if (hcClient.encryptFunction != null)
                        {
                            data = hcClient.encryptFunction(hccConfig.externalUrl[i].url, data);

                            await hcClient.AddCachedStreamAsync(hccConfig.externalUrl[i].url, data, headers: headerString, zipped: zipped, encrypted: 1);
                        }
                        else
                        {
                            await hcClient.AddCachedStreamAsync(hccConfig.externalUrl[i].url, data, headers: headerString, zipped: zipped);
                        }
                    }
                }
            }
        }

        private void import_status_set(string status)
        {
            Device.BeginInvokeOnMainThread(() => {
                lblImportStatus.Text = status;
            });
        }
        private void server_status_set(string msg)
        {
            Device.BeginInvokeOnMainThread(() => {
                lblServerStatus.Text = msg;
            });
       }

    }

    internal class HccManagerNodeEntry
    {
        public string fname { get; set; }
        public string url { get; set; }
        public string aliasUrl { get; set; }
        public Boolean needReplace { get; set; }
        public Boolean canBeZipped { get; set; }
    }

    internal class BindingObj
    {
        public SqLiteCacheItem SelectedItem { get; set; }
    }

    public static class HccTag
    {
        public static readonly BindableProperty TagProperty = BindableProperty.Create("Tag", typeof(string), typeof(HccTag), null);

        public static string GetTag(BindableObject bindable)
        {
            return (string)bindable.GetValue(TagProperty);
        }

        public static void SetTag(BindableObject bindable, string value)
        {
            bindable.SetValue(TagProperty, value);
        }
    }

    public static class Bom
    {
        // got from https://stackoverflow.com/a/16315911
        public static int GetCursor(Byte[] bytes)
        {
            // UTF-32, big-endian
            if (IsMatch(bytes, new byte[] { 0x00, 0x00, 0xFE, 0xFF }))
                return 4;
            // UTF-32, little-endian
            if (IsMatch(bytes, new byte[] { 0xFF, 0xFE, 0x00, 0x00 }))
                return 4;
            // UTF-16, big-endian
            if (IsMatch(bytes, new byte[] { 0xFE, 0xFF }))
                return 2;
            // UTF-16, little-endian
            if (IsMatch(bytes, new byte[] { 0xFF, 0xFE }))
                return 2;
            // UTF-8
            if (IsMatch(bytes, new byte[] { 0xEF, 0xBB, 0xBF }))
                return 3;
            return 0;
        }

        private static bool IsMatch(Byte[] bytes, byte[] match)
        {
            var buffer = new byte[match.Length];
            Array.Copy(bytes,0,buffer, 0, buffer.Length);
            return !buffer.Where((t, i) => t != match[i]).Any();
        }
    }
}