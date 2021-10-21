using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;
using Android.Util;
using Android.Gms.Common;

namespace ServingFresh.Droid
{
    [Activity(Label = "Serving Fresh", Icon = "@mipmap/icon2", Theme = "@style/MainTheme" , MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        // Step 7 of Set up notification hubs in your project
        public const string TAG = "MainActivity";
        internal static readonly string CHANNEL_ID = "my_notification_channel";


        // Copied from Serving Now - Not sure if this is needed so commented out
        //readonly string[] permissionGroup =
        //{
        //    Manifest.Permission.ReadExternalStorage,
        //    Manifest.Permission.WriteExternalStorage,
        //    Manifest.Permission.Camera
        //};

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            // Copied from Serving Now - Not sure if this is needed so commented out
            //GoogleClientManager.Initialize(this);


            // Step 10 of Set up notification hubs in your project
            if (Intent.Extras != null)
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    if (key != null)
                    {
                        var value = Intent.Extras.GetString(key);
                        Log.Debug(TAG, "Key: {0} Value: {1}", key, value);
                    }
                }
            }

            IsPlayServicesAvailable();
            CreateNotificationChannel();





            global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, savedInstanceState);
            global::Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
            Xamarin.FormsMaps.Init(this, savedInstanceState);
            UserDialogs.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            // Copied from Serving Now - Not sure if this is needed so commented out
            //RequestPermissions(permissionGroup, 0);
            //UserDialogs.Init(this);
        }


        // Copied from Serving Now - Not sure if this is needed so commented out
        //protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        //{
        //    base.OnActivityResult(requestCode, resultCode, data);
        //    GoogleClientManager.OnAuthCompleted(requestCode, resultCode, data);
        //}



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }



        // Step 8 of Set up notification hubs in your project
        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    Log.Debug(TAG, GoogleApiAvailability.Instance.GetErrorString(resultCode));
                else
                {
                    Log.Debug(TAG, "This device is not supported");
                    Finish();
                }
                return false;
            }

            Log.Debug(TAG, "Google Play Services is available.");
            return true;
        }


        // Step 9 of Set up notification hubs in your project
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = CHANNEL_ID;
            var channelDescription = string.Empty;
            var channel = new NotificationChannel(CHANNEL_ID, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

    }
}