using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServingFresh.Config;
using ServingFresh.LogIn.Classes;
using ServingFresh.Views;
using Xamarin.Auth;
using Xamarin.Forms;

namespace ServingFresh.Models
{
    public class SignIn
    {
        private string accessToken;
        private string refreshToken;
        private string platform;
        private AuthenticatorCompletedEventArgs googleAccount;

        public SignIn()
        {
            googleAccount = null;
            platform = "";
            accessToken = "";
            refreshToken = "";
        }

        public async Task<User> SignInDirectUser(Button button, Entry email, Entry password)
        {
            User directUser = null;
            button.IsEnabled = false;
            if (String.IsNullOrEmpty(email.Text) || String.IsNullOrEmpty(password.Text))
            {
                //await DisplayAlert("Error", "Please fill in all fields", "OK");
                button.IsEnabled = true;
            }
            else
            {
                var accountSalt = await RetrieveAccountSalt(email.Text.ToLower().Trim());

                if (accountSalt != null)
                {
                    var loginAttempt = await LogInUser(email.Text.ToLower().Trim(), password.Text, accountSalt);

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

                                Debug.WriteLine("DIRECT LOGIN ENDPOINT CONTENT: " + SFUser);

                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                directUser = new User();

                                directUser.setUserID(userData.result[0].customer_uid);
                                directUser.setUserSessionTime(expDate);
                                directUser.setUserPlatform("DIRECT");
                                directUser.setUserType("CUSTOMER");
                                directUser.setUserEmail(userData.result[0].customer_email);
                                directUser.setUserFirstName(userData.result[0].customer_first_name);
                                directUser.setUserLastName(userData.result[0].customer_last_name);
                                directUser.setUserPhoneNumber(userData.result[0].customer_phone_num);
                                directUser.setUserAddress(userData.result[0].customer_address);
                                directUser.setUserUnit(userData.result[0].customer_unit);
                                directUser.setUserCity(userData.result[0].customer_city);
                                directUser.setUserState(userData.result[0].customer_state);
                                directUser.setUserZipcode(userData.result[0].customer_zip);
                                directUser.setUserLatitude(userData.result[0].customer_lat);
                                directUser.setUserLongitude(userData.result[0].customer_long);

                                //if (Device.RuntimePlatform == Device.iOS)
                                //{
                                //    deviceId = Preferences.Get("guid", null);
                                //    if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
                                //}
                                //else
                                //{
                                //    deviceId = Preferences.Get("guid", null);
                                //    if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
                                //}

                                //if (deviceId != null)
                                //{
                                //    NotificationPost notificationPost = new NotificationPost();

                                //    notificationPost.uid = user.getUserID();
                                //    notificationPost.guid = deviceId.Substring(5);
                                //    user.setUserDeviceID(deviceId.Substring(5));
                                //    notificationPost.notification = "TRUE";

                                //    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                //    Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                //    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                //    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                //    Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);

                                //    if (clientResponse.IsSuccessStatusCode)
                                //    {
                                //        System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                //    }
                                //    else
                                //    {
                                //        await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                //    }
                                //}
                                //Application.Current.MainPage = new SelectionPage();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            //await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                        }
                    }
                    else
                    {
                        //await DisplayAlert("Error", "Wrong password was entered", "OK");
                        button.IsEnabled = true;
                    }
                }
                button.IsEnabled = true;
            }
            return directUser;
        }

        //private string platform;
        //private AuthenticatorCompletedEventArgs googleAccount;
        public void setPlatform(string platform)
        {
            this.platform = platform;
        }

        public void setGoogleAccount(AuthenticatorCompletedEventArgs googleAccount)
        {
            this.googleAccount = googleAccount;
        }

        public void setAccessToken(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public void setRefreshToken(string refreshToken)
        {
            this.refreshToken = refreshToken;
        }

        public AuthenticatorCompletedEventArgs getGoogleAccount()
        {
            return googleAccount;
        }

        public string getPlatfomr()
        {
            return platform;
        }

        public string getAccessToken()
        {
            return accessToken;
        }

        public string getRefreshToken()
        {
            return refreshToken;
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
                        //createAccount = true;
                        Debug.WriteLine(DRSMessage);
                        //await DisplayAlert("Oops!", data.message, "OK");
                    }
                    else if (DRSMessage.Contains(Constant.EmailNotFound))
                    {
                        //await DisplayAlert("Oops!", "Our records show that you don't have an accout. Please sign up!", "OK");
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

        private async Task<LogInResponse> LogInUser(string userEmail, string password, AccountSalt accountSalt)
        {
            try
            {
                SHA512 sHA512 = new SHA512Managed();
                var client = new HttpClient();
                byte[] data = sHA512.ComputeHash(Encoding.UTF8.GetBytes(password + accountSalt.password_salt));
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

        public void SignInWithFacebook()
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

        public void FacebookAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
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
                Application.Current.MainPage = new PrincipalPage();
                //await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
            }
        }


        private void FacebookAutheticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }
            Application.Current.MainPage = new PrincipalPage();
            //await DisplayAlert("Authentication error: ", e.Message, "OK");
        }


        public void SignInWithGoogle()
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
        private void GoogleAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
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
                    Application.Current.MainPage = new SelectionPage(e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], e, null, "GOOGLE");
                    //GoogleUserProfileAsync(e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], e);
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }

            }
            else
            {
                Application.Current.MainPage = new PrincipalPage();
                //await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
            }
        }



        private void GoogleAuthenticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthenticatorCompleted;
                authenticator.Error -= GoogleAuthenticatorError;
            }
            Application.Current.MainPage = new PrincipalPage();
            //await DisplayAlert("Authentication error: ", e.Message, "OK");
        }

        //public void AppleLogInClick(System.Object sender, System.EventArgs e)
        //{
        //    SignIn?.Invoke(sender, e);
        //    var c = (ImageButton)sender;
        //    c.Command?.Execute(c.CommandParameter);
        //}

        //public void InvokeSignInEvent(object sender, EventArgs e)
        //    => SignIn?.Invoke(sender, e);
    }
}
