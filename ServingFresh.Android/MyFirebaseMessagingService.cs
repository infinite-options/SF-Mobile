using System;
using Android.Util;
using Firebase.Messaging;
using Android.Support.V4.App;
using WindowsAzure.Messaging;
using Android.App;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Xamarin.Essentials;
using Xamarin.Forms;
using ServingFresh;
using ServingFresh.Droid;

namespace InfiniteMeals.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            // NOTE: save token instance locally, or log if desired
            Console.WriteLine("New Token:" + token);
            SendRegistrationToServer(token);
        }

        void SendRegistrationToServer(string token)
        {
            if (Preferences.Get("guid", null) != null)
            {
                return;
            }
            try
            {
                NotificationHub hub = new NotificationHub(AppConstants.NotificationHubName, AppConstants.ListenConnectionString, this);
                var guid = Guid.NewGuid();
                var tag = "guid_" + guid.ToString();
                Console.WriteLine("guid:" + tag);
                Preferences.Set("guid", tag);
                string[] tags = new string[2] { "default",tag };
           
                // register device with Azure Notification Hub using the token from FCM
                Registration registration = hub.Register(token, tags);

                // subscribe to the SubscriptionTags list with a simple template.
                string pnsHandle = registration.PNSHandle;
                TemplateRegistration templateReg = hub.RegisterTemplate(pnsHandle, "defaultTemplate", AppConstants.FCMTemplateBody, tags);
            }
            catch (Exception e)
            {
                Log.Error(AppConstants.DebugTag, $"Error registering device: {e.Message}");
            }
        }
        public override void OnMessageReceived(RemoteMessage message)
        {
            
            base.OnMessageReceived(message);
            string messageBody = string.Empty;

            Console.WriteLine("Received Notification: " + messageBody);

            if (message.GetNotification() != null)
            {
                messageBody = message.GetNotification().Body;
            }

            // NOTE: test messages sent via the Azure portal will be received here
            else
            {
                messageBody = message.Data.Values.First();
            }

            // convert the incoming message to a local notification
            SendLocalNotification(messageBody);

            // send the incoming message directly to the MainPage
            SendMessageToMainPage(messageBody);
        }

        void SendLocalNotification(string body)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("message", body);

            //Unique request code to avoid PendingIntent collision.
            var requestCode = new Random().Next();
            var pendingIntent = PendingIntent.GetActivity(this, requestCode, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this)
                .SetContentTitle("Serving Now")
                .SetSmallIcon(ServingFresh.Droid.Resource.Drawable.ic_launcher)
                .SetContentText(body)
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                notificationBuilder.SetChannelId(AppConstants.NotificationChannelName);
            }

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }

        void SendMessageToMainPage(string body)
        {
            //(App.Current.MainPage as MainPage)?.AddMessage(body);
            return;
        }

        public FirebaseService()
        {

        }
    }
}
