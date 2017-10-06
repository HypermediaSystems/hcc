## Release mode

In Release mode the linker may incorrectly drop required classes.
(s.  [https://forums.xamarin.com/discussion/57462/dependencyservice-get-returns-null-only-1-platform-installed](https://forums.xamarin.com/discussion/57462/dependencyservice-get-returns-null-only-1-platform-installed) )
To avoid this add do the following:

### iOS

Add

	Xamarin.Forms.DependencyService.Register<HMS.Net.Http.iOS.SQLImplementation.SqliOS>();
to AppDelegate.cs in the function
 
	public override bool FinishedLaunching(UIApplication app, NSDictionary options)

### UWP

Add
	
	Xamarin.Forms.DependencyService.Register<HMS.Net.Http.UWP.SQLImplementation.SqlUWP>();
to MainPage.xaml.cs  in the constructor 

	public MainPage()

### Android

Add

	Xamarin.Forms.DependencyService.Register<HMS.Net.Http.Android.SQLImplementation.SqlAndroid>();

to MainActivity.cs in the function

	protected override void OnCreate(Bundle bundle)


## Profiles

https://portablelibraryprofiles.stephencleary.com/