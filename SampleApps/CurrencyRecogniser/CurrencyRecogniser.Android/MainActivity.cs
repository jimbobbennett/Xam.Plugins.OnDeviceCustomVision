using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Xam.Plugins.OnDeviceCustomVision;
using System.IO;

namespace CurrencyRecogniser.Droid
{
    [Activity(Label = "CurrencyRecogniser", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        readonly string labels = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "labels.txt");
        readonly string model = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "model.pb");


        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            CrossCurrentActivity.Current.Init(this, bundle);

            CopyModelToApplicationFolder();

            AndroidImageClassifier.Init(model, labels);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        private void CopyModelToApplicationFolder()
        {
            var assets = Application.Context.Assets;

            if (File.Exists(labels))
            {
                File.Delete(labels);
            }

            if (!File.Exists(labels))
            {
                using (var sr = assets.Open("labels.txt"))
                using (var fileStream = File.OpenWrite(labels))
                    sr.CopyTo(fileStream);
            }

            if (!File.Exists(model))
            {
                using (var sr = assets.Open("model.pb"))
                using (var fileStream = File.OpenWrite(model))
                    sr.CopyTo(fileStream);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

