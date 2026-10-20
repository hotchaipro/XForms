# XForms
XForms is a cross-platform UI framework optimized for building custom user interfaces.

## What's in XForms?
XForms is a UI framework that provides a very thin cross-platform wrapper around the essential components of a user interface:

* Windows (Forms)
* Navigation
* Layout
* Input
* Custom-drawn controls

XForms abstracts these basic components just enough to provide a consistent look for your application across platforms.

Think of XForms as providing the containers for your custom-drawn controls.

## Versus Xamarin.Forms
Xamarin.Forms is focused on apps that want to provide a "native" look on each platform. Your Xamarin.Forms app will look and work differently on each platform, and there are very few options to give your UI a custom look and feel.

In short, Xamarin.Forms is about churning out generic iOS apps that look like every other iOS app, and generic Android apps that look like every other Android app.

In contrast, XForms is focused on apps that want to provide a custom user interface that has a consistent look across platforms.

## Why not Native?
If you've been shipping apps for more than a few years, you'll know that the "native" look on every platform periodically changes, and apps that once looked sleek and modern now look obviously dated--not necessarily bad, just dated because they're using old "native" controls.

I noticed that some old iOS apps in particular still looked great, and the thing they all had in common was a completely custom user interface that still looked and worked great for their scenario.

That's when the light clicked on for me.

Despite how much each platform owner (Apple, Google, Windows) push you to support their latest and greatest native look (so your app looks like every other app and you'll probably have to rewrite it for every platform as well), it may not be your best option.

If you agree that makes sense for your app, then XForms may be just what you need.

## Is XForms for Me?
If you're writing an enterprise application where familiarity is the top priority (i.e., "boring" is a good thing), you really should look into Xamarin.Forms.

If your objective is to create a modern, exciting and unique user interface without having to re-write it for each platform, XForms may be for you.

## Architecture

XForms is a framework that contains all the platform-specific code, plus a
toolkit of controls built on the framework's public API. Other control
libraries can be built the same way and run on every platform. See
[docs/architecture.md](docs/architecture.md).

For how XForms compares with Uno Platform, Avalonia, .NET MAUI, Electron, React
Native, Flutter and Compose Multiplatform, see
[docs/comparisons.md](docs/comparisons.md).

## The current state of affairs:

* Windows (Win32, DirectComposition and DirectWrite, no XAML): in development
* Android: in development
* iOS: in development (builds; not yet tested on a device)
