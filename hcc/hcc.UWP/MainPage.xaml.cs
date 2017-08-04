using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace hcc.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            // we use this hack to get the linker not removing the assembly
            // (s. https://forums.xamarin.com/discussion/57462/dependencyservice-get-returns-null-only-1-platform-installed)
            Xamarin.Forms.DependencyService.Register<HMS.Net.Http.UWP.SQLImplementation.SqlUWP>();
            this.InitializeComponent();

            LoadApplication(new hcc.App());
        }
    }
}
