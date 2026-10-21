using System;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;

namespace XForms.Android
{
    /// <summary>
    /// Android's services for the application.
    /// </summary>
    internal static class AndroidServices
    {
        public static PlatformServices Create(
            AndroidApplication host)
        {
            var context = host.Context;
            return new PlatformServices()
            {
                Clipboard = new AndroidClipboard(context),
                Launcher = new IntentLauncher(host),
                Preferences = new SharedPreferencesStore(context, ".preferences"),
                SecureStorage = new KeystoreStorage(context),
                Share = new IntentShare(host),
                HapticFeedback = new ViewHapticFeedback(host),
                Connectivity = new AndroidConnectivity(host),
                FilePicker = new DocumentPicker(host),
            };
        }

        // Starts an activity from the app's activity (or, without one, in a
        // new task)
        public static void StartActivity(
            AndroidApplication host,
            Intent intent)
        {
            var activity = host.Activity;
            if (null == activity)
            {
                intent.AddFlags(ActivityFlags.NewTask);
                host.Context.StartActivity(intent);
                return;
            }

            activity.StartActivity(intent);
        }
    }

    internal sealed class AndroidClipboard : IClipboard
    {
        private readonly Context _context;

        public AndroidClipboard(
            Context context)
        {
            this._context = context;
        }

        private ClipboardManager Manager
        {
            get
            {
                return (ClipboardManager)this._context.GetSystemService(Context.ClipboardService);
            }
        }

        public bool HasText
        {
            get
            {
                return this.Manager.HasPrimaryClip;
            }
        }

        public Task<string> GetTextAsync()
        {
            var clip = this.Manager.PrimaryClip;
            string text = ((null != clip) && (0 < clip.ItemCount)) ? clip.GetItemAt(0).CoerceToText(this._context) : null;
            return Task.FromResult(text);
        }

        public Task SetTextAsync(
            string text)
        {
            this.Manager.PrimaryClip = ClipData.NewPlainText(string.Empty, text ?? string.Empty);
            return Task.CompletedTask;
        }
    }

    internal sealed class IntentLauncher : ILauncher
    {
        private readonly AndroidApplication _host;

        public IntentLauncher(
            AndroidApplication host)
        {
            this._host = host;
        }

        public Task<bool> OpenAsync(
            Uri uri)
        {
            if (null == uri)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(uri.AbsoluteUri));
            try
            {
                AndroidServices.StartActivity(this._host, intent);
                return Task.FromResult(true);
            }
            catch (ActivityNotFoundException)
            {
                return Task.FromResult(false);
            }
        }
    }

    /// <summary>
    /// Preferences in the app's shared preferences.
    /// </summary>
    internal sealed class SharedPreferencesStore : IPreferences
    {
        private readonly ISharedPreferences _preferences;

        public SharedPreferencesStore(
            Context context,
            string suffix)
        {
            this._preferences = context.GetSharedPreferences(context.PackageName + suffix, FileCreationMode.Private);
        }

        public bool ContainsKey(string key) => this._preferences.Contains(key);

        public string Get(
            string key,
            string defaultValue = null)
        {
            return this._preferences.GetString(key, defaultValue);
        }

        public void Set(
            string key,
            string value)
        {
            using (var editor = this._preferences.Edit())
            {
                if (null == value)
                {
                    editor.Remove(key);
                }
                else
                {
                    editor.PutString(key, value);
                }

                editor.Apply();
            }
        }

        public void Remove(
            string key)
        {
            this.Set(key, null);
        }

        public void Clear()
        {
            using (var editor = this._preferences.Edit())
            {
                editor.Clear();
                editor.Apply();
            }
        }
    }

    /// <summary>
    /// Secrets encrypted with an AES key that never leaves the Android
    /// Keystore, kept in shared preferences.
    /// </summary>
    internal sealed class KeystoreStorage : ISecureStorage
    {
        private const string KeyStoreName = "AndroidKeyStore";
        private const string Transformation = "AES/GCM/NoPadding";

        private readonly string _alias;
        private readonly SharedPreferencesStore _store;

        public KeystoreStorage(
            Context context)
        {
            this._alias = context.PackageName + ".xforms.secure";
            this._store = new SharedPreferencesStore(context, ".secure");
        }

        private IKey GetKey()
        {
            var keyStore = KeyStore.GetInstance(KeyStoreName);
            keyStore.Load(null);
            if (!keyStore.ContainsAlias(this._alias))
            {
                var generator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, KeyStoreName);
                generator.Init(new KeyGenParameterSpec.Builder(this._alias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(KeyProperties.BlockModeGcm)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingNone)
                    .Build());
                return generator.GenerateKey();
            }

            // NOTE: A Java object, used through its interface (a C# cast fails)
            return keyStore.GetKey(this._alias, null);
        }

        public Task<string> GetAsync(
            string key)
        {
            string stored = this._store.Get(key);
            if (null == stored)
            {
                return Task.FromResult<string>(null);
            }

            // The initialization vector, then the encrypted value
            string[] parts = stored.Split(':');
            var cipher = Cipher.GetInstance(Transformation);
            cipher.Init(CipherMode.DecryptMode, this.GetKey(), new GCMParameterSpec(128, Convert.FromBase64String(parts[0])));
            byte[] data = cipher.DoFinal(Convert.FromBase64String(parts[1]));
            return Task.FromResult(Encoding.UTF8.GetString(data));
        }

        public Task SetAsync(
            string key,
            string value)
        {
            if (null == value)
            {
                this.Remove(key);
                return Task.CompletedTask;
            }

            var cipher = Cipher.GetInstance(Transformation);
            cipher.Init(CipherMode.EncryptMode, this.GetKey());
            byte[] encrypted = cipher.DoFinal(Encoding.UTF8.GetBytes(value));
            this._store.Set(key, Convert.ToBase64String(cipher.GetIV()) + ":" + Convert.ToBase64String(encrypted));
            return Task.CompletedTask;
        }

        public bool Remove(
            string key)
        {
            bool exists = this._store.ContainsKey(key);
            this._store.Remove(key);
            return exists;
        }

        public void RemoveAll()
        {
            this._store.Clear();
        }
    }

    internal sealed class IntentShare : IShare
    {
        private readonly AndroidApplication _host;

        public IntentShare(
            AndroidApplication host)
        {
            this._host = host;
        }

        public Task RequestAsync(
            ShareRequest request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string text = string.Join("\n", new[] { request.Text, request.Uri?.AbsoluteUri }).Trim();
            var intent = new Intent(Intent.ActionSend);
            intent.SetType("text/plain");
            intent.PutExtra(Intent.ExtraText, text);
            if (null != request.Title)
            {
                intent.PutExtra(Intent.ExtraSubject, request.Title);
            }

            AndroidServices.StartActivity(this._host, Intent.CreateChooser(intent, request.Title));
            return Task.CompletedTask;
        }
    }

    internal sealed class ViewHapticFeedback : IHapticFeedback
    {
        private readonly AndroidApplication _host;

        public ViewHapticFeedback(
            AndroidApplication host)
        {
            this._host = host;
        }

        public bool IsSupported => true;

        public void Perform(
            HapticFeedbackType type)
        {
            var feedback = (type == HapticFeedbackType.LongPress)
                ? global::Android.Views.FeedbackConstants.LongPress
                : global::Android.Views.FeedbackConstants.VirtualKey;
            this._host.Activity?.Window?.DecorView.PerformHapticFeedback(feedback);
        }
    }
}
