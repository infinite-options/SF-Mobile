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

namespace ServingFresh.Views
{
    public partial class LogInPage : ContentPage
    {
        public event EventHandler SignIn;                                                                   // Variable of EventHandler used for Apple
        public bool createAccount = false;                                                                  // Initalized boolean value called createAccount

        public LogInPage()                                                                                  // This is the class Constructor
        {
            InitializeComponent();                                                                          // This is a Xamarin default
            InitializeAppProperties();                                                                      // This refers to class below

            if (Device.RuntimePlatform == Device.Android)
            {
                appleLogInButton.IsEnabled = false;
            }
            else
            {
                InitializedAppleLogin();                                                                    // Turns on Apple Login for Apple devices
            }
        }

        public void InitializeAppProperties()                                                               // Initializes most (not all) Application.Current.Properties
        {                                                                                                   // You can create additional parameters on the fly
            Application.Current.Properties["user_email"] = "";                                              // If you don't initialize it you can get a Null Reference
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

        public void InitializedAppleLogin()                                                                 // Explain this format
        {
            var vm = new LoginViewModel();
            vm.AppleError += AppleError;                                                                    // Assigns handler class from LoginViewModel.cs <== Need further review += creates a button handler like OnClick in xaml
            BindingContext = vm;
        }

        private async void DirectLogInClick(System.Object sender, System.EventArgs e)                       // Clicked from LogInPage.xaml
        {
            logInButton.IsEnabled = false;                                                                  // Login button is disabled after clicked.  You don't want button constantly enabled
            if (String.IsNullOrEmpty(userEmailAddress.Text) || String.IsNullOrEmpty(userPassword.Text))     // This is how you get input from a Mobile App.  Linked to xaml file.
            { 
                await DisplayAlert("Error", "Please fill in all fields", "OK");                             
                logInButton.IsEnabled = true;
                                                                                                            // code to set for use outside of LogInPage:  Application.Current.Properties["user_email"] = userEmailAddress.Text;
            }
            else
            {
                var accountSalt = await RetrieveAccountSalt(userEmailAddress.Text.ToLower().Trim());        // This refers to class below and should return Salt Key
                System.Diagnostics.Debug.WriteLine("accountSalt :" + accountSalt);

                if (accountSalt != null)
                {
                    var loginAttempt = await LogInUser(userEmailAddress.Text.ToLower().Trim(), userPassword.Text, accountSalt);     // Calls LogInUser function below. Convert email to lowercase, 
                    System.Diagnostics.Debug.WriteLine("loginAttempt: " + loginAttempt);

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


        private async Task<AccountSalt> RetrieveAccountSalt(string userEmail)
        {
            try
            {
                // Take in email
                System.Diagnostics.Debug.WriteLine("In LogInPage.xaml.cs > RetrieveAccountSalt function");
                System.Diagnostics.Debug.WriteLine("user_email: " + userEmail);
                // System.Diagnostics.Debug.WriteLine(userEmail);

                // Store email in saltPost                                                                              // This is a Login Class in Login > Classes
                SaltPost saltPost = new SaltPost();                                                                     // Creates a new object saltPost of class SaltPost.  In English it would read:  "of type SaltPost, create a new object called saltPost and set equal to an object of that type"
                saltPost.email = userEmail;                                                                             // Sets value of saltPost.email = userEmail  
                System.Diagnostics.Debug.WriteLine("saltPost_email: " + saltPost.email);                                // Now we can call a property of that class using dot notation

                // Create JSON object to send in endpoint
                var saltPostSerilizedObject = JsonConvert.SerializeObject(saltPost);                                    // JSONifies everything is saltPost (Serialize)
                var saltPostContent = new StringContent(saltPostSerilizedObject, Encoding.UTF8, "application/json");    // converts JSON object into a string (Stringify)
                System.Diagnostics.Debug.WriteLine("JSON object: " + saltPostSerilizedObject);
                // System.Diagnostics.Debug.WriteLine(saltPostSerilizedObject);
                System.Diagnostics.Debug.WriteLine("JSON string: " + saltPostContent);                                  // Encodes JSON object.  Only prints: System.Net.Http.StringContent

                // Setup, call and receive Endpoint message
                var client = new HttpClient();                                                                          // Endpoint call is httpClient with URL and stringified JSON Object                        
                var DRSResponse = await client.PostAsync(Constant.AccountSaltUrl, saltPostContent);                     // Calls endpoint with JSON object.
                var DRSMessage = await DRSResponse.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine("DRSResponse :" + DRSResponse);                                      // Response is a non-useful JSON object
                System.Diagnostics.Debug.WriteLine("DRSMessage:" + DRSMessage);                                         // Message is backend response with Success/Fail codes & contains Salt info
                //System.Diagnostics.Debug.WriteLine(DRSMessage);   


                // Route based on Endpoint message
                AccountSalt userInformation = null;                                                                     // Initializes AccountSalt object.  All properties set to null
                                                                                                                        // TRY PRINTING userInformation.password_algorithm
                if (DRSResponse.IsSuccessStatusCode)
                {
                    var result = await DRSResponse.Content.ReadAsStringAsync();                                         // result is the same as DRSMessage
                                                                                                                        // This is a Login Class in Login > Classes                                                  
                    AcountSaltCredentials data = new AcountSaltCredentials();                                           // Creates a new object data of class AcountSaltCredentials
                    data = JsonConvert.DeserializeObject<AcountSaltCredentials>(result);                                // Deserialize JSON object.  Definition is given by Class Definition. data contains JSON object
                    System.Diagnostics.Debug.WriteLine("data: " + data);                                                

                    // If accidentally using Direct Login for Social Media Login
                    if (DRSMessage.Contains(Constant.UseSocialMediaLogin))                                              // code 401
                    {
                        createAccount = true;
                        System.Diagnostics.Debug.WriteLine("Error:" + Constant.UseSocialMediaLogin);
                        System.Diagnostics.Debug.WriteLine("DRSMessage:" + DRSMessage);
                        // System.Diagnostics.Debug.WriteLine(DRSMessage);
                        await DisplayAlert("Oops!", data.message, "OK");
                    }

                    // If accidentally using and email that is not registered
                    else if (DRSMessage.Contains(Constant.EmailNotFound))                                               // code 404
                    {
                        await DisplayAlert("Oops!", "Our records show that you don't have an account. Please sign up!", "OK");
                    }

                    // Finally returning Account Salt info
                    else
                    {
                        userInformation = new AccountSalt                                                               // userInformation is the variable ultimately returned
                        {
                            password_algorithm = data.result[0].password_algorithm,
                            password_salt = data.result[0].password_salt
                        };
                    }
                }

                System.Diagnostics.Debug.WriteLine("result :" + userInformation);                                       // Print result before returning from RetrieveAccountSalt function
                return userInformation;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);                                                         // Is there a way to test this
                return null;
            }
        }

        public async Task<LogInResponse> LogInUser(string userEmail, string userPassword, AccountSalt accountSalt)
        {
            try
            {
                SHA512 sHA512 = new SHA512Managed();                                                                    // Calls Xamarin Class.  Why SHA512 and SHA512Managed?
                var client = new HttpClient();
                byte[] data = sHA512.ComputeHash(Encoding.UTF8.GetBytes(userPassword + accountSalt.password_salt));     // Calls actual hashing.  What does accountSalt.password_salt mean?
                string hashedPassword = BitConverter.ToString(data).Replace("-", string.Empty).ToLower();               // Stores hashedPassword

                LogInPost loginPostContent = new LogInPost();                                                           // Calls LogInPost Class and sets variables in loginPostContent
                loginPostContent.email = userEmail;
                loginPostContent.password = hashedPassword;
                loginPostContent.social_id = "";
                loginPostContent.signup_platform = "";

                string loginPostContentJson = JsonConvert.SerializeObject(loginPostContent);                            // Serialize

                var httpContent = new StringContent(loginPostContentJson, Encoding.UTF8, "application/json");           // Stringify
                var response = await client.PostAsync(Constant.LogInUrl, httpContent);                                  // Post Response
                var message = await response.Content.ReadAsStringAsync();                                               // Post Message
                System.Diagnostics.Debug.WriteLine("message:" + message);

                if (message.Contains(Constant.AutheticatedSuccesful))                                                   // code 200
                {

                    var responseContent = await response.Content.ReadAsStringAsync();                                   // Stores message in responseContent
                    var loginResponse = JsonConvert.DeserializeObject<LogInResponse>(responseContent);                  // JSONifies loginResponse (or message)
                    return loginResponse;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception message: " + e.Message);                                  // Is there a way to test this?
                return null;
            }
        }


        // Facebook
        // comes from LoginPage.xaml or App.xaml.cs
        public void FacebookLogInClick(System.Object sender, System.EventArgs e)                                        // Linked to Clicked="FacebookLogInClick" from LogInPage.xaml
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

            var authenticator = new OAuth2Authenticator(clientID, Constant.FacebookScope, new Uri(Constant.FacebookAuthorizeUrl), new Uri(redirectURL), null, false);  // Initializes variable authenticator
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            // Is this like clicking a button?  ie I just clicked FacebookAuthenticatorCompleted and then that function ran?
            authenticator.Completed += FacebookAuthenticatorCompleted;                                                  // += Creates a button handler (like OnClick in xaml).  Assignment to submit button on the Facebook page
            authenticator.Error += FacebookAutheticatorError;                                                           // Assignment to Cancel button on the Facebook page
            
            presenter.Login(authenticator);                                                                             // Calls Facebook and invokes Facebook UI.  Authenticator contains app_uid,etc
        }


        // Verifies Facebook Authenticated
        public void FacebookAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)                    // Called when Facebook submitt is clicked.  Facebook send in "sender" (event handler calls) and "e" contains user parameters
        {
            var authenticator = sender as OAuth2Authenticator;                                                          // Casting sender an an OAuth2Authenticator type

            if (authenticator != null)
            {                   
                authenticator.Completed -= FacebookAuthenticatorCompleted;                                              
                authenticator.Error -= FacebookAutheticatorError;                                                       
            }

            if (e.IsAuthenticated)                                                                                      // How does this statement work?
            {
                FacebookUserProfileAsync(e.Account.Properties["access_token"]);
            }
        }


        // Checks if they have an accoutn with us
        public async void FacebookUserProfileAsync(string accessToken)
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
        }


        // Closes button handlers if they click Cancel in Facebook
        private async void FacebookAutheticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }

            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }





        // Google
        // comes from LoginPage.xaml or App.xaml.cs
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
                GoogleUserProfileAsync(e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], e);
            }
            else
            {
                await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
            }
        }

        public async void GoogleUserProfileAsync(string accessToken, string refreshToken, AuthenticatorCompletedEventArgs e)
        {
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
        }

        private async void GoogleAuthenticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthenticatorCompleted;
                authenticator.Error -= GoogleAuthenticatorError;
            }

            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }



        // Apple
        // comes from LoginPage.xaml or App.xaml.cs
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

        void ProceedAsGuestClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new GuestPage();
        }

        void SignUpClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SignUpPage();
        }
    }
}
