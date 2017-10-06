# Database



## SqLiteCacheItem

This table contains the entries in the database.

| name | type | descriptiom|
| ---  | --- | --- |
| url | varchar | the url or id of the entry
| data | blob | the data stored as a byte array
| header | blob | the header data 
| zipped | integer|1: data and header is stored unzipped, 0 zipped
| encrypted | integer | 1: data and header are stored encryptedun
| lastWrite | bigint | datetime of last write access
| lastRead | bigint | datetime of last read access
| expire | bigint | dastetie of expire date
| size | integer | number of bytes
| dontRemove | integer| 1 never remove this entry



## SqLiteAlias

This table contains a mapping of urls. The data for aliasUrl is stored in the entry for url. This table can be used for mapping applications whre the same data can be stored on different servers. It will reduce the size of the database. 

| name | type | descriptiom|
| ---  | --- | --- |
| aliasUrl | varchar | 
| url | varchar | 




## SqLiteMetadata

This table holds some metadata. You can use this table to store custom data. Custom tags must not start with `hcc.`, these are used internally.


| name | type | descriptiom|
| ---  | --- | --- |
| tag | varchar | 
| value | varchar | 



