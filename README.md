# Nager.DataFragmentationHandler

<img src="https://raw.githubusercontent.com/nager/Nager.DataFragmentationHandler/main/doc/icon.png" width="150" title="Nager DataFragmentationHandler" alt="Nager DataFragmentationHandler" align="left">

Process fragmented bytes via a buffer to dataPackages. The library is especially memory saving because only one buffer is defined and a [span](https://docs.microsoft.com/en-us/dotnet/api/system.span-1) is used to move in it. There are several analyzers that can be used to identify a package.
<br>
<br>
<br>
<br>
<br>

## How can I use it?

The package is available via [NuGet](https://www.nuget.org/packages/Nager.DataFragmentationHandler)
```
PM> install-package Nager.DataFragmentationHandler
```


## Examples of use

### For data packets with a fixed start and end token
In this example StartToken is `0x01` and EndToken is `0x02`

```cs
void NewDataPackage(DataPackage dataPackage)
{
    //dataPackage.Data <-- 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f
}

var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer);
dataPackageHandler.NewDataPackage += NewDataPackage;
dataPackageHandler.AddData(new byte[] { 0x01, 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x02 });
dataPackageHandler.NewDataPackage -= NewDataPackage;
```

### For data packets with a start token and lengh info on second byte
In this example StartToken is `0x01` and the Length is `0x06`

```cs
void NewDataPackage(DataPackage dataPackage)
{
    //dataPackage.Data <-- 0x10, 0x65, 0x6c, 0x6c
}

var dataPackageAnalyzer = new StartTokenWithLengthInfoDataPackageAnalyzer(0x01);

var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer);
dataPackageHandler.NewDataPackage += NewDataPackage;
dataPackageHandler.AddData(new byte[] { 0x01, 0x06, 0x10, 0x65, 0x6c, 0x6c });
dataPackageHandler.NewDataPackage -= NewDataPackage;
```
