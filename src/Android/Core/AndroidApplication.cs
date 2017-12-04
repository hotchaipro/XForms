using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Android.App.Application;

namespace XForms.Android
{
    public class AndroidApplicationActivity : global::Android.App.Activity
    {
        public event EventHandler Suspending;
        public event EventHandler Resuming;

        public AndroidApplicationActivity()
        {
        }

        protected override void OnPause()
        {
            base.OnPause();

            this?.Suspending.Invoke(this, EventArgs.Empty);
        }

        protected override void OnResume()
        {
            base.OnResume();

            this?.Resuming.Invoke(this, EventArgs.Empty);
        }
    }
}
