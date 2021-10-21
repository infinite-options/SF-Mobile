using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using ServingFresh.Config;
using ServingFresh.LogIn.Apple;
using ServingFresh.LogIn.Classes;
using ServingFresh.Views;
using Xamarin.Auth;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ServingFresh.Views.PrincipalPage;
namespace ServingFresh.Models
{

    public class SignIn
    {
        private string deviceId = "";
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
                                SaveUser(directUser);

                                deviceId = Preferences.Get("guid", null);

                                if (deviceId != null)
                                {
                                    NotificationPost notificationPost = new NotificationPost();

                                    notificationPost.uid = user.getUserID();
                                    notificationPost.guid = Preferences.Get("guid", null).Substring(5);
                                    user.setUserDeviceID(deviceId.Substring(5));
                                    notificationPost.notification = "TRUE";

                                    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                    Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                    Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);

                                    if (clientResponse.IsSuccessStatusCode)
                                    {
                                        Debug.WriteLine("We have post the guid to the database");
                                    }
                                }
                            }
                            catch (Exception errorSignInDirectUser)
                            {
                                var client1 = new Diagnostic();
                                client1.parseException(errorSignInDirectUser.ToString(), user);
                            }
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        button.IsEnabled = true;
                    }
                }
                button.IsEnabled = true;
            }
            return directUser;
        }

        public async Task<string> VerifyUserCredentials(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleAccount = null, AppleAccount appleCredentials = null, string platform = "")
        {
            var isUserVerified = "";
            try
            {
                var client = new HttpClient();
                var socialLogInPost = new SocialLogInPost();

                var googleData = new GoogleResponse();
                var facebookData = new FacebookResponse();

                if (platform == "GOOGLE")
                {
                    var request = new OAuth2Request("GET", new Uri(Constant.GoogleUserInfoUrl), null, googleAccount.Account);
                    var GoogleResponse = await request.GetResponseAsync();
                    var googelUserData = GoogleResponse.GetResponseText();

                    googleData = JsonConvert.DeserializeObject<GoogleResponse>(googelUserData);

                    socialLogInPost.email = googleData.email;
                    socialLogInPost.social_id = googleData.id;
                    Debug.WriteLine("IMAGE: " + googleData.picture);
                    user.setUserImage(googleData.picture);
                }
                else if (platform == "FACEBOOK")
                {
                    var facebookResponse = client.GetStringAsync(Constant.FacebookUserInfoUrl + accessToken);
                    var facebookUserData = facebookResponse.Result;

                    Debug.WriteLine("FACEBOOK DATA: " + facebookUserData);
                    facebookData = JsonConvert.DeserializeObject<FacebookResponse>(facebookUserData);

                    socialLogInPost.email = facebookData.email;
                    socialLogInPost.social_id = facebookData.id;
                }
                else if (platform == "APPLE")
                {
                    socialLogInPost.email = appleCredentials.Email;
                    socialLogInPost.social_id = appleCredentials.UserId;
                }

                socialLogInPost.password = "";
                socialLogInPost.signup_platform = platform;

                var socialLogInPostSerialized = JsonConvert.SerializeObject(socialLogInPost);
                var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");

                var RDSResponse = await client.PostAsync(Constant.LogInUrl, postContent);
                var responseContent = await RDSResponse.Content.ReadAsStringAsync();
                var authetication = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                if (RDSResponse.IsSuccessStatusCode)
                {
                    if (responseContent != null)
                    {
                        if (authetication.code.ToString() == Constant.EmailNotFound)
                        {
                            isUserVerified = "USER NEEDS TO SIGN UP";
                        }
                        if (authetication.code.ToString() == Constant.AutheticatedSuccesful)
                        {
                            try
                            {
                                var data = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                                user.setUserID(data.result[0].customer_uid);

                                UpdateTokensPost updateTokesPost = new UpdateTokensPost();
                                updateTokesPost.uid = data.result[0].customer_uid;
                                if (platform == "GOOGLE")
                                {
                                    updateTokesPost.mobile_access_token = accessToken;
                                    updateTokesPost.mobile_refresh_token = refreshToken;
                                }
                                else if (platform == "FACEBOOK")
                                {
                                    updateTokesPost.mobile_access_token = accessToken;
                                    updateTokesPost.mobile_refresh_token = accessToken;
                                }
                                else if (platform == "APPLE")
                                {
                                    updateTokesPost.mobile_access_token = appleCredentials.Token;
                                    updateTokesPost.mobile_refresh_token = appleCredentials.Token;
                                }

                                var updateTokesPostSerializedObject = JsonConvert.SerializeObject(updateTokesPost);
                                var updateTokesContent = new StringContent(updateTokesPostSerializedObject, Encoding.UTF8, "application/json");
                                var updateTokesResponse = await client.PostAsync(Constant.UpdateTokensUrl, updateTokesContent);
                                var updateTokenResponseContent = await updateTokesResponse.Content.ReadAsStringAsync();

                                if (updateTokesResponse.IsSuccessStatusCode)
                                {
                                    var user1 = new RequestUserInfo();
                                    user1.uid = data.result[0].customer_uid;

                                    var requestSelializedObject = JsonConvert.SerializeObject(user1);
                                    var requestContent = new StringContent(requestSelializedObject, Encoding.UTF8, "application/json");

                                    var clientRequest = await client.PostAsync(Constant.GetUserInfoUrl, requestContent);

                                    if (clientRequest.IsSuccessStatusCode)
                                    {
                                        var userSfJSON = await clientRequest.Content.ReadAsStringAsync();
                                        var userProfile = JsonConvert.DeserializeObject<UserInfo>(userSfJSON);

                                        DateTime today = DateTime.Now;
                                        DateTime expDate = today.AddDays(Constant.days);

                                        user.setUserID(data.result[0].customer_uid);
                                        user.setUserSessionTime(expDate);
                                        user.setUserPlatform(platform);
                                        user.setUserType("CUSTOMER");
                                        user.setUserEmail(userProfile.result[0].customer_email);
                                        user.setUserFirstName(userProfile.result[0].customer_first_name);
                                        user.setUserLastName(userProfile.result[0].customer_last_name);
                                        user.setUserPhoneNumber(userProfile.result[0].customer_phone_num);
                                        user.setUserAddress(userProfile.result[0].customer_address);
                                        user.setUserUnit(userProfile.result[0].customer_unit);
                                        user.setUserCity(userProfile.result[0].customer_city);
                                        user.setUserState(userProfile.result[0].customer_state);
                                        user.setUserZipcode(userProfile.result[0].customer_zip);
                                        user.setUserLatitude(userProfile.result[0].customer_lat);
                                        user.setUserLongitude(userProfile.result[0].customer_long);

                                        SaveUser(user);

                                        if (data.result[0].role == "GUEST")
                                        {
                                            var clientSignUp = new SignUp();
                                            var content = clientSignUp.UpdateSocialUser(user, userProfile.result[0].mobile_access_token, userProfile.result[0].mobile_refresh_token, userProfile.result[0].social_id, platform);
                                            var signUpStatus = await SignUp.SignUpNewUser(content);
                                        }

                                        isUserVerified = "LOGIN USER";

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
                                                Debug.WriteLine("We have post the guid to the database");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        isUserVerified = "ERROR1";
                                    }
                                }
                                else
                                {
                                    isUserVerified = "ERROR2";
                                }
                            }
                            catch (Exception second)
                            {
                                Debug.WriteLine(second.Message);
                            }
                        }
                        if (authetication.code.ToString() == Constant.ErrorPlatform)
                        {
                            //var RDSCode = JsonConvert.DeserializeObject<RDSLogInMessage>(responseContent);

                            isUserVerified = "WRONG SOCIAL MEDIA TO SIGN IN";
                        }

                        if (authetication.code.ToString() == Constant.ErrorUserDirectLogIn)
                        {
                            isUserVerified = "SIGN IN DIRECTLY";
                        }
                    }
                }
                return isUserVerified;
            }
            catch (Exception errorVerifyUserCredentials)
            {

                var client = new Diagnostic();
                client.parseException(errorVerifyUserCredentials.ToString(), user);
                isUserVerified = "ERROR";
                return isUserVerified;
            }
        }

        void SaveUser(Models.User user)
        {
            string account = JsonConvert.SerializeObject(user);

            if (Application.Current.Properties.Keys.Contains(Constant.Autheticatior))
            {
                Application.Current.Properties[Constant.Autheticatior] = account;
            }
            else
            {
                Application.Current.Properties.Add(Constant.Autheticatior, account);
            }

            Application.Current.SavePropertiesAsync();
        }

        public async Task<string> ResetPassword(ResetPassword request)
        {
            try
            {
                string result = "";
                var client = new HttpClient();
                var serializedObject = JsonConvert.SerializeObject(request);
                var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
                var endpointCall = await client.PostAsync(Constant.ResetPasswork, content);

                Debug.WriteLine("JSON TO BE SENT " + serializedObject);

                if (endpointCall.IsSuccessStatusCode)
                {
                    var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                    Debug.WriteLine("RESPONSE " + endpointContentString);
                    result = endpointContentString;
                }

                return result;
            }catch(Exception errorResetPassword)
            {
                var client = new Diagnostic();
                client.parseException(errorResetPassword.ToString(), user);
                return "";
            }
        }

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

                        Debug.WriteLine(DRSMessage);

                    }
                    else if (DRSMessage.Contains(Constant.EmailNotFound))
                    {
                       
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
            catch (Exception errorRetrieveAccountSalt)
            {
                var client = new Diagnostic();
                client.parseException(errorRetrieveAccountSalt.ToString(), user);
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
            catch (Exception errorLogInUser)
            {
                var client = new Diagnostic();
                client.parseException(errorLogInUser.ToString(), user);
                return null;
            }
        }


        public OAuth2Authenticator GetFacebookAuthetication()
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
            return authenticator;
        }

        public OAuth2Authenticator GetGoogleAuthetication()
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
            return authenticator;
        }


        public async Task<User> VerifyUserCredentials(string email, string socialID, string platform)
        {
            User resultUser = null;

            var client = new HttpClient();
            var socialUser = new SocialLogInPost();

            socialUser.email = email;
            socialUser.password = "";
            socialUser.signup_platform = platform;
            socialUser.social_id = socialID;

            var socialLogInPostSerialized = JsonConvert.SerializeObject(socialUser);
            var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");

            var test = UserDialogs.Instance.Loading("Loading...");
            var endpointCall = await client.PostAsync(Constant.LogInUrl, postContent);
            if (endpointCall.IsSuccessStatusCode)
            {
               
                var endpointContent = await endpointCall.Content.ReadAsStringAsync();
                var authetication = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(endpointContent);
                if (authetication.code.ToString() == Constant.AutheticatedSuccesful)
                {
                    DateTime today = DateTime.Now;
                    DateTime expDate = today.AddDays(Constant.days);

                    resultUser = new User();
                    resultUser.setUserID(authetication.result[0].customer_uid);
                    resultUser.setUserSessionTime(expDate);
                    resultUser.setUserPlatform(platform);
                    resultUser.setUserType(authetication.result[0].role);
                    resultUser.setUserEmail(authetication.result[0].customer_email);
                    resultUser.setUserFirstName(authetication.result[0].customer_first_name);
                    resultUser.setUserLastName(authetication.result[0].customer_last_name);
                    resultUser.setUserPhoneNumber(authetication.result[0].customer_phone_num);
                    resultUser.setUserAddress(authetication.result[0].customer_address);
                    resultUser.setUserUnit(authetication.result[0].customer_unit);
                    resultUser.setUserCity(authetication.result[0].customer_city);
                    resultUser.setUserState(authetication.result[0].customer_state);
                    resultUser.setUserZipcode(authetication.result[0].customer_zip);
                    resultUser.setUserLatitude(authetication.result[0].customer_lat);
                    resultUser.setUserLongitude(authetication.result[0].customer_long);
                }
            }

            test.Hide();
            return resultUser;
        }


        public FacebookResponse GetFacebookUser(string accessToken){

            FacebookResponse facebookData = null;

            var client = new HttpClient();
            var facebookResponse = client.GetStringAsync(Constant.FacebookUserInfoUrl + accessToken);
            var facebookUserData = facebookResponse.Result;

            facebookData = JsonConvert.DeserializeObject<FacebookResponse>(facebookUserData);
            return facebookData;
        }

        public async Task<GoogleResponse> GetGoogleUser(AuthenticatorCompletedEventArgs googleAccount)
        {
            GoogleResponse googleDate = null;
            var request = new OAuth2Request("GET", new Uri(Constant.GoogleUserInfoUrl), null, googleAccount.Account);
            var GoogleResponse = await request.GetResponseAsync();
            var googelUserData = GoogleResponse.GetResponseText();

            googleDate = JsonConvert.DeserializeObject<GoogleResponse>(googelUserData);
            return googleDate;
        }

        public async Task<UserProfile> ValidateExistingAccountFromEmail(string email)
        {
            try
            {
                UserProfile result = null;

                var client = new System.Net.Http.HttpClient();
                var endpointCall = await client.GetAsync(Constant.UpdateUserProfile + email);


                if (endpointCall.IsSuccessStatusCode)
                {
                    var endpointContent = await endpointCall.Content.ReadAsStringAsync();
                    Debug.WriteLine("PROFILE: " + endpointContent);
                    var profile = JsonConvert.DeserializeObject<UserProfile>(endpointContent);
                    if (profile.result.Count != 0)
                    {
                        result = profile;
                    }

                }

                return result;
            }catch(Exception errorValidateExistingAccountFromEmail)
            {
                var client = new Diagnostic();
                client.parseException(errorValidateExistingAccountFromEmail.ToString(), user);
                UserProfile result = null;
                return result;
            }
        }

        public async Task<bool> UpdateProfile(UserProfile profile)
        {
            try
            {
                bool result = false;

                var client = new HttpClient();
                var updateClient = new UpdatedProfile();
                var updatedProfile = updateClient.GetUpdatedProfile(profile);

                var serializedObject = JsonConvert.SerializeObject(updatedProfile);
                var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
                var endpointCall = await client.PostAsync(Constant.UpdateUserProfileAddress, content);

                Debug.WriteLine("JSON TO BE SEND: " + serializedObject);

                if (endpointCall.IsSuccessStatusCode)
                {
                    var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                    Debug.WriteLine("UPDATED PROFILE: " + endpointContentString);
                    result = true;
                }

                return result;
            }catch(Exception errorUpdateProfile)
            {
                var client = new Diagnostic();
                client.parseException(errorUpdateProfile.ToString(), user);
                return false;
            }
        }
    }
}
