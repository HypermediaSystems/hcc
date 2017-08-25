using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace hcc.Droid
{
    [Activity(Label = "hcc", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // we use this hack to get the linker not removing the assembly
            // (s. https://forums.xamarin.com/discussion/57462/dependencyservice-get-returns-null-only-1-platform-installed)
            Xamarin.Forms.DependencyService.Register<HMS.Net.Http.Android.SQLImplementation.SqlAndroid>();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
    }
}

