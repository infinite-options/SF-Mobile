using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServingFresh.LogIn.Classes;
using ServingFresh.Config;
using Xamarin.Forms;
using Xamarin.Auth;
using System.Windows.Input;
using Xamarin.Essentials;
using ServingFresh.LogIn.Apple;
using ServingFresh.Notifications;
using System.Diagnostics;
using ServingFresh.Models;
using Acr.UserDialogs;

namespace ServingFresh.Views
{
    public partial class LogInPage : ContentPage
    {
        public event EventHandler SignIn;
        public bool createAccount = false;
        INotifications appleNotification = DependencyService.Get<INotifications>();
        private string deviceId;

        public LogInPage()
        {
            InitializeComponent();
            InitializeAppProperties();
            
            if (Device.RuntimePlatform == Device.Android)
            {
                System.Diagnostics.Debug.WriteLine("Running on Android: Line 32");
                Console.WriteLine("guid: " + Preferences.Get("guid", null));
                appleLogInButton.IsEnabled = false;
            }
            else
            {
                InitializedAppleLogin();
                appleNotification.IsNotifications();
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
            }
            else
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
            }
        }

        public LogInPage(string title, string message)
        {
            InitializeComponent();
            InitializeAppProperties();

            if (Device.RuntimePlatform == Device.Android)
            {
                System.Diagnostics.Debug.WriteLine("Running on Android: Line 32");
                Console.WriteLine("guid: " + Preferences.Get("guid", null));
                appleLogInButton.IsEnabled = false;
            }
            else
            {
                InitializedAppleLogin();
                appleNotification.IsNotifications();
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
            }
            else
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
            }
            MessageFromSelectionPage(title,message);
        }

        public async void MessageFromSelectionPage(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }

        public void InitializeAppProperties()
        {
            Application.Current.Properties["user_email"] = "";
            Application.Current.Properties["user_first_name"] = "";
            Application.Current.Properties["user_last_name"] = "";
            Application.Current.Properties["user_phone_num"] = "";
            Application.Current.Properties["user_address"] = "";
            Application.Current.Properties["user_unit"] = "";
            Application.Current.Properties["user_city"] = "";
            Application.Current.Properties["user_state"] = "";
            Application.Current.Properties["user_zip_code"] = "";
            Application.Current.Properties["user_latitude"] = "";
            Application.Current.Properties["user_longitude"] = "";
            Application.Current.Properties["user_delivery_instructions"] = "";
        }

        public void InitializedAppleLogin()
        {
            var vm = new LoginViewModel();
            vm.AppleError += AppleError;
            BindingContext = vm;
        }

        private async void DirectLogInClick(System.Object sender, System.EventArgs e)
        {
            logInButton.IsEnabled = false;
            if (String.IsNullOrEmpty(userEmailAddress.Text) || String.IsNullOrEmpty(userPassword.Text))
            { 
                await DisplayAlert("Error", "Please fill in all fields", "OK");
                logInButton.IsEnabled = true;
            }
            else
            {
                var accountSalt = await RetrieveAccountSalt(userEmailAddress.Text.ToLower().Trim());

                if (accountSalt != null)
                {
                    var loginAttempt = await LogInUser(userEmailAddress.Text.ToLower().Trim(), userPassword.Text, accountSalt);

                    if (loginAttempt != null && loginAttempt.message != "Request failed, wrong password.")
                    {
                        var client = new HttpClient();
                        var request = new RequestUserInfo();
                        request.uid = loginAttempt.result[0].customer_uid;

                        var requestSelializedObject = JsonConvert.SerializeObject(request);
                        var requestContent = new StringContent(requestSelializedObject, Encoding.UTF8, "application/json");

                        var clientRequest = await client.PostAsync(Constant.GetUserInfoUrl, requestContent);

                        if (clientRequest.IsSuccessStatusCode)
                        {
                            try
                            {
                                var SFUser = await clientRequest.Content.ReadAsStringAsync();
                                var userData = JsonConvert.DeserializeObject<UserInfo>(SFUser);

                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                Application.Current.Properties["user_id"] = userData.result[0].customer_uid;
                                Application.Current.Properties["time_stamp"] = expDate;
                                Application.Current.Properties["platform"] = "DIRECT";
                                Application.Current.Properties["user_email"] = userData.result[0].customer_email;
                                Application.Current.Properties["user_first_name"] = userData.result[0].customer_first_name;
                                Application.Current.Properties["user_last_name"] = userData.result[0].customer_last_name;
                                Application.Current.Properties["user_phone_num"] = userData.result[0].customer_phone_num;
                                Application.Current.Properties["user_address"] = userData.result[0].customer_address;
                                Application.Current.Properties["user_unit"] = userData.result[0].customer_unit;
                                Application.Current.Properties["user_city"] = userData.result[0].customer_city;
                                Application.Current.Properties["user_state"] = userData.result[0].customer_state;
                                Application.Current.Properties["user_zip_code"] = userData.result[0].customer_zip;
                                Application.Current.Properties["user_latitude"] = userData.result[0].customer_lat;
                                Application.Current.Properties["user_longitude"] = userData.result[0].customer_long;
                                Application.Current.Properties["user_delivery_instructions"] = "";

                                _ = Application.Current.SavePropertiesAsync();

                                if (Device.RuntimePlatform == Device.iOS)
                                {
                                    deviceId = Preferences.Get("guid", null);
                                    if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
                                }
                                else
                                {
                                    deviceId = Preferences.Get("guid", null);
                                    if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
                                }

                                if (deviceId != null)
                                {
                                    NotificationPost notificationPost = new NotificationPost();

                                    notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                    notificationPost.guid = deviceId.Substring(5);
                                    Application.Current.Properties["guid"] = deviceId.Substring(5);
                                    notificationPost.notification = "TRUE";

                                    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                    Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                    Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);

                                    if (clientResponse.IsSuccessStatusCode)
                                    {
                                        System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                    }
                                    else
                                    {
                                        await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                    }
                                }
                                Application.Current.MainPage = new SelectionPage();
                            }catch (Exception ex){

                                System.Diagnostics.Debug.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Wrong password was entered", "OK");
                        logInButton.IsEnabled = true;
                    }
                }
                logInButton.IsEnabled = true;
            }
        }

        public async void AddMessage(string body)
        {
            await DisplayAlert("Aler!", body, "OK");
        }

        private async Task<AccountSalt> RetrieveAccountSalt(string userEmail)
        {
            try
            {
                Debug.WriteLine(userEmail);

                SaltPost saltPost = new SaltPost();
                saltPost.email = userEmail;

                var saltPostSerilizedObject = JsonConvert.SerializeObject(saltPost);
                var saltPostContent = new StringContent(saltPostSerilizedObject, Encoding.UTF8, "application/json");

                Debug.WriteLine(saltPostSerilizedObject);

                var client = new HttpClient();
                var DRSResponse = await client.PostAsync(Constant.AccountSaltUrl, saltPostContent);
                var DRSMessage = await DRSResponse.Content.ReadAsStringAsync();
                Debug.WriteLine(DRSMessage);

                AccountSalt userInformation = null;

                if (DRSResponse.IsSuccessStatusCode)
                {
                    var result = await DRSResponse.Content.ReadAsStringAsync();

                    AcountSaltCredentials data = new AcountSaltCredentials();
                    data = JsonConvert.DeserializeObject<AcountSaltCredentials>(result);

                    if (DRSMessage.Contains(Constant.UseSocialMediaLogin))
                    {
                        createAccount = true;
                        Debug.WriteLine(DRSMessage);
                        await DisplayAlert("Oops!", data.message, "OK");
                    }
                    else if (DRSMessage.Contains(Constant.EmailNotFound))
                    {
                        await DisplayAlert("Oops!", "Our records show that you don't have an accout. Please sign up!", "OK");
                    }
                    else
                    {
                        userInformation = new AccountSalt
                        {
                            password_algorithm = data.result[0].password_algorithm,
                            password_salt = data.result[0].password_salt
                        };
                    }
                }

                return userInformation;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<LogInResponse> LogInUser(string userEmail, string userPassword, AccountSalt accountSalt)
        {
            try
            {
                SHA512 sHA512 = new SHA512Managed();
                var client = new HttpClient();
                byte[] data = sHA512.ComputeHash(Encoding.UTF8.GetBytes(userPassword + accountSalt.password_salt)); 
                string hashedPassword = BitConverter.ToString(data).Replace("-", string.Empty).ToLower(); 

                LogInPost loginPostContent = new LogInPost();
                loginPostContent.email = userEmail;
                loginPostContent.password = hashedPassword;
                loginPostContent.social_id = "";
                loginPostContent.signup_platform = "";

                string loginPostContentJson = JsonConvert.SerializeObject(loginPostContent); 

                var httpContent = new StringContent(loginPostContentJson, Encoding.UTF8, "application/json"); 
                var response = await client.PostAsync(Constant.LogInUrl, httpContent); 
                var message = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(message);

                if (message.Contains(Constant.AutheticatedSuccesful))
                {

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<LogInResponse>(responseContent);
                    return loginResponse;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception message: " + e.Message);
                return null;
            }
        }

        public void FacebookLogInClick(System.Object sender, System.EventArgs e)
        {
            string clientID = string.Empty;
            string redirectURL = string.Empty;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientID = Constant.FacebookiOSClientID;
                    redirectURL = Constant.FacebookiOSRedirectUrl;
                    break;
                case Device.Android:
                    clientID = Constant.FacebookAndroidClientID;
                    redirectURL = Constant.FacebookAndroidRedirectUrl;
                    break;
            }

            var authenticator = new OAuth2Authenticator(clientID, Constant.FacebookScope, new Uri(Constant.FacebookAuthorizeUrl), new Uri(redirectURL), null, false);
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            authenticator.Completed += FacebookAuthenticatorCompleted;
            authenticator.Error += FacebookAutheticatorError;

            presenter.Login(authenticator);
        }

        public async void FacebookAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    Application.Current.MainPage = new SelectionPage(e.Account.Properties["access_token"], "", null, null, "FACEBOOK");
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
            else
            {
                Application.Current.MainPage = new LogInPage();
                await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
            }
        }

        public async void FacebookUserProfileAsync(string accessToken)
        {

            try
            {
                var client = new HttpClient();
                var socialLogInPost = new SocialLogInPost();

                var facebookResponse = client.GetStringAsync(Constant.FacebookUserInfoUrl + accessToken);
                var userData = facebookResponse.Result;

                System.Diagnostics.Debug.WriteLine(userData);

                FacebookResponse facebookData = JsonConvert.DeserializeObject<FacebookResponse>(userData);

                socialLogInPost.email = facebookData.email;
                socialLogInPost.password = "";
                socialLogInPost.social_id = facebookData.id;
                socialLogInPost.signup_platform = "FACEBOOK";

                var socialLogInPostSerialized = JsonConvert.SerializeObject(socialLogInPost);
                var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine(socialLogInPostSerialized);

                var RDSResponse = await client.PostAsync(Constant.LogInUrl, postContent);
                var responseContent = await RDSResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine(responseContent);
                System.Diagnostics.Debug.WriteLine(RDSResponse.IsSuccessStatusCode);

                if (RDSResponse.IsSuccessStatusCode)
                {
                    if (responseContent != null)
                    {
                        if (responseContent.Contains(Constant.EmailNotFound))
                        {
                            var signUp = await DisplayAlert("Message", "It looks like you don't have a Serving Fresh account. Please sign up!", "OK", "Cancel");
                            if (signUp)
                            {
                                Application.Current.MainPage = new SocialSignUp(facebookData.id, facebookData.name, "", facebookData.email, accessToken, accessToken, "FACEBOOK");
                            }
                        }

                        if (responseContent.Contains(Constant.AutheticatedSuccesful))
                        {
                            var data = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                            Application.Current.Properties["user_id"] = data.result[0].customer_uid;

                            UpdateTokensPost updateTokesPost = new UpdateTokensPost();
                            updateTokesPost.uid = data.result[0].customer_uid;
                            updateTokesPost.mobile_access_token = accessToken;
                            updateTokesPost.mobile_refresh_token = accessToken;

                            var updateTokesPostSerializedObject = JsonConvert.SerializeObject(updateTokesPost);
                            var updateTokesContent = new StringContent(updateTokesPostSerializedObject, Encoding.UTF8, "application/json");
                            var updateTokesResponse = await client.PostAsync(Constant.UpdateTokensUrl, updateTokesContent);
                            var updateTokenResponseContent = await updateTokesResponse.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine(updateTokenResponseContent);

                            if (updateTokesResponse.IsSuccessStatusCode)
                            {
                                var request = new RequestUserInfo();
                                request.uid = data.result[0].customer_uid;

                                var requestSelializedObject = JsonConvert.SerializeObject(request);
                                var requestContent = new StringContent(requestSelializedObject, Encoding.UTF8, "application/json");

                                var clientRequest = await client.PostAsync(Constant.GetUserInfoUrl, requestContent);

                                if (clientRequest.IsSuccessStatusCode)
                                {
                                    var SFUser = await clientRequest.Content.ReadAsStringAsync();
                                    var FacebookUserData = JsonConvert.DeserializeObject<UserInfo>(SFUser);

                                    DateTime today = DateTime.Now;
                                    DateTime expDate = today.AddDays(Constant.days);

                                    Application.Current.Properties["user_id"] = data.result[0].customer_uid;
                                    Application.Current.Properties["time_stamp"] = expDate;
                                    Application.Current.Properties["platform"] = "FACEBOOK";
                                    Application.Current.Properties["user_email"] = FacebookUserData.result[0].customer_email;
                                    Application.Current.Properties["user_first_name"] = FacebookUserData.result[0].customer_first_name;
                                    Application.Current.Properties["user_last_name"] = FacebookUserData.result[0].customer_last_name;
                                    Application.Current.Properties["user_phone_num"] = FacebookUserData.result[0].customer_phone_num;
                                    Application.Current.Properties["user_address"] = FacebookUserData.result[0].customer_address;
                                    Application.Current.Properties["user_unit"] = FacebookUserData.result[0].customer_unit;
                                    Application.Current.Properties["user_city"] = FacebookUserData.result[0].customer_city;
                                    Application.Current.Properties["user_state"] = FacebookUserData.result[0].customer_state;
                                    Application.Current.Properties["user_zip_code"] = FacebookUserData.result[0].customer_zip;
                                    Application.Current.Properties["user_latitude"] = FacebookUserData.result[0].customer_lat;
                                    Application.Current.Properties["user_longitude"] = FacebookUserData.result[0].customer_long;

                                    _ = Application.Current.SavePropertiesAsync();

                                    if (Device.RuntimePlatform == Device.iOS)
                                    {
                                        deviceId = Preferences.Get("guid", null);
                                        if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
                                    }
                                    else
                                    {
                                        deviceId = Preferences.Get("guid", null);
                                        if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
                                    }
                                    
                                    if (deviceId != null)
                                    {
                                        NotificationPost notificationPost = new NotificationPost();

                                        notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                        notificationPost.guid = deviceId.Substring(5);
                                        Application.Current.Properties["guid"] = deviceId.Substring(5);
                                        notificationPost.notification = "TRUE";

                                        var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                        Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                        var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                        var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                        Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);

                                        if (clientResponse.IsSuccessStatusCode)
                                        {
                                            System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                        }
                                        else
                                        {
                                            await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                        }
                                    }

                                    Application.Current.MainPage = new SelectionPage();
                                }
                                else
                                {
                                    await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Oops", "We are facing some problems with our internal system. We weren't able to update your credentials", "OK");
                            }
                        }

                        if (responseContent.Contains(Constant.ErrorPlatform))
                        {
                            var RDSCode = JsonConvert.DeserializeObject<RDSLogInMessage>(responseContent);
                            await DisplayAlert("Message", RDSCode.message, "OK");
                        }

                        if (responseContent.Contains(Constant.ErrorUserDirectLogIn))
                        {
                            await DisplayAlert("Oops!", "You have an existing Serving Fresh account. Please use direct login", "OK");
                        }
                    }
                }
            }catch (Exception facebook)
            {
                Debug.WriteLine(facebook.Message);
            }
        }

        private async void FacebookAutheticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }
            Application.Current.MainPage = new LogInPage();
            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }

        public void GoogleLogInClick(System.Object sender, System.EventArgs e)
        {
            string clientId = string.Empty;
            string redirectUri = string.Empty;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientId = Constant.GoogleiOSClientID;
                    redirectUri = Constant.GoogleRedirectUrliOS;
                    break;

                case Device.Android:
                    clientId = Constant.GoogleAndroidClientID;
                    redirectUri = Constant.GoogleRedirectUrlAndroid;
                    break;
            }

            var authenticator = new OAuth2Authenticator(clientId, string.Empty, Constant.GoogleScope, new Uri(Constant.GoogleAuthorizeUrl), new Uri(redirectUri), new Uri(Constant.GoogleAccessTokenUrl), null, true);
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            authenticator.Completed += GoogleAuthenticatorCompleted;
            authenticator.Error += GoogleAuthenticatorError;

            AuthenticationState.Authenticator = authenticator;
            presenter.Login(authenticator);
            
        }
        private async void GoogleAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthenticatorCompleted;
                authenticator.Error -= GoogleAuthenticatorError;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    Application.Current.MainPage = new SelectionPage(e.Account.Properties["access_token"], e.Account.Properties["refresh_token"],e,null,"GOOGLE");
                    //GoogleUserProfileAsync(e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], e);
                }
                catch(Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
                
            }
            else
            {
                Application.Current.MainPage = new LogInPage();
                await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
            }
        }

        public async void GoogleUserProfileAsync(string accessToken, string refreshToken, AuthenticatorCompletedEventArgs e)
        {
            try
            {
                var progress = UserDialogs.Instance.Loading("Sending your request...");
                
                var client = new HttpClient();
                var socialLogInPost = new SocialLogInPost();

                var request = new OAuth2Request("GET", new Uri(Constant.GoogleUserInfoUrl), null, e.Account);
                var GoogleResponse = await request.GetResponseAsync();
                var userData = GoogleResponse.GetResponseText();

                System.Diagnostics.Debug.WriteLine(userData);
                GoogleResponse googleData = JsonConvert.DeserializeObject<GoogleResponse>(userData);

                socialLogInPost.email = googleData.email;
                socialLogInPost.password = "";
                socialLogInPost.social_id = googleData.id;
                socialLogInPost.signup_platform = "GOOGLE";

                var socialLogInPostSerialized = JsonConvert.SerializeObject(socialLogInPost);
                var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine(socialLogInPostSerialized);

                var RDSResponse = await client.PostAsync(Constant.LogInUrl, postContent);
                var responseContent = await RDSResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine(responseContent);
                System.Diagnostics.Debug.WriteLine(RDSResponse.IsSuccessStatusCode);

                if (RDSResponse.IsSuccessStatusCode)
                {
                    if (responseContent != null)
                    {
                        if (responseContent.Contains(Constant.EmailNotFound))
                        {
                            var signUp = await DisplayAlert("Message", "It looks like you don't have a Serving Fresh account. Please sign up!", "OK", "Cancel");
                            if (signUp)
                            {
                                Application.Current.MainPage = new SocialSignUp(googleData.id, googleData.given_name, googleData.family_name, googleData.email, accessToken, refreshToken, "GOOGLE");
                            }
                        }
                        if (responseContent.Contains(Constant.AutheticatedSuccesful))
                        {
                            try
                            {


                                var data = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                                Application.Current.Properties["user_id"] = data.result[0].customer_uid;

                                UpdateTokensPost updateTokesPost = new UpdateTokensPost();
                                updateTokesPost.uid = data.result[0].customer_uid;
                                updateTokesPost.mobile_access_token = accessToken;
                                updateTokesPost.mobile_refresh_token = refreshToken;

                                var updateTokesPostSerializedObject = JsonConvert.SerializeObject(updateTokesPost);
                                var updateTokesContent = new StringContent(updateTokesPostSerializedObject, Encoding.UTF8, "application/json");
                                var updateTokesResponse = await client.PostAsync(Constant.UpdateTokensUrl, updateTokesContent);
                                var updateTokenResponseContent = await updateTokesResponse.Content.ReadAsStringAsync();
                                System.Diagnostics.Debug.WriteLine(updateTokenResponseContent);

                                if (updateTokesResponse.IsSuccessStatusCode)
                                {
                                    var GoogleRequest = new RequestUserInfo();
                                    GoogleRequest.uid = data.result[0].customer_uid;

                                    var requestSelializedObject = JsonConvert.SerializeObject(GoogleRequest);
                                    var requestContent = new StringContent(requestSelializedObject, Encoding.UTF8, "application/json");

                                    var clientRequest = await client.PostAsync(Constant.GetUserInfoUrl, requestContent);

                                    if (clientRequest.IsSuccessStatusCode)
                                    {
                                        var SFUser = await clientRequest.Content.ReadAsStringAsync();
                                        var GoogleUserData = JsonConvert.DeserializeObject<UserInfo>(SFUser);

                                        DateTime today = DateTime.Now;
                                        DateTime expDate = today.AddDays(Constant.days);

                                        Application.Current.Properties["user_id"] = data.result[0].customer_uid;
                                        Application.Current.Properties["time_stamp"] = expDate;
                                        Application.Current.Properties["platform"] = "GOOGLE";
                                        Application.Current.Properties["user_email"] = GoogleUserData.result[0].customer_email;
                                        Application.Current.Properties["user_first_name"] = GoogleUserData.result[0].customer_first_name;
                                        Application.Current.Properties["user_last_name"] = GoogleUserData.result[0].customer_last_name;
                                        Application.Current.Properties["user_phone_num"] = GoogleUserData.result[0].customer_phone_num;
                                        Application.Current.Properties["user_address"] = GoogleUserData.result[0].customer_address;
                                        Application.Current.Properties["user_unit"] = GoogleUserData.result[0].customer_unit;
                                        Application.Current.Properties["user_city"] = GoogleUserData.result[0].customer_city;
                                        Application.Current.Properties["user_state"] = GoogleUserData.result[0].customer_state;
                                        Application.Current.Properties["user_zip_code"] = GoogleUserData.result[0].customer_zip;
                                        Application.Current.Properties["user_latitude"] = GoogleUserData.result[0].customer_lat;
                                        Application.Current.Properties["user_longitude"] = GoogleUserData.result[0].customer_long;

                                        _ = Application.Current.SavePropertiesAsync();

                                        if (Device.RuntimePlatform == Device.iOS)
                                        {
                                            deviceId = Preferences.Get("guid", null);
                                            if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
                                        }
                                        else
                                        {
                                            deviceId = Preferences.Get("guid", null);
                                            if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
                                        }

                                        if (deviceId != null)
                                        {
                                            NotificationPost notificationPost = new NotificationPost();

                                            notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                            notificationPost.guid = deviceId.Substring(5);
                                            Application.Current.Properties["guid"] = deviceId.Substring(5);
                                            notificationPost.notification = "TRUE";

                                            var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                            Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                            var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                            var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                            Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);

                                            if (clientResponse.IsSuccessStatusCode)
                                            {
                                                System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                            }
                                            else
                                            {
                                                await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                            }
                                        }

                                        Application.Current.MainPage = new SelectionPage();
                                    }
                                    else
                                    {
                                        await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "We are facing some problems with our internal system. We weren't able to update your credentials", "OK");
                                }
                            }
                            catch (Exception second)
                            {
                                Debug.WriteLine(second.Message);
                            }
                        }
                        if (responseContent.Contains(Constant.ErrorPlatform))
                        {
                            var RDSCode = JsonConvert.DeserializeObject<RDSLogInMessage>(responseContent);
                            await DisplayAlert("Message", RDSCode.message, "OK");
                        }

                        if (responseContent.Contains(Constant.ErrorUserDirectLogIn))
                        {
                            await DisplayAlert("Oops!", "You have an existing Serving Fresh account. Please use direct login", "OK");
                        }
                    }
                }
                progress.Hide();
            }
            catch (Exception first)
            {
                Debug.WriteLine(first.Message);
            }
        }

        private async void GoogleAuthenticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthenticatorCompleted;
                authenticator.Error -= GoogleAuthenticatorError;
            }
            Application.Current.MainPage = new LogInPage();
            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }

        public void AppleLogInClick(System.Object sender, System.EventArgs e)
        {
            SignIn?.Invoke(sender, e);
            var c = (ImageButton)sender;
            c.Command?.Execute(c.CommandParameter);
        }

        public void InvokeSignInEvent(object sender, EventArgs e)
            => SignIn?.Invoke(sender, e);

        private async void AppleError(object sender, EventArgs e)
        {
            await DisplayAlert("Error", "We weren't able to set an account for you", "OK");
        }

        async void ProceedAsGuestClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.Properties["user_latitude"] = "0";
            Application.Current.Properties["user_longitude"] = "0";

            List<string> types = new List<string>();
            List<string> businessId = new List<string>();

            types.Add("fruit");
            types.Add("vegetable");
            types.Add("dessert");
            types.Add("other");

            businessId.Add("200-000016");
            businessId.Add("200-000017");
            businessId.Add("200-000018");
            businessId.Add("200-000019");

            var weekDay = DateTime.Now.DayOfWeek.ToString();
            Application.Current.Properties["guest"] = true;
            
            
            GuestItemsPage businessItemPage = new GuestItemsPage(types, businessId, weekDay);
            Application.Current.MainPage = businessItemPage;
            //Application.Current.MainPage = new GuestPage();

            //await DisplayAlert("", "Additional feature coming soon", "Thanks");
        }

        void SignUpClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SignUpPage();
        }

        void ReturnBackToPrincipalPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new PrincipalPage();
        }
    }
}
