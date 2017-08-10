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
            listView.ItemTapped -= ListView_ItemTapped;
            listView.ItemSelected -= listView_ItemSelected;
            bindingObj bo = new bindingObj();
            listView.BindingContext = bo;
            listView.ItemsSource = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntries(tbUrl.Text);
            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            listView.SelectedItem = ((IEnumerable<SqLiteCacheItem>)listView.ItemsSource).FirstOrDefault();


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
}