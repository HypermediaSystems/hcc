using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace hcc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class hccManagerPage : TabbedPage
    {
        public hccManagerPage()
        {
            InitializeComponent();

            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;
            
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            detailGrid.BindingContext = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntry(((SqLiteCacheItem)e.Item).url);
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            detailGrid.BindingContext = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntry(((SqLiteCacheItem)e.SelectedItem).url);
        }

        private void btnRefresh_Clicked(object sender, EventArgs e)
        {
            refreshList();
        }
        private void refreshList()
        {
            listView.ItemTapped -= ListView_ItemTapped;
            listView.ItemSelected -= listView_ItemSelected;
            bindingObj bo = new bindingObj();
            listView.BindingContext = bo;
            listView.ItemsSource = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntries(tbUrl.Text);
            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            listView.SelectedItem = ((IEnumerable<SqLiteCacheItem>)listView.ItemsSource).FirstOrDefault();

        }
        private void btnSelect_Clicked(object sender, EventArgs e)
        {            
            detailGrid.BindingContext = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntry(((Button)sender).Text);
        }
        private void btnEntryDelete_Clicked(object sender, EventArgs e)
        {
            string url = hccTag.GetTag((Button)sender);
            (BindingContext as HMS.Net.Http.HttpCachedClient).DeleteCachedData(url);
            refreshList();

        }
    }
    class bindingObj
    {
        private SqLiteCacheItem _SelectedItem;
        public SqLiteCacheItem SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                _SelectedItem = value;
            }
        }
    }
    public class hccTag
    {
        public static readonly BindableProperty TagProperty = BindableProperty.Create("Tag", typeof(string), typeof(hccTag), null);

        public static string GetTag(BindableObject bindable)
        {
            return (string)bindable.GetValue(TagProperty);
        }

        public static void SetTag(BindableObject bindable, string value)
        {
            bindable.SetValue(TagProperty, value);
        }
    }
}