using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ServingFresh.Config;
using ServingFresh.LogIn.Classes;
using ServingFresh.Views;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace ServingFresh.LogIn.Apple
{
    public class LoginViewModel
    {
        public static string apple_token = null;
        public static string apple_email = null;

        public bool IsAppleSignInAvailable { get { return appleSignInService?.IsAvailable ?? false; } }
        public ICommand SignInWithAppleCommand { get; set; }

        public event EventHandler AppleError = delegate { };

        IAppleSignInService appleSignInService = null;
        public LoginViewModel()
        {
            appleSignInService = DependencyService.Get<IAppleSignInService>();
            SignInWithAppleCommand = new Command(OnAppleSignInRequest);
        }

        public async void OnAppleSignInRequest()
        {
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
                        Application.Current.Properties[account.UserId.ToString()] = account.Email;
                    }
                    else
                    {
                        Application.Current.Properties[account.UserId.ToString()] = account.Email;
                    }
                }
                if (account.Email == null) { account.Email = ""; }
                if (account.Name == null) { account.Name = ""; }

                System.Diagnostics.Debug.WriteLine((string)Application.Current.Properties[account.UserId.ToString()]);
                AppleUserProfileAsync(account.UserId, account.Token, (string)Application.Current.Properties[account.UserId.ToString()], account.Name);
            }
            else
            {
                AppleError?.Invoke(this, default(EventArgs));
            }
        }

        public async void AppleUserProfileAsync(string appleId, string appleToken, string appleUserEmail, string userName)
        {
            var client = new HttpClient();
            var socialLogInPost = new SocialLogInPost();

            socialLogInPost.email = appleUserEmail;
            socialLogInPost.password = "";
            socialLogInPost.social_id = appleId;
            socialLogInPost.signup_platform = "APPLE";

            var socialLogInPostSerialized = JsonConvert.SerializeObject(socialLogInPost);

            System.Diagnostics.Debug.WriteLine(socialLogInPostSerialized);

            var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");
            var RDSResponse = await client.PostAsync(Constant.LogInUrl, postContent);
            var responseContent = await RDSResponse.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(responseContent);

            if (RDSResponse.IsSuccessStatusCode)
            {
                if (responseContent != null)
                {
                    if (responseContent.Contains(Constant.EmailNotFound))
                    {
                        var signUp = await Application.Current.MainPage.DisplayAlert("Message", "It looks like you don't have a Serving Fresh account. Please sign up!", "OK", "Cancel");
                        if (signUp)
                        {
                            Application.Current.MainPage = new SocialSignUp(appleId, userName, "", appleUserEmail, appleToken, appleToken, "APPLE");
                        }
                    }
                    if (responseContent.Contains(Constant.AutheticatedSuccesful))
                    {
                        var data = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                        Application.Current.Properties["user_id"] = data.result[0].customer_uid;

                        UpdateTokensPost updateTokesPost = new UpdateTokensPost();
                        updateTokesPost.uid = data.result[0].customer_uid;
                        updateTokesPost.mobile_access_token = appleToken;
                        updateTokesPost.mobile_refresh_token = appleToken;

                        var updateTokesPostSerializedObject = JsonConvert.SerializeObject(updateTokesPost);
                        var updateTokesContent = new StringContent(updateTokesPostSerializedObject, Encoding.UTF8, "application/json");
                        var updateTokesResponse = await client.PostAsync(Constant.UpdateTokensUrl, updateTokesContent);
                        var updateTokenResponseContent = await updateTokesResponse.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(updateTokenResponseContent);

                        if (updateTokesResponse.IsSuccessStatusCode)
                        {
                            var AppleRequest = new RequestUserInfo();
                            AppleRequest.uid = data.result[0].customer_uid;

                            var requestSelializedObject = JsonConvert.SerializeObject(AppleRequest);
                            var requestContent = new StringContent(requestSelializedObject, Encoding.UTF8, "application/json");

                            var clientRequest = await client.PostAsync(Constant.GetUserInfoUrl, requestContent);

                            if (clientRequest.IsSuccessStatusCode)
                            {
                                var SFUser = await clientRequest.Content.ReadAsStringAsync();
                                var AppleUserData = JsonConvert.DeserializeObject<UserInfo>(SFUser);

                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                Application.Current.Properties["user_id"] = data.result[0].customer_uid;
                                Application.Current.Properties["time_stamp"] = expDate;
                                Application.Current.Properties["platform"] = "APPLE";
                                Application.Current.Properties["user_email"] = AppleUserData.result[0].customer_email;
                                Application.Current.Properties["user_first_name"] = AppleUserData.result[0].customer_first_name;
                                Application.Current.Properties["user_last_name"] = AppleUserData.result[0].customer_last_name;
                                Application.Current.Properties["user_phone_num"] = AppleUserData.result[0].customer_phone_num;
                                Application.Current.Properties["user_address"] = AppleUserData.result[0].customer_address;
                                Application.Current.Properties["user_unit"] = AppleUserData.result[0].customer_unit;
                                Application.Current.Properties["user_city"] = AppleUserData.result[0].customer_city;
                                Application.Current.Properties["user_state"] = AppleUserData.result[0].customer_state;
                                Application.Current.Properties["user_zip_code"] = AppleUserData.result[0].customer_zip;
                                Application.Current.Properties["user_latitude"] = AppleUserData.result[0].customer_lat;
                                Application.Current.Properties["user_longitude"] = AppleUserData.result[0].customer_long;

                                _ = Application.Current.SavePropertiesAsync();
                                Application.Current.MainPage = new SelectionPage();
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                            }
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Oops", "We are facing some problems with our internal system. We weren't able to update your credentials", "OK");
                        }
                    }
                    if (responseContent.Contains(Constant.ErrorPlatform))
                    {
                        var RDSCode = JsonConvert.DeserializeObject<RDSLogInMessage>(responseContent);
                        await Application.Current.MainPage.DisplayAlert("Message", RDSCode.message, "OK");
                    }

                    if (responseContent.Contains(Constant.ErrorUserDirectLogIn))
                    {
                        await Application.Current.MainPage.DisplayAlert("Oops!", "You have an existing Serving Fresh account. Please use direct login", "OK");
                    }
                }
            }
        }
    }
}
