# Pre-compressed static files provider

[![Build status](https://github.com/arian2ashk/CompressedStaticFiles/workflows/CI/badge.svg)](https://github.com/arian2ashk/CompressedStaticFiles/actions)
[![NuGet version](https://badge.fury.io/nu/AspNetCore.PreCompressedStaticFiles.svg)](https://badge.fury.io/nu/AspNetCore.PreCompressedStaticFiles)
[![GitHub license](https://img.shields.io/badge/license-Apache%202-blue.svg)](https://raw.githubusercontent.com/arian2ashk/CompressedStaticFiles/master/LICENSE)

This is a middleware for Asp</span>.Net Core 3.1 that provides a static files provider capable of providing pre-compressed static files.

This solution is based on [this repository](https://github.com/AnderssonPeter/CompressedStaticFiles).

## Installation

You can install the latest stable version via [NuGet](https://www.nuget.org/packages/AspNetCore.PreCompressedStaticFiles).

```
> dotnet add package AspNetCore.PreCompressedStaticFiles
```

## Documentation

Ensure that you are using the `Kestrel` server without the IIS Integration or that the "Dynamic Content Compression" is disabled in IIS.

### Usage

Place `app.UsePreCompressedStaticFiles();` instead of `app.UseStaticFiles();` in `Startup.Configure()`. Also if using `StaticFileOptions` remember to replace it with `PreCompressedStaticFileOptions`. `PreCompressedStaticFileOptions` inherits from `StaticFileOptions` and supports all the options that come with `StaticFileOptions`.

This will ensure that your application will serve pre-compressed `gzip` `(filename.ext.gz)` or `brotli` `(filename.ext.br)` compressed files if the browser supports it. Without providing any options the default behavior is that, if the browser supports both `gzip` and `brotli` and if pre-compressed files for both types exist it will provide the `gzipped` files only.

Checkout the `AspNetCore.PreCompressedStaticFiles.Example` project for usage.

### How to change preferred compression types

to change the order of the preferred compression types or to restrict it to only one type, create a `PreCompressedStaticFileOptions` object and then add the available types in the preferred order like below:

```csharp
var options = new PreCompressedStaticFileOptions();
options.CompressionTypes.Add<Brotli>();
options.CompressionTypes.Add<Gzip>();
app.UsePreCompressedStaticFiles(options);
```

In the example above if the browser supports `brotli` and the pre-compressed `brotli` files exist it will provide `brotli` files otherwise it will provide `gzipped` files.

### How to add more compression types

By default the middleware only supports `gzip` and `brotli`, to add more types you need to implement `ICompressionType` interface and then add it to the `CompressionTypes` list in `PreCompressedStaticFileOptions`.

> Note: the compression type needs to be supported in the commonly used browsers otherwise the users will not benefit from it.

here is an example of how to create a custom compression type:

```csharp
public class ExampleCompressionType: ICompressionType
{
    public string Encoding { get; } = "example";
    public string Extension { get; } = ".exm";
    public string ContentType { get; } = "application/example";
}
```

And here is how to set it up:

```csharp
var options = new PreCompressedStaticFileOptions();
options.CompressionTypes.Add<ExampleCompressionType>();
app.UsePreCompressedStaticFiles(options);
```
