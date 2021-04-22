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
using ServingFresh.Models;
using System.Diagnostics;
using System.Collections.Generic;

namespace ServingFresh.LogIn.Apple
{
    public class Info
    {
        public string customer_email { get; set; }
    }

    public class AppleUser
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<Info> result { get; set; }
        public string sql { get; set; }
    }

    public class AppleEmail
    {
        public string social_id {get;set;}
    }
    public class LoginViewModel
    {
        public static string apple_token = null;
        public static string apple_email = null;

        public bool IsAppleSignInAvailable { get { return appleSignInService?.IsAvailable ?? false; } }
        public ICommand SignInWithAppleCommand { get; set; }

        public event EventHandler AppleError = delegate { };

        IAppleSignInService appleSignInService = null;
        private string deviceId;

        public LoginViewModel()
        {
            appleSignInService = DependencyService.Get<IAppleSignInService>();
            SignInWithAppleCommand = new Command(OnAppleSignInRequest);
            if (Device.RuntimePlatform == Device.iOS)
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Direct Sign Up: " + deviceId); }
            }
        }

        public async void OnAppleSignInRequest()
        {
            try
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
                        Application.Current.MainPage = new SelectionPage("", "", null, account, "APPLE");
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
                            Application.Current.MainPage = new SelectionPage("", "", null, account, "APPLE");
                            //AppleUserProfileAsync(account.UserId, account.Token, (string)Application.Current.Properties[account.UserId.ToString()], account.Name);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Ooops", "Our system is not working. We can't process your request at this moment", "OK");
                        }
                    }
                }
                else
                {
                    AppleError?.Invoke(this, default(EventArgs));
                }
            }
            catch(Exception apple)
            {
                await Application.Current.MainPage.DisplayAlert("Error", apple.Message, "OK");
            }
        }

      
    }
}
