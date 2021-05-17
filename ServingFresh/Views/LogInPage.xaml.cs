﻿using System;
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
using static ServingFresh.Views.PrincipalPage;

namespace ServingFresh.Views
{
    public partial class LogInPage : ContentPage
    {
        public string direction = "";
        public Page realod = null;
        public event EventHandler SignIn;
        public bool createAccount = false;
        INotifications appleNotification = DependencyService.Get<INotifications>();
        private string deviceId;

        public LogInPage()
        {
            //grids.BackgroundColor = Color.FromHex("AB000000");
            InitializeComponent();
            //grids.BackgroundColor = Color.FromHex("AB000000");
            BackgroundColor = Color.FromHex("AB000000");
            if (Device.RuntimePlatform == Device.Android)
            {
                System.Diagnostics.Debug.WriteLine("Running on Android: Line 32");
                Console.WriteLine("guid: " + Preferences.Get("guid", null));
                appleLogInButton.IsEnabled = false;
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

        public LogInPage(double height, string direction)
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("AB000000");
            logInFrame.Margin = new Thickness(0, height, 0, 0);
            this.direction = direction;
            //realod = toload;

            if (Device.RuntimePlatform == Device.Android)
            {
                System.Diagnostics.Debug.WriteLine("Running on Android: Line 32");
                Console.WriteLine("guid: " + Preferences.Get("guid", null));
                appleLogInButton.IsEnabled = false;
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


        public async void MessageFromSelectionPage(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }


        public async void AddMessage(string body)
        {
            await DisplayAlert("Aler!", body, "OK");
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

                                user.setUserID(userData.result[0].customer_uid);
                                user.setUserSessionTime(expDate);
                                user.setUserPlatform("DIRECT");
                                user.setUserType("CUSTOMER");
                                user.setUserEmail(userData.result[0].customer_email);
                                user.setUserFirstName(userData.result[0].customer_first_name);
                                user.setUserLastName(userData.result[0].customer_last_name);
                                user.setUserPhoneNumber(userData.result[0].customer_phone_num);
                                user.setUserAddress(userData.result[0].customer_address);
                                user.setUserUnit(userData.result[0].customer_unit);
                                user.setUserCity(userData.result[0].customer_city);
                                user.setUserState(userData.result[0].customer_state);
                                user.setUserZipcode(userData.result[0].customer_zip);
                                user.setUserLatitude(userData.result[0].customer_lat);
                                user.setUserLongitude(userData.result[0].customer_long);



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

                                    notificationPost.uid = user.getUserID();
                                    notificationPost.guid = deviceId.Substring(5);
                                    user.setUserDeviceID(deviceId.Substring(5));
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
                                if(direction == "")
                                {
                                    Application.Current.MainPage = new SelectionPage();
                                }
                                else
                                {
                                    await Application.Current.MainPage.Navigation.PopModalAsync();
                                }

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
            Application.Current.MainPage.Navigation.PopModalAsync();
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
                   // Application.Current.MainPage = new SelectionPage(e.Account.Properties["access_token"], "", null, null, "FACEBOOK");

                    var client = new SignIn();
                    UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                    var status = await client.VerifyUserCredentials(e.Account.Properties["access_token"], "", null, null, "FACEBOOK");
                    RedirectUserBasedOnVerification(status, direction);
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
            else
            {
               // Application.Current.MainPage = new LogInPage();
               //await DisplayAlert("Error", "Facebook was not able to autheticate your account", "OK");
            }
        }


        public async void AppleLogIn(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleAccount = null, AppleAccount appleCredentials = null, string platform = "")
        {
            var client = new SignIn();
            UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
            var status = await client.VerifyUserCredentials(accessToken, refreshToken, googleAccount, appleCredentials, platform);
            RedirectUserBasedOnVerification(status, direction);
        }

        public async void RedirectUserBasedOnVerification(string status, string direction)
        {
            if (status == "LOGIN USER")
            {
                UserDialogs.Instance.HideLoading();
                await Application.Current.MainPage.DisplayAlert("Great!", "You have successfully loged in!", "OK");

                Debug.WriteLine("DIRECTION VALUE: " + direction);
                if (direction == "")
                {
                    Application.Current.MainPage = new SelectionPage();
                }
                else
                {
                    Dictionary<string, Page> array = new Dictionary<string, Page>();
                    array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                    array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                    array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                    array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                    array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                    array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                    array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                    var root = Application.Current.MainPage;
                    Debug.WriteLine("ROOT VALUE: " + root);
                    Application.Current.MainPage = array[root.ToString()];
                }
                
            }
            else if (status == "USER NEEDS TO SIGN UP")
            {
                UserDialogs.Instance.HideLoading();
                await Application.Current.MainPage.DisplayAlert("Oops", "It looks like you don't have an account with Serving Fresh. Please sign up!", "OK");
                await Application.Current.MainPage.Navigation.PushModalAsync(new AddressPage(), true);
            }
            else if (status == "WRONG SOCIAL MEDIA TO SIGN IN")
            {
                UserDialogs.Instance.HideLoading();
                await Application.Current.MainPage.DisplayAlert("Oops", "Our records show that you have attempted to log in with a different social media account. Please log in through the correct social media platform. Thanks!", "OK");
            }
            else if (status == "SIGN IN DIRECTLY")
            {
                UserDialogs.Instance.HideLoading();
                await Application.Current.MainPage.DisplayAlert("Oops", "Our records show that you have attempted to log in with a social media account. Please log in through our direct log in. Thanks!", "OK");
            }
            else if (status == "ERROR1")
            {
                UserDialogs.Instance.HideLoading();
                await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
            }
            else if (status == "ERROR2")
            {
                UserDialogs.Instance.HideLoading();
                await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
            }
            else
            {
                UserDialogs.Instance.HideLoading();
                await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
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
            Application.Current.MainPage.Navigation.PopModalAsync();
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
                    var client = new SignIn();
                    UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                    var status = await client.VerifyUserCredentials(e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], e, null, "GOOGLE");
                    RedirectUserBasedOnVerification(status, direction);
                }
                catch(Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
                
            }
            else
            {
                //Application.Current.MainPage = new LogInPage();
                //await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
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
            if (Device.RuntimePlatform != Device.Android)
            {
                OnAppleSignInRequest();
            }
        }

        public void InvokeSignInEvent(object sender, EventArgs e)
            => SignIn?.Invoke(sender, e);

        private async void AppleError(object sender, EventArgs e)
        {
            await DisplayAlert("Error", "We weren't able to set an account for you", "OK");
        }


        public async void OnAppleSignInRequest()
        {
            try
            {
                IAppleSignInService appleSignInService = DependencyService.Get<IAppleSignInService>();
                var account = await appleSignInService.SignInAsync();
                if (account != null)
                {
                    Preferences.Set(App.LoggedInKey, true);
                    await SecureStorage.SetAsync(App.AppleUserIdKey, account.UserId);

                    if (account.Token == null) { account.Token = ""; }
                    if (account.Email != null)
                    {
                        if (Application.Current.Properties.ContainsKey(account.UserId.ToString()))
                        {
                            //Application.Current.Properties[account.UserId.ToString()] = account.Email;
                            Debug.WriteLine((string)Application.Current.Properties[account.UserId.ToString()]);
                        }
                        else
                        {
                            Application.Current.Properties[account.UserId.ToString()] = account.Email;
                        }
                    }
                    if (account.Email == null) { account.Email = ""; }
                    if (account.Name == null) { account.Name = ""; }

                    if (Application.Current.Properties.ContainsKey(account.UserId.ToString()))
                    {
                        account.Email = (string)Application.Current.Properties[account.UserId.ToString()];
                        //Application.Current.MainPage = new SelectionPage("", "", null, account, "APPLE");
                        //var root = (LogInPage)Application.Current.MainPage;
                        //root.AppleLogIn("", "", null, account, "APPLE");

                        var client = new SignIn();
                        UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                        var status = await client.VerifyUserCredentials("", "", null, account, "APPLE");
                        RedirectUserBasedOnVerification(status, direction);
                        //AppleUserProfileAsync(account.UserId, account.Token, (string)Application.Current.Properties[account.UserId.ToString()], account.Name);
                    }
                    else
                    {
                        var client = new HttpClient();
                        var getAppleEmail = new AppleEmail();
                        getAppleEmail.social_id = account.UserId;

                        var socialLogInPostSerialized = JsonConvert.SerializeObject(getAppleEmail);

                        System.Diagnostics.Debug.WriteLine(socialLogInPostSerialized);

                        var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");
                        var RDSResponse = await client.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/AppleEmail", postContent);
                        var responseContent = await RDSResponse.Content.ReadAsStringAsync();

                        System.Diagnostics.Debug.WriteLine(responseContent);
                        if (RDSResponse.IsSuccessStatusCode)
                        {
                            var data = JsonConvert.DeserializeObject<AppleUser>(responseContent);
                            Application.Current.Properties[account.UserId.ToString()] = data.result[0].customer_email;
                            account.Email = (string)Application.Current.Properties[account.UserId.ToString()];
                            //var root = (LogInPage)Application.Current.MainPage;
                            //root.AppleLogIn("", "", null, account, "APPLE");
                            //Application.Current.MainPage = new SelectionPage("", "", null, account, "APPLE");
                            //AppleUserProfileAsync(account.UserId, account.Token, (string)Application.Current.Properties[account.UserId.ToString()], account.Name);
                            var client1 = new SignIn();
                            UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                            var status = await client1.VerifyUserCredentials("", "", null, account, "APPLE");
                            RedirectUserBasedOnVerification(status, direction);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Ooops", "Our system is not working. We can't process your request at this moment", "OK");
                        }
                    }
                }
                else
                {
                    //AppleError?.Invoke(this, default(EventArgs));

                }
            }
            catch (Exception apple)
            {
                await Application.Current.MainPage.DisplayAlert("Error", apple.Message, "OK");
            }
        }

        void ReturnBackToPrincipalPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new PrincipalPage();
        }

        void ShowHidePassword(System.Object sender, System.EventArgs e)
        {
            Label label = (Label)sender;
            if (label.Text == "Show password")
            {
                userPassword.IsPassword = false;
                label.Text = "Hide password";
            }
            else
            {
                userPassword.IsPassword = true;
                label.Text = "Show password";
            }
        }

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }

        void ResetPassword(System.Object sender, System.EventArgs e)
        {
            UpdatePassword(sender, e);
        }

        async void UpdatePassword(System.Object sender, System.EventArgs e)
        {
            if (!String.IsNullOrEmpty(userEmailAddress.Text))
            {
                var client = new SignIn();
                var password = new ResetPassword();
                password.email = userEmailAddress.Text;

                var result = await client.ResetPassword(password);
                if(result != "")
                {
                    if(result.Contains("\"Need to do login via social\""))
                    {
                        await DisplayAlert("Oops", "We can't reset the password for the given email since it is associated with a media social log-in. Please log in via social media.", "");
                    }
                    else if (result.Contains("\"message\": \"A temporary password has been sent\""))
                    {
                        await DisplayAlert("Great!", "We have reset your password. A temporary password has been sent to your email.", "OK");
                    }
                    else if (result.Contains("\"message\": \"No such email exists\""))
                    {
                        await DisplayAlert("Oops", "Our records show that this email is invalid", "");
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "We were not able to fulfill your request. Please try again.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Oops","Plese enter the email address associated with your Serving Fresh account","OK");
            }
        }
    }
}
