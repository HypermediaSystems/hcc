#Introduction
Each entry in the cache is identified by an `url`.
The cache can be filled with in 2 different ways:


1. `GetCachedString()` or `GetCachedData()`
2. `AddCachedString()` or `AddCachedData()`

The `url` passed to the `GetCachedxxx` functions must always be absolute urls, starting with `http://` or `https://`.

You may remove data from the cache with `DeleteCachedData()`, even for a relative url. 
But you may not reload the data for relative urls with a `GetCachedxxx` function.

##Release mode
In Release mode the linker may incorrectly drop required classes.
(s.  [https://forums.xamarin.com/discussion/57462/dependencyservice-get-returns-null-only-1-platform-installed](https://forums.xamarin.com/discussion/57462/dependencyservice-get-returns-null-only-1-platform-installed) )
To avoid this add do the following:

###iOS
Add

	Xamarin.Forms.DependencyService.Register<HMS.Net.Http.iOS.SQLImplementation.SqliOS>();
to AppDelegate.cs in the function
 
	public override bool FinishedLaunching(UIApplication app, NSDictionary options)

###UWP
Add
	
	Xamarin.Forms.DependencyService.Register<HMS.Net.Http.UWP.SQLImplementation.SqlUWP>();
to MainPage.xaml.cs  in the constructor 

	public MainPage()

###Android
Add

	Xamarin.Forms.DependencyService.Register<HMS.Net.Http.Android.SQLImplementation.SqlAndroid>();

to MainActivity.cs in the function

	protected override void OnCreate(Bundle bundle)


##Install Autofac
In order to install Autofac in a PCL, you have to to install System.Runtime.InteropServices.RuntimeInformation Version 4.3.0 at first. (s. https://github.com/dotnet/corefx/issues/10445#issuecomment-264929319  )

##Profiles
https://portablelibraryprofiles.stephencleary.com/