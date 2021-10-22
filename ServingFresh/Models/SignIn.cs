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
        public class UserTypeEvaluation { public string role { get; set; } public bool statusCode { get; set; } public UserTypeEvaluation() { role = "CUSTOMER"; statusCode = false; } }
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

        public async Task<AccountSalt> RetrieveAccountSalt(string userEmail)
        {
            AccountSalt userInformation = null;

            try
            {
                SaltPost saltPost = new SaltPost();
                saltPost.email = userEmail;

                var saltPostSerilizedObject = JsonConvert.SerializeObject(saltPost);
                var saltPostContent = new StringContent(saltPostSerilizedObject, Encoding.UTF8, "application/json");

                var client = new HttpClient();
                var DRSResponse = await client.PostAsync(Constant.AccountSaltUrl, saltPostContent);
                var DRSMessage = await DRSResponse.Content.ReadAsStringAsync();

                if (DRSResponse.IsSuccessStatusCode)
                {
                    var result = await DRSResponse.Content.ReadAsStringAsync();

                    AcountSaltCredentials data = new AcountSaltCredentials();
                    data = JsonConvert.DeserializeObject<AcountSaltCredentials>(result);

                    if (DRSMessage.Contains(Constant.UseSocialMediaLogin))
                    {
                        userInformation = new AccountSalt
                        {
                            password_algorithm = null,
                            password_salt = null,
                            message = data.message == null ? "" : data.message
                        };
                    }
                    else if (DRSMessage.Contains(Constant.EmailNotFound))
                    {
                        userInformation = new AccountSalt
                        {
                            password_algorithm = null,
                            password_salt = null,
                            message = "USER NEEDS TO SIGN UP"
                        };
                    }
                    else
                    {
                        userInformation = new AccountSalt
                        {
                            password_algorithm = data.result[0].password_algorithm,
                            password_salt = data.result[0].password_salt,
                            message = null
                        };
                    }
                }
            }
            catch (Exception errorRetrieveAccountSalt)
            {
                var client = new Diagnostic();
                client.parseException(errorRetrieveAccountSalt.ToString(), user);
            }

            return userInformation;
        }

        public async Task<string> VerifyUserCredentials(string userEmail, string userPassword, AccountSalt accountSalt)
        {
            string isUserVerified = "";

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

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authetication = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);

                    if (authetication.code.ToString() == Constant.EmailNotFound)
                    {
                        isUserVerified = "USER NEEDS TO SIGN UP";
                    }
                    else if (authetication.code.ToString() == Constant.AutheticatedSuccesful)
                    {
                        isUserVerified = "LOGIN USER";

                        DateTime today = DateTime.Now;
                        DateTime expDate = today.AddDays(Constant.days);

                        var status1 = await EvaluateUserType(authetication.result[0].role, userPassword);

                        user.setUserType(status1.role);
                        user.setUserID(authetication.result[0].customer_uid);
                        user.setUserSessionTime(expDate);
                        user.setUserPlatform("DIRECT");
                        user.setUserEmail(authetication.result[0].customer_email);
                        user.setUserFirstName(authetication.result[0].customer_first_name);
                        user.setUserLastName(authetication.result[0].customer_last_name);
                        user.setUserPhoneNumber(authetication.result[0].customer_phone_num);
                        user.setUserAddress(authetication.result[0].customer_address);
                        user.setUserUnit(authetication.result[0].customer_unit);
                        user.setUserCity(authetication.result[0].customer_city);
                        user.setUserState(authetication.result[0].customer_state);
                        user.setUserZipcode(authetication.result[0].customer_zip);
                        user.setUserLatitude(authetication.result[0].customer_lat);
                        user.setUserLongitude(authetication.result[0].customer_long);

                        var status2 = await SetUserRemoteNotification();

                        isUserVerified = EvaluteUserUpdates(status1.statusCode, status2);

                        SaveUser(user);
                    }
                    else if (authetication.code.ToString() == Constant.ErrorPlatform)
                    {
                        //var RDSCode = JsonConvert.DeserializeObject<RDSLogInMessage>(responseContent);

                        isUserVerified = "WRONG SOCIAL MEDIA TO SIGN IN";

                    }else if (authetication.code.ToString() == Constant.ErrorUserDirectLogIn)
                    {
                        isUserVerified = "SIGN IN DIRECTLY";
                    }
                    
                }


            }
            catch (Exception errorLogInUser)
            {
                var client = new Diagnostic();
                client.parseException(errorLogInUser.ToString(), user);
            }

            return isUserVerified;
        }

        // update role, set notifications

        string EvaluteUserUpdates(bool status1, bool status2)
        {
            string result = "SUCCESSFUL:0";

            if(status1 && status2) // 11
            {
                result = "SUCCESSFUL:0";
            }
            else if (status1 == true && status2 == false) // 10
            {
                result = "SUCCESSFUL:1";
            }
            else if (status1 == false && status2 == true) // 01
            {
                result = "SUCCESSFUL:2";
            }
            else if (status1 == false && status2 == false) // 00
            {
                result = "SUCCESSFUL:3";
            }

            return result;
        }


        // update role, set notification, update tokens

        string EvaluteUserUpdates(bool status1, bool status2, bool status3)
        {
            string result = "SUCCESSFUL:0";

            if (status1 && status2 && status3) // 111
            {
                result = "SUCCESSFUL:0";
            }
            else if (status1 == true && status2 == true && status3 == false) // 110
            {
                result = "SUCCESSFUL:5";
            }
            else if (status1 == true && status2 == false && status3 == true) // 101
            {
                result = "SUCCESSFUL:6";
            }
            else if (status1 == true && status2 == false && status3 == false) // 100
            {
                result = "SUCCESSFUL:7";
            }
            else if (status1 == true && status2 == false && status3 == false) // 011
            {
                result = "SUCCESSFUL:8";
            }
            else if (status1 == true && status2 == false && status3 == false) // 010
            {
                result = "SUCCESSFUL:9";
            }
            else if (status1 == true && status2 == false && status3 == false) // 001
            {
                result = "SUCCESSFUL:10";
            }
            else if (status1 == true && status2 == false && status3 == false) // 000
            {
                result = "SUCCESSFUL:11";
            }

            return result;
        }

        async Task<UserTypeEvaluation> EvaluateUserType(string role, string password)
        {
            UserTypeEvaluation userType = new UserTypeEvaluation();

            try
            {
                if (role == "CUSTOMER" || role == "ADMIN")
                {
                    userType.role = "CUSTOMER";
                    userType.statusCode = true;
                }
                else if (role == "GUEST")
                {
                    var didProfileUpdatedSucessfully = await UpdateUserProfile(password);

                    if (didProfileUpdatedSucessfully)
                    {
                        userType.role = "CUSTOMER";
                        userType.statusCode = true;
                    }
                    else
                    {
                        userType.role = "GUEST";
                        userType.statusCode = false;
                    }
                }
            }
            catch
            {

            }

            return userType;
           
        }

        async Task<UserTypeEvaluation> EvaluateUserType(string role, string mobile_access_token, string mobile_refresh_token, string social_id, string platform)
        {
            UserTypeEvaluation userType = new UserTypeEvaluation();

            try
            {
                if (role == "CUSTOMER" || role == "ADMIN")
                {
                    userType.role = "CUSTOMER";
                    userType.statusCode = true;
                }
                else if (role == "GUEST")
                {
                    var didProfileUpdatedSucessfully = await UpdateUserProfile(accessToken, refreshToken, social_id, platform);

                    if (didProfileUpdatedSucessfully)
                    {
                        userType.role = "CUSTOMER";
                        userType.statusCode = true;
                    }
                    else
                    {
                        userType.role = "GUEST";
                        userType.statusCode = false;
                    }
                }
            }
            catch
            {

            }

            return userType;

        }

        async Task<bool> UpdateUserProfile(string password)
        {
            bool result = false;

            try
            {
                var clientSignUp = new SignUp();
                var content = clientSignUp.UpdateDirectUser(user, password);
                result = await SignUp.SignUpNewUser(content);
            }
            catch
            {
                Debug.Write("ERROR UPDATING DIRECT USER'S PROFILE FROM GUEST TO CUSTOMER");
            }

            return result;
        }

        async Task<bool> UpdateUserProfile(string mobile_access_token, string mobile_refresh_token, string social_id, string platform)
        {
            bool result = false;

            try
            {
                var clientSignUp = new SignUp();
                var content = clientSignUp.UpdateSocialUser(user, mobile_access_token, mobile_refresh_token, social_id, platform);
                result = await SignUp.SignUpNewUser(content);
            }
            catch
            {
                Debug.Write("ERROR UPDATING SOCIAL MEDIA USER'S PROFILE FROM GUEST TO CUSTOMER");
            }

            return result;

        }

        async Task<bool> SetUserRemoteNotification()
        {
            bool result = false;

            try
            {
                deviceId = Preferences.Get("guid", null);

                if (deviceId != null)
                {
                    var client = new HttpClient();
                    NotificationPost notificationPost = new NotificationPost();

                    notificationPost.uid = user.getUserID();
                    notificationPost.guid = deviceId.Substring(5);
                    user.setUserDeviceID(deviceId.Substring(5));
                    notificationPost.notification = "TRUE";

                    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");
                    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                    if (clientResponse.IsSuccessStatusCode)
                    {
                        result = true;
                        Debug.WriteLine("GUID WAS WRITTEN SUCCESFULLY WERE SET SUCESSFULLY");
                    }
                    else
                    {
                        Debug.WriteLine("ERROR SETTING GUID FOR NOTIFICATIONS");
                    }
                }
            }
            catch
            {

            }

            return result;
        }

        public async Task<string> VerifyUserCredentials(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleAccount = null, AppleAccount appleCredentials = null, string platform = "")
        {
            var isUserVerified = "";

            try
            {
                string _accessToken = accessToken;
                string _refreshToken = refreshToken;

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

                    _accessToken = accessToken;
                    _refreshToken = refreshToken;
                }
                else if (platform == "FACEBOOK")
                {
                    var facebookResponse = client.GetStringAsync(Constant.FacebookUserInfoUrl + accessToken);
                    var facebookUserData = facebookResponse.Result;

                    Debug.WriteLine("FACEBOOK DATA: " + facebookUserData);
                    facebookData = JsonConvert.DeserializeObject<FacebookResponse>(facebookUserData);

                    socialLogInPost.email = facebookData.email;
                    socialLogInPost.social_id = facebookData.id;

                    _accessToken = accessToken;
                    _refreshToken = refreshToken;
                }
                else if (platform == "APPLE")
                {
                    socialLogInPost.email = appleCredentials.Email;
                    socialLogInPost.social_id = appleCredentials.UserId;

                    _accessToken = appleCredentials.Token;
                    _refreshToken = appleCredentials.Token;
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
                               

                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                var status1 = await EvaluateUserType(authetication.result[0].role, accessToken, refreshToken, socialLogInPost.social_id, platform);

                                user.setUserType(status1.role);
                                user.setUserID(authetication.result[0].customer_uid);
                                user.setUserSessionTime(expDate);
                                user.setUserPlatform(platform);
                                user.setUserEmail(authetication.result[0].customer_email);
                                user.setUserFirstName(authetication.result[0].customer_first_name);
                                user.setUserLastName(authetication.result[0].customer_last_name);
                                user.setUserPhoneNumber(authetication.result[0].customer_phone_num);
                                user.setUserAddress(authetication.result[0].customer_address);
                                user.setUserUnit(authetication.result[0].customer_unit);
                                user.setUserCity(authetication.result[0].customer_city);
                                user.setUserState(authetication.result[0].customer_state);
                                user.setUserZipcode(authetication.result[0].customer_zip);
                                user.setUserLatitude(authetication.result[0].customer_lat);
                                user.setUserLongitude(authetication.result[0].customer_long);

                                var status2 = await SetUserRemoteNotification();
                                var status3 = await UpdateAccessRefreshToken(user.getUserID(), accessToken, refreshToken);

                                //  isUserVerified = "LOGIN USER";

                                isUserVerified = EvaluteUserUpdates(status1.statusCode, status2, status3);

                                SaveUser(user);
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
            }
            catch (Exception errorVerifyUserCredentials)
            {
                var client = new Diagnostic();
                client.parseException(errorVerifyUserCredentials.ToString(), user);
                isUserVerified = "ERROR";
            }

            return isUserVerified;
        }

        async Task<bool> UpdateAccessRefreshToken(string id, string accessToken, string refreshToken)
        {
            bool result = false;

            try
            {
                var client = new HttpClient();

                UpdateTokensPost updateTokesPost = new UpdateTokensPost();

                updateTokesPost.uid = id;
                updateTokesPost.mobile_access_token = accessToken;
                updateTokesPost.mobile_refresh_token = refreshToken;

                var updateTokesPostSerializedObject = JsonConvert.SerializeObject(updateTokesPost);
                var updateTokesContent = new StringContent(updateTokesPostSerializedObject, Encoding.UTF8, "application/json");
                var updateTokesResponse = await client.PostAsync(Constant.UpdateTokensUrl, updateTokesContent);

                if (updateTokesResponse.IsSuccessStatusCode)
                {
                    result = true;
                    Debug.WriteLine("UPDATING ACCESS AND REFRESH TOKENS WAS SUCESSFULLY");
                }
                else
                {
                    Debug.WriteLine("ERROR UPDATING ACCESS AND REFRESH TOKENS");
                }
            }
            catch
            {

            }

            return result;
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
            string result = "";

            try
            {

                var client = new HttpClient();
                var serializedObject = JsonConvert.SerializeObject(request);
                var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
                var endpointCall = await client.PostAsync(Constant.ResetPasswork, content);


                if (endpointCall.IsSuccessStatusCode)
                {
                    var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                    result = endpointContentString;
                }

            }catch(Exception errorResetPassword)
            {
                var client = new Diagnostic();
                client.parseException(errorResetPassword.ToString(), user);
            }

            return result;
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

            try
            {
                var client = new HttpClient();
                var facebookResponse = client.GetStringAsync(Constant.FacebookUserInfoUrl + accessToken);
                var facebookUserData = facebookResponse.Result;

                facebookData = JsonConvert.DeserializeObject<FacebookResponse>(facebookUserData);
            }
            catch
            {

            }

            return facebookData;
        }

        public async Task<GoogleResponse> GetGoogleUser(AuthenticatorCompletedEventArgs googleAccount)
        {
            GoogleResponse googleDate = null;

            try
            {
                var request = new OAuth2Request("GET", new Uri(Constant.GoogleUserInfoUrl), null, googleAccount.Account);
                var GoogleResponse = await request.GetResponseAsync();
                var googelUserData = GoogleResponse.GetResponseText();

                googleDate = JsonConvert.DeserializeObject<GoogleResponse>(googelUserData);
            }
            catch
            {

            }
            
            return googleDate;
        }

        public async Task<UserProfile> ValidateExistingAccountFromEmail(string email)
        {
            UserProfile result = null;

            try
            {
                var client = new HttpClient();
                var endpointCall = await client.GetAsync(Constant.UpdateUserProfile + email);

                if (endpointCall.IsSuccessStatusCode)
                {
                    var endpointContent = await endpointCall.Content.ReadAsStringAsync();
                    var profile = JsonConvert.DeserializeObject<UserProfile>(endpointContent);
                    if (profile.result.Count != 0)
                    {
                        result = profile;
                    }
                }
            }catch(Exception errorValidateExistingAccountFromEmail)
            {
                var client = new Diagnostic();
                client.parseException(errorValidateExistingAccountFromEmail.ToString(), user);
            }

            return result;
        }

        public async Task<bool> UpdateProfile(UserProfile profile)
        {
            bool result = false;

            try
            {
                var client = new HttpClient();
                var updateClient = new UpdatedProfile();
                var updatedProfile = updateClient.GetUpdatedProfile(profile);

                var serializedObject = JsonConvert.SerializeObject(updatedProfile);
                var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
                var endpointCall = await client.PostAsync(Constant.UpdateUserProfileAddress, content);

                if (endpointCall.IsSuccessStatusCode)
                {
                    var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                    result = true;
                }

            }
            catch(Exception errorUpdateProfile)
            {
                var client = new Diagnostic();
                client.parseException(errorUpdateProfile.ToString(), user);
            }

            return result;
        }
    }
}
