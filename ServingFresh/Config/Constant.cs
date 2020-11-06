using System;
namespace ServingFresh.Config
{
    public class Constant
    {
        // FACEBOOK CONSTANTS
        public static string FacebookScope = "email";
        public static string FacebookAuthorizeUrl = "https://www.facebook.com/dialog/oauth/";
        public static string FacebookAccessTokenUrl = "https://www.facebook.com/connect/login_success.html";
        public static string FacebookUserInfoUrl = "https://graph.facebook.com/me?fields=email,name,picture&access_token=";

        // FACEBOOK ID 
        public static string FacebookAndroidClientID = "257223515515874";
        public static string FacebookiOSClientID = "257223515515874";

        // FACEBOOK REDIRECT
        public static string FacebookiOSRedirectUrl = "https://www.facebook.com/connect/login_success.html:/oauth2redirect";
        public static string FacebookAndroidRedirectUrl = "https://www.facebook.com/connect/login_success.html";

        // GOOGLE CONSTANTS
        public static string GoogleScope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email";
        public static string GoogleAuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        public static string GoogleAccessTokenUrl = "https://www.googleapis.com/oauth2/v4/token";
        public static string GoogleUserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

        // GOOGLE ID
        public static string GoogleiOSClientID = "97916302968-f22boafqno1dicq4a0eolpr6qj8hkvbm.apps.googleusercontent.com";
        public static string GoogleAndroidClientID = "97916302968-7una3voi6tjhf92jmvf87rdaeblaaf3s.apps.googleusercontent.com";

        // GOOGLE REDIRECT
        public static string GoogleRedirectUrliOS = "com.googleusercontent.apps.97916302968-f22boafqno1dicq4a0eolpr6qj8hkvbm:/oauth2redirect";
        public static string GoogleRedirectUrlAndroid = "com.googleusercontent.apps.97916302968-7una3voi6tjhf92jmvf87rdaeblaaf3s:/oauth2redirect";

        // ENDPOINTS SF
        public static string AccountSaltUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/AccountSalt";
        public static string LogInUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/Login/";
        public static string SignUpUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/SignUp";
        public static string UpdateTokensUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/token_fetch_update/update_mobile";
        public static string GetUserInfoUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/token_fetch_update/get";
        public static string ZoneUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/Categorical_Options/";
        public static string PurchaseUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/purchase_Data_SF";
        public static string GetItemsUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/getItems";
        public static string GetHistoryUrl = "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/history/";

        // RDS CODES
        public static string EmailNotFound = "404";
        public static string ErrorPlatform = "411";
        public static string ErrorUserDirectLogIn = "406";
        public static string UseSocialMediaLogin = "401";
        public static string AutheticatedSuccesful = "200";

        // PLATFORM
        public static string Google = "GOOGLE";
        public static string Facebook = "FACEBOOK";
        public static string Apple = "APPLE";

        // EXTENDED TIME
        public static double days = 14;

        // SERVICE FEES
        public static double deliveryFee = 1.50;
        public static double serviceFee = 5.00;

        // STRIPE KEYS
        public static string StipeKey = "sk_test_fe99fW2owhFEGTACgW3qaykd006gHUwj1j";
        public static string Contry = "US";
    }
}
