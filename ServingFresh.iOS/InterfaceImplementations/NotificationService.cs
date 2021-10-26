using System;
using Foundation;
using ServingFresh.iOS.AppleNotificationService;
using ServingFresh.Notifications;
using UIKit;
using UserNotifications;

[assembly: Xamarin.Forms.Dependency(typeof(NotificationService))]
namespace ServingFresh.iOS.AppleNotificationService
{
    public class NotificationService : NSObject, INotifications
    {
        public NotificationService()
        {
        }

        public bool IsNotifications()
        {
            return RegistedDeviceToPushNotifications();
        }

        public bool RegistedDeviceToPushNotifications()
        {
            //UIApplication.SharedApplication.UnregisterForRemoteNotifications();
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
            //return false;
        }
    }
}
