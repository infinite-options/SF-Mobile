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
using WindowsAzure.Messaging;
using System.Diagnostics;
using Xamarin.Essentials;

namespace ServingFresh.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {


        private SBNotificationHub Hub { get; set; }


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
            // "#FF8500"
            UINavigationBar.Appearance.TintColor = Color.FromHex("#FFFFFF").ToUIColor();

            //UIView statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
            //if (statusBar != null && statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
            //{
            //    // change to your desired color 
            //    statusBar.BackgroundColor = Color.FromHex("#7f6550").ToUIColor();
            //}

            // Color of the selected tab icon:
            //UITabBar.Appearance.SelectedImageTintColor = Color.FromHex("#a0050f").ToUIColor();
            UITabBar.Appearance.SelectedImageTintColor = Color.FromHex("#FF8500").ToUIColor();

            // Color of the tabbar background:
            //UITabBar.Appearance.BarTintColor = UIColor.FromRGB(247, 247, 247);

            // Color of the selected tab text color:
            UITabBarItem.Appearance.SetTitleTextAttributes(
                new UITextAttributes()
                {
                    //TextColor = Color.FromHex("#a0050f").ToUIColor()
                    TextColor = Color.FromHex("#FF8500").ToUIColor()
                },
                UIControlState.Selected);

            // Color of the unselected tab icon & text:
            UITabBarItem.Appearance.SetTitleTextAttributes(
                new UITextAttributes()
                {
                    TextColor = Color.FromHex("#000000").ToUIColor()
                },
                UIControlState.Normal);
            //UIView.AppearanceWhenContainedIn(typeof(UIAlertView)).TintColor = Color.FromHex("#a0050f").ToUIColor();
            UIView.AppearanceWhenContainedIn(typeof(UIAlertView)).TintColor = Color.FromHex("#FF8500").ToUIColor();

            //UIView.AppearanceWhenContainedIn(typeof(UIAlertController)).TintColor = Color.FromHex("#a0050f").ToUIColor();
            UIView.AppearanceWhenContainedIn(typeof(UIAlertController)).TintColor = Color.FromHex("#FF8500").ToUIColor();

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
            // ADDED LINE FOR NOTIFICATIONS WHEN APP IS OPEN
            UNUserNotificationCenter.Current.Delegate = new NotificationDelegate();
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

        //public bool RegistedDeviceToPushNotifications()
        //{
        //    if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
        //    {
        //        UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
        //                                                                (granted, error) => InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications));
        //    }
        //    else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
        //    {
        //        var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
        //                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
        //                new NSSet());

        //        UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
        //        UIApplication.SharedApplication.RegisterForRemoteNotifications();
        //    }
        //    else
        //    {
        //        UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
        //        UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
        //    }
        //    return UIApplication.SharedApplication.IsRegisteredForRemoteNotifications;
        //}

        //public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        //{
        //    if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
        //    {
        //        UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
        //                                                                (granted, error) => InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications));
        //    }
        //    else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
        //    {
        //        var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
        //                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
        //                new NSSet());

        //        UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
        //        UIApplication.SharedApplication.RegisterForRemoteNotifications();
        //    }
        //    else
        //    {
        //        UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
        //        UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
        //    }

        //    return true;
        //}

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            Debug.WriteLine("Registered");
            if (Preferences.Get("guid", null) != null)
            {
                return;
            }

            Hub = new SBNotificationHub(AppConstants.ListenConnectionString, AppConstants.NotificationHubName);

            // update registration with Azure Notification Hub
            Hub.UnregisterAll(deviceToken, (error) =>
            {
                if (error != null)
                {
                    Debug.WriteLine($"Unable to call unregister {error}");
                    return;
                }

                var guid = Guid.NewGuid();
                var tag = "guid_" + guid.ToString();
                Console.WriteLine("guid:" + tag);
                Preferences.Set("guid", tag);
                System.Diagnostics.Debug.WriteLine("This is the GUID from RegisteredForRemoteNotifications: " + Preferences.Get("guid", string.Empty));
                var tags = new NSSet(AppConstants.SubscriptionTags.Append(tag).ToArray());

                Preferences.Set("Token", deviceToken.ToString());

                Hub.RegisterNative(deviceToken, tags, (errorCallback) =>
                {
                    if (errorCallback != null)
                    {
                        Debug.WriteLine($"RegisterNativeAsync error: {errorCallback}");
                    }
                });

                var templateExpiration = DateTime.Now.AddDays(120).ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                Hub.RegisterTemplate(deviceToken, "defaultTemplate", AppConstants.APNTemplateBody, templateExpiration, tags, (errorCallback) =>
                {
                    if (errorCallback != null)
                    {
                        if (errorCallback != null)
                        {
                            Debug.WriteLine($"RegisterTemplateAsync error: {errorCallback}");
                        }
                    }
                });
            });
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            ProcessNotification(userInfo, false);
        }

        void ProcessNotification(NSDictionary options, bool fromFinishedLaunching)
        {
            // Check to see if the dictionary has the aps key.  This is the notification payload you would have sent
            if (null != options && options.ContainsKey(new NSString("aps")))
            {
                //Get the aps dictionary
                NSDictionary aps = options.ObjectForKey(new NSString("aps")) as NSDictionary;

                string alert = string.Empty;

                //Extract the alert text
                // NOTE: If you're using the simple alert by just specifying
                // "  aps:{alert:"alert msg here"}  ", this will work fine.
                // But if you're using a complex alert with Localization keys, etc.,
                // your "alert" object from the aps dictionary will be another NSDictionary.
                // Basically the JSON gets dumped right into a NSDictionary,
                // so keep that in mind.
                if (aps.ContainsKey(new NSString("alert")))
                    alert = (aps[new NSString("alert")] as NSString).ToString();

                //If this came from the ReceivedRemoteNotification while the app was running,
                // we of course need to manually process things like the sound, badge, and alert.
                if (!fromFinishedLaunching)
                {
                    //Manually show an alert
                    if (!string.IsNullOrEmpty(alert))
                    {
                        var myAlert = UIAlertController.Create("Notification", alert, UIAlertControllerStyle.Alert);
                        myAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(myAlert, true, null);
                    }
                }
            }
        }

        // ADDED LINE FOR NOTIFICATIONS WHEN APP IS OPEN
        public class NotificationDelegate : UNUserNotificationCenterDelegate {
            public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler) {completionHandler(UNNotificationPresentationOptions.Alert); }
        }
    }
}
