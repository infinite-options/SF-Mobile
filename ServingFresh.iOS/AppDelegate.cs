using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Foundation;
using ServingFresh.LogIn.Classes;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using UserNotifications;
using ServingFresh.Notifications;

namespace ServingFresh.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        



        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Auth.Presenters.XamarinIOS.AuthenticationConfiguration.Init();
            global::Xamarin.Forms.Forms.Init();
            Xamarin.FormsMaps.Init();
            global::Xamarin.Forms.Forms.Init();

            //UIApplication.SharedApplication.UnregisterForRemoteNotifications();
            //var answer = RegistedDeviceToPushNotifications();
            //IsEnable = answer;
            //System.Diagnostics.Debug.WriteLine(answer);
            LoadApplication(new App());

            base.FinishedLaunching(app, options);

            UINavigationBar.Appearance.TintColor = Color.FromHex("#FFFFFF").ToUIColor();

            //UIView statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
            //if (statusBar != null && statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
            //{
            //    // change to your desired color 
            //    statusBar.BackgroundColor = Color.FromHex("#7f6550").ToUIColor();
            //}

            // Color of the selected tab icon:
            UITabBar.Appearance.SelectedImageTintColor = Color.FromHex("#a0050f").ToUIColor();

            // Color of the tabbar background:
            //UITabBar.Appearance.BarTintColor = UIColor.FromRGB(247, 247, 247);

            // Color of the selected tab text color:
            UITabBarItem.Appearance.SetTitleTextAttributes(
                new UITextAttributes()
                {
                    TextColor = Color.FromHex("#a0050f").ToUIColor()
                },
                UIControlState.Selected);

            // Color of the unselected tab icon & text:
            UITabBarItem.Appearance.SetTitleTextAttributes(
                new UITextAttributes()
                {
                    TextColor = Color.FromHex("#000000").ToUIColor()
                },
                UIControlState.Normal);
            UIView.AppearanceWhenContainedIn(typeof(UIAlertView)).TintColor = Color.FromHex("#a0050f").ToUIColor();
            UIView.AppearanceWhenContainedIn(typeof(UIAlertController)).TintColor = Color.FromHex("#a0050f").ToUIColor();
            return true;
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            // Convert NSUrl to Uri
            var uri = new Uri(url.AbsoluteString);

            // Load redirectUrl page
            AuthenticationState.Authenticator.OnPageLoading(uri);

            return true;
        }

        public bool RegistedDeviceToPushNotifications()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                                                                        (granted, error) => InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications));
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }
            return UIApplication.SharedApplication.IsRegisteredForRemoteNotifications;
        }
    }
}
