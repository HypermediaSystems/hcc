#Introduction
With HHTCachecClient it is posssible to access internet resources that are seemlessly cached in an SQLite database. So they are available the next time, even if there is no internet connection.
#Installation
The HttpCachedClient can be installed with NuGet:
nuget install HttpCachedClient.

#Get started
The HtpCachedClient requires an object with an implementation of the interface iSQL.
The NuGet package includes these classes for iOS, Android and UWP.
E.g.: `iSQL SQL = new HMS.Net.Http.UWP.SQLImplementation.SqlUwp()`
An HttpCachecClient object can tha be instantiated in a portable class library as follows:
       this.SQL = SQL; //  Xamarin.Forms.DependencyService.Get<iSQL>();
       this.sqLiteCache = new SqLiteCache(SQL, "");
       this.hcClient = new HttpCachedClient(this.sqLiteCache);

Normally you will create the object once in an application rather than using an using statement. It is is possible tohave several `HttpCachedClient` object at the same time. But all requests will be stores in the same database.

A string is requested as follows:

          await hc.GetCachedStringAsync(url, (json, hi) =>
           {   }(ConfigureAwait(false);
`json` will contain the returned data, whichnin most cases will be JSON, hence the name.
There is also a GetCachedStreamAsync, which will return a stream.

The `hi` object (`class HccInfo`) contains additional information, especially if the request was fullfilled from the database.

If the request could not fullfilled from the database and if there is an internet connection, the request is emitted with `GetStringAsync `and the response is stored in the database before beeing returned to the application.

#WiKi
##Add authentication
If the internet reaource re	uires authtication data it is possible to pass a `AuthenticationHeaderValue`
 object.
Additional there is a `beforeGetAsyncHandler` callback that can be set:
    public delegate int beforeGetAsyncHandler(string urlRequested, HttpCachedClient httpCachedClient);

HttpCachedClient is subclass of HttpClinet, so you can set additiona heders here: 

    hc.beforeGetAsyncFunction = (urlRequested, httpCachedClient) => {
        httpCachedClient.DefaultRequestHeaders.Add("X-Clacks-Overhead", "GNU Terry Pratchett");
        return 0;
    };

##Get header data
With the property `string[] includeHeaders` can be defined which headers should be stored and returned: 

- null, add no headers,
- empty, add all headers
- else, add listed headers
        
If the request is fullfilled from the internet, the set of headers is filtered before it is stored in the database. So requests fullfilled from the database have access only to this filtered set.



##Add data directly without an HttpRequest
With the two functions:

-`void AddCachedString(string id, string data)`
-`void AddCachedStream(string id, byte[] data)`

you can add data directly to the databasse. id should be the Url under which the data can be requested.

`AddCachedString` and `AddCachedStream` have the following optional arguments:

- string headers = "", 
- Boolean overwrite = true, 
- byte zipped = 1, 
- byte encrypted = 0

Note that `AddCachedString` calls `AddCachedStream` after creating a byte array from the string. with `Encoding.UTF8.GetBytes(data)`.
 
##Delete data from the cache

You can delete an entry from the database with `void DeleteCachedData(string id)` and all entries with `void DeleteAllCachedData()`.

Note that some entries in the database may be fixed, so they will not be deleted.

##Make undeleteable entries


##Backup and restore



##Ecryption

You can set the parameter encrypted in the call to AddCached...() and GetChached...() functions to 1. In this case a encryption callback will be called before the data is stored in the database. If a request is fullfilled from the databasse and the entry is encrypted, the decyrption callback is called.
  





HTTPCachedClient is a subclass of System.Web.HttpClient. The additional functions and properties contain Cached, e.g. GetCachedString().



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