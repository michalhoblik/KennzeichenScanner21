using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System;

namespace KennzeichenScanner21.Droid
{
    [Activity(Label = "KennzeichenScanner21", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Action<int, Result, Intent> _activityResultCallback;

        internal static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            Instance = this;
            LoadApplication(new App());
        }

        public void ConfigureActivityResultCallback(Action<int, Result, Intent> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            _activityResultCallback = callback;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (_activityResultCallback != null)
            {
                _activityResultCallback.Invoke(requestCode, resultCode, data);
                _activityResultCallback = null;
            }
        }
    }
}

