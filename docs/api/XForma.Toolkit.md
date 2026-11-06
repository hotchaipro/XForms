# XForma.Toolkit

## Clipboard

The clipboard's text: `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.IClipboard` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## Connectivity

Whether the device is online: `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.IConnectivity` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## FilePicker

Opening and saving files the user picks: `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.IFilePicker` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## FilePickerOptions

What a file picker offers.

| Member | |
|---|---|
| `AllowMultiple.get -> bool` | Whether more than one file can be picked. |
| `AllowMultiple.init -> void` | Whether more than one file can be picked. |
| `FilePickerOptions() -> void` |  |
| `FileTypes.get -> System.Collections.Generic.IReadOnlyList<string>` | The file name extensions to offer (such as ".txt"), or null for any file. |
| `FileTypes.init -> void` | The file name extensions to offer (such as ".txt"), or null for any file. |
| `Title.get -> string` |  |
| `Title.init -> void` |  |

## HapticFeedback

Haptic feedback, where the device has it: `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.IHapticFeedback` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## HapticFeedbackType

| Member | |
|---|---|
| `Click = 0 -> XForma.Toolkit.HapticFeedbackType` | A light tap, as for a key or a toggle. |
| `LongPress = 1 -> XForma.Toolkit.HapticFeedbackType` | A stronger one, as for a long press. |

## IClipboard

The system clipboard's text.

| Member | |
|---|---|
| `GetTextAsync() -> System.Threading.Tasks.Task<string>` |  |
| `HasText.get -> bool` |  |
| `SetTextAsync(string text) -> System.Threading.Tasks.Task` |  |

## IConnectivity

Whether the device is online. `ConnectivityChanged` is raised on the UI thread.

| Member | |
|---|---|
| `ConnectivityChanged -> System.EventHandler` |  |
| `NetworkAccess.get -> XForma.Toolkit.NetworkAccess` |  |

## IFilePicker

The system's dialogs for opening and saving files.

| Member | |
|---|---|
| `PickAsync(XForma.Toolkit.FilePickerOptions options = null) -> System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<XForma.Toolkit.PickedFile>>` | Lets the user pick files to open. Returns none when they cancel. |
| `SaveAsync(string suggestedName, System.Func<System.IO.Stream, System.Threading.Tasks.Task> write, XForma.Toolkit.FilePickerOptions options = null) -> System.Threading.Tasks.Task<bool>` | Lets the user choose where to save a file, then writes it with the callback. Returns false when they cancel. |

## IHapticFeedback

Haptic feedback, where the device has it.

| Member | |
|---|---|
| `IsSupported.get -> bool` |  |
| `Perform(XForma.Toolkit.HapticFeedbackType type) -> void` |  |

## ILauncher

Opens URIs in the apps that handle them, such as the web browser.

| Member | |
|---|---|
| `OpenAsync(System.Uri uri) -> System.Threading.Tasks.Task<bool>` | Opens the URI. Completes with whether an app opened it. |

## IPreferences

Small settings kept between runs, as strings. Not for secrets: see `ISecureStorage`.

| Member | |
|---|---|
| `Clear() -> void` |  |
| `ContainsKey(string key) -> bool` |  |
| `Get(string key, string defaultValue = null) -> string` | The value, or the default when it isn't set. |
| `Remove(string key) -> void` |  |
| `Set(string key, string value) -> void` |  |

## ISecureStorage

Secrets kept between runs, encrypted by the platform: DPAPI on Windows, the Android Keystore, and the iOS Keychain.

| Member | |
|---|---|
| `GetAsync(string key) -> System.Threading.Tasks.Task<string>` | The value, or null when it isn't set. |
| `Remove(string key) -> bool` |  |
| `RemoveAll() -> void` |  |
| `SetAsync(string key, string value) -> System.Threading.Tasks.Task` |  |

## IShare

Shares text and links through the system's share sheet.

| Member | |
|---|---|
| `RequestAsync(XForma.Toolkit.ShareRequest request) -> System.Threading.Tasks.Task` |  |

## Launcher

Opening URIs in other apps (such as the browser): `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.ILauncher` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## NetworkAccess

How far the device's network reaches.

| Member | |
|---|---|
| `Internet = 3 -> XForma.Toolkit.NetworkAccess` | The internet. |
| `Local = 2 -> XForma.Toolkit.NetworkAccess` | A network without the internet (or not yet confirmed to reach it). |
| `None = 1 -> XForma.Toolkit.NetworkAccess` | No network. |
| `Unknown = 0 -> XForma.Toolkit.NetworkAccess` | The platform can't tell. |

## PickedFile

A file the user picked. Mobile platforms grant access to the file only (not a path), so read it with `OpenReadAsync`.

| Member | |
|---|---|
| `Name.get -> string` | The file's name, with its extension. |
| `OpenReadAsync() -> System.Threading.Tasks.Task<System.IO.Stream>` |  |
| `PickedFile(string name, System.Func<System.Threading.Tasks.Task<System.IO.Stream>> open) -> void` |  |

## Preferences

Simple settings that last between runs: `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.IPreferences` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## PreferencesExtensions

Typed values in preferences, stored as invariant strings.

| Member | |
|---|---|
| `Get<T>(this XForma.Toolkit.IPreferences preferences, string key, T defaultValue) -> T` |  |
| `Set<T>(this XForma.Toolkit.IPreferences preferences, string key, T value) -> void` |  |

## SecureStorage

Secrets, kept encrypted by the platform: `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.ISecureStorage` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## Share

Sharing text and links with other apps: `Default`.

| Member | |
|---|---|
| `Default.get -> XForma.Toolkit.IShare` | The platform's implementation, set when its services are registered (an app or test can set its own). |
| `Default.set -> void` | The platform's implementation, set when its services are registered (an app or test can set its own). |

## ShareRequest

What to share: text, a link, or both.

| Member | |
|---|---|
| `ShareRequest() -> void` |  |
| `Text.get -> string` |  |
| `Text.init -> void` |  |
| `Title.get -> string` |  |
| `Title.init -> void` |  |
| `Uri.get -> System.Uri` |  |
| `Uri.init -> void` |  |
