# AzCopy UI and DataMovement samples for large file upload

There are two projects:
- bb-copy, a UI wrapper for AzCopy
- bb-azdm, a barebones winforms app for the [Azure Data Movement](https://github.com/Azure/azure-storage-net-data-movement) library

## [bb-copy](https://github.com/jpda/bb-azcopy/tree/master/bb-azcopy) - UI Wrapper for [azcopy](https://docs.microsoft.com/en-us/azure/storage/storage-use-azcopy)
The most important target to hit for the UI was to use .net 4.0. With some other code that ships, .net 4.0 is guaranteed to be on the box but nothing beyond. The latest azcopy I could find that only required .net 4.0 is 2.2.2, which is unfortunately being retired 4/7/17. 

The winforms app is .net 4.0 Client, the minimum installable flavor of .net 4.0 at the time. It will check for .net 4.5 *using the documented but oh-so-convoluted registry-checking process outlined here: https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx)* and switch to AzCopy 5.3.0, the latest as of now.

### Additions
#### SAS blob URIs
The app expects a container SAS URI with a minimum of write. This is mostly due to customer requirements. There is a versioning requirement when working with AzCopy 2.2.2 in particular - the `api-version` or `sv` parameters that dictate the targeted service version don't appear to work with 2.2.2 except for specific versions. Tested versions for the `sv` parameter are currently `2013-08-05`.
#### Short links
The app will attempt to follow and parse a bit.ly shortlink for a SAS, lowering the customer data-entry requirements slightly
### Issues
#### Status reporting and StandardOutput
It appears AzCopy doesn't call flush() during the transfer - it just backspaces or does other things, but `BeginOpenReadLine()` doesn't get data until the transfer ends (presumably because no lines are flushed until the transfer is complete). Currently the app is set to show the azcopy window when it opens to report status to the user since `StandardOutput` redirects successfully but is largely unusable. There are some attempts to read the streams in different fashions in the code.
## To-do
Error handling isn't terribly robust. I haven't written apps in winforms in a long time so some of my patterns and practices may be bit rusty.

## [bb-azdm](https://github.com/jpda/bb-azcopy/tree/master/bb-azdm) - barebones winforms using Azure Data Movement libraries
AzCopy is underpinned by the Azure Data Movement libraries, so it makes sense to try it here. This simple app uses the libraries to upload data to blob storage, using the SAS to get a container reference, create a file and report progress back. Requires .net 4.5.
