# DDown
DDown is a dotnet downloader API.

## Features

* Download by partitions
* Stop the download with `Pause` method.
* Paused download can be resume with `StartAsync` method.

## Example

```csharp
using DDown;

var downloader = new Downloader(link);
var status = await downloader.PrepareAsync(); // status contain information about file. (e.g. Length, IsRangeSupported, PartitionCount)

await downloader.StartAsync(); 

if (!downloader.Canceled)
{
    // this method will marge partitions
    await downloader.MergeAsync();
}

```


```csharp
```


