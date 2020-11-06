using System;
namespace ServingFresh
{
    public static class AppConstants
    {
        //public static string NotificationChannelName { get; set; } = "XamarinNotifyChannel";
        //public static string NotificationHubName { get; set; } = "Serving-Now-Notifications";
        //public static string ListenConnectionString { get; set; } = "Endpoint=sb://serving-now-notifications-namespace.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=WsS2X3SXFSoTZPXCafmms71Ezsey+8yCyBturtVmoCQ=";
        public static string NotificationChannelName { get; set; } = "XamarinNotifyChannel";
        public static string NotificationHubName { get; set; } = "Serving-Fresh-Notification-Hub";
        public static string ListenConnectionString { get; set; } = "Endpoint=sb://serving-fresh-notification-namespace.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=56+I0UhYCRohkoUYHgb4dHHyWTx8puGBv+PvaDLp6K8=";

        public static string DebugTag { get; set; } = "XamarinNotify";
        public static string[] SubscriptionTags { get; set; } = { "default" };
        public static string FCMTemplateBody { get; set; } = "{\"data\":{\"message\":\"$(messageParam)\"}}";
        public static string APNTemplateBody { get; set; } = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";
    }
}