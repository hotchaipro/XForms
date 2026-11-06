# Building and deploying

Each platform's project is an ordinary .NET project, and uses the usual tools.

## Windows

```bash
dotnet publish MyApp.Win32 -c Release -r win-x64 --self-contained -p:PublishTrimmed=true
```

The result needs nothing installed, and is about 21 MB for the test app. A `WebView` needs the
WebView2 runtime, which comes with Windows 11.

## Android

```bash
dotnet publish MyApp.Android -c Release
```

Sign it as for any .NET Android app, with `AndroidKeyStore` and its related properties. Android 7.0
(API 24) is the minimum. Add the permissions you use, such as `INTERNET`, to
`Properties/AndroidManifest.xml`.

## iOS

Build on a Mac, with Xcode:

```bash
dotnet publish MyApp.iOS -c Release -r ios-arm64
```

Set `CodesignKey` and `CodesignProvision` for your signing identity and profile. iOS 15 is the
minimum. A background refresh needs `BGTaskSchedulerPermittedIdentifiers` and `UIBackgroundModes` in
`Info.plist`.

## Web

```bash
dotnet publish MyApp.Web -c Release
```

Serve `wwwroot` from the publish folder from any static host. The test app is about 4.8 MB compressed. Serve
the `.br` files with `Content-Encoding: br` to get that size, and let paths fall back to
`index.html` if the app uses them in the address.

## Versions

See [what's supported](../support.md) for the versions of .NET and each platform.
