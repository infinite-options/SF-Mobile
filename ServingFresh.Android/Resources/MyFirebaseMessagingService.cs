using System;
using Android.Util;
using Firebase.Messaging;
using Android.Support.V4.App;
using WindowsAzure.Messaging;
using Android.App;
using System.Linq;
using Android.Content;
using System.Collections.Generic;
using Xamarin.Essentials;
using Android.OS;

//namespace ServingFresh.Droid.Resources
//{
//    // Step 13 of Set up notification hubs in your project
//    [Service]
//    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
//    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
//    public class MyFirebaseMessagingService : FirebaseMessagingService
//    {

//        // Step 14 of Set up notification hubs in your project
//        const string TAG = "MyFirebaseMsgService";
//        NotificationHub hub;

//        public override void OnMessageReceived(RemoteMessage message)
//        {
//            Log.Debug(TAG, "From: " + message.From);
//            if (message.GetNotification() != null)
//            {
//                //These is how most messages will be received
//                Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
//                SendNotification(message.GetNotification().Body);
//            }
//            else
//            {
//                //Only used for debugging payloads sent from the Azure portal
//                SendNotification(message.Data.Values.First());

//            }
//        }

//        void SendNotification(string messageBody)
//        {
//            var intent = new Intent(this, typeof(MainActivity));
//            intent.AddFlags(ActivityFlags.ClearTop);
//            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

//            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);

//            notificationBuilder.SetContentTitle("FCM Message")
//                        .SetSmallIcon(Resource.Drawable.ic_launcher)
//                        .SetContentText(messageBody)
//                        .SetAutoCancel(true)
//                        .SetShowWhen(false)
//                        .SetContentIntent(pendingIntent);

//            var notificationManager = NotificationManager.FromContext(this);

//            notificationManager.Notify(0, notificationBuilder.Build());
//        }



//        // Step 15 of Set up notification hubs in your project
//        public override void OnNewToken(string token)
//        {
//            Log.Debug(TAG, "FCM token: " + token);
//            SendRegistrationToServer(token);
//        }

//        void SendRegistrationToServer(string token)
//        {
//            // Register with Notification Hubs
//            hub = new NotificationHub(Constants.NotificationHubName,
//                                        Constants.ListenConnectionString, this);

//            var tags = new List<string>() { };
//            var regID = hub.Register(token, tags.ToArray()).RegistrationId;

//            Log.Debug(TAG, $"Successful registration of ID {regID}");
//        }

//    }

//}


namespace ServingFresh.Droid
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
                var tag = Preferences.Get("guid", null);
                Console.WriteLine("guid:" + tag);
                Console.WriteLine("token:" + token);
                return;
            }
            try
            {
                NotificationHub hub = new NotificationHub(AppConstants.NotificationHubName, AppConstants.ListenConnectionString, this);
                var guid = Guid.NewGuid();
                var tag = "guid_" + guid.ToString();
                Console.WriteLine("guid:" + tag);
                Console.WriteLine("token:" + token);
                Preferences.Set("guid", tag);
                string[] tags = new string[2] { "default", tag };

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
                .SetSmallIcon(Resource.Drawable.ic_launcher)
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
