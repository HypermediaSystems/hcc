# Introduction

With **HttpCachedClient** it is possible to access internet resources that are seamlessly cached in an SQLite database. So they are available the next time, even if there is no internet connection.

# Installation

The HttpCachedClient can be installed with NuGet:

    nuget install HttpCachedClient

# Get started

The **HttpCachedClient** requires an object with an implementation of the interface iSQL.
The NuGet package includes these classes for iOS, Android and UWP.
E.g.:

     `iSQL SQL = new HMS.Net.Http.UWP.SQLImplementation.SqlUwp()`

An **HttpCachedClient** object can be instantiated in a portable class library as follows:

       this.SQL = SQL; 
       this.sqLiteCache = new SqLiteCache(SQL, "");
       this.hcClient = new HttpCachedClient(this.sqLiteCache);

Normally you will create the object once in an application rather than using an using statement. It is is possible to have several `HttpCachedClient` objects at the same time. But all requests will be stores in the same database.

A string is requested as follows:

           HccResponse hccResponse = await hc.GetCachedStringAsync(url);

`hccResponse.json` will contain the returned data:, which in most cases will be JSON, hence the name.
There is also a `GetCachedStreamAsync`, which will return a stream in `hccResponse.stream` 

The `hccResponse.hccInfo` object (`class HccInfo`) contains additional information, e.g. if the request was fulfilled from the database.

If the request could not fulfilled from the database and if there is an internet connection, the request is emitted with `GetAsync `and the response is stored in the database before being returned to the application.

# WiKi

## Add authentication

If the internet resource requires authentication data it is possible to pass a `AuthenticationHeaderValue`  object.
Additionally there is a `beforeGetAsyncHandler` callback that can be set:
    public delegate int beforeGetAsyncHandler(string urlRequested, HttpCachedClient httpCachedClient);

HttpCachedClient is subclass of HttpClient, so you can set additional headers here: 

    hc.beforeGetAsyncFunction = (urlRequested, httpCachedClient) => {
        httpCachedClient.DefaultRequestHeaders.Add(
             "X-Clacks-Overhead", "GNU Terry Pratchett");
        return 0;
    };

## Get header data

With the property `string[] includeHeaders` can be defined which headers should be stored and returned: 

- null, add no headers,
- empty, add all headers
- else, add listed headers
        
If the request is fulfilled from the internet, the set of headers is filtered before it is stored in the database. So requests fulfilled from the database have access only to this filtered set.



## Add data directly without an HttpRequest

With the two functions:

-`void AddCachedString(string id, string data)`
-`void AddCachedStream(string id, byte[] data)`

you can add data directly to the database. id should be the url under which the data can be requested.

`AddCachedString` and `AddCachedStream` have the following optional arguments:

- string headers = "", 
- Boolean overwrite = true, 
- byte zipped = 1, 
- byte encrypted = 0

Note that `AddCachedString` calls `AddCachedStream` after creating a byte array from the string. with `Encoding.UTF8.GetBytes(data)`.
 
## Delete data from the cache

You can delete an entry from the database with `void DeleteCachedData(string id)` and all entries with `void DeleteAllCachedData()`.

Note that some entries in the database may be fixed, so they will not be deleted.

## Make undeleteable entries


## Backup and restore

     string serverUrl;
     await hcClient.BackupAsync(serverUrl);

This will send the SqLite database as a byte array in a multipart message with `PostAsync` to the given serverUrl.

     await hcClient.RestoreAsync(serverUrl);

This will fetch the bay data from the given server and replace the local SqLite database.

## Encryption

You can set the parameter `encrypted` in the call to `AddCached...()` and `GetCached...()` functions to `1`. In this case an encryption callback will be called before the data is stored in the database. If a request is fullfilled from the database and the entry is encrypted, the decryption callback is called.

## Read-only database

When the property `hccClient.isReadonly` is set to true, requested data will not be stored in the database.

## Offline

When the property `hccClient.isOffline` is set to true, requests that can not be fulfilled from the database will return null as string or stream.


## Finally


HttpCachedClient is a subclass of System.Web.HttpClient. The additional functions and properties contain Cached, e.g. GetCachedString().


Each entry in the cache is identified by an `url`.
The cache can be filled with in 2 different ways:

1. `GetCachedString()` or `GetCachedData()`
2. `AddCachedString()` or `AddCachedData()`

The `url` passed to the `GetCached...` functions must always be absolute urls, starting with `http://` or `https://`.

You may remove data from the cache with `DeleteCachedData()`, even for a relative url. 
But you may not reload the data for relative urls with a `GetCached...` function.

