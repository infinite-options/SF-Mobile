using System;
using ServingFresh.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ServingFresh.Config;
using Xamarin.Essentials;
using ServingFresh.LogIn.Apple;
using System.Diagnostics;
using static ServingFresh.Views.PrincipalPage;
namespace ServingFresh
{
    public partial class App : Application
    {
        public const string LoggedInKey = "LoggedIn";
        public const string AppleUserIdKey = "AppleUserIdKey";
        string userId;
        
        public App()
        {
            InitializeComponent();            
            try
            {

                if(user.getUserID() == "")
                {
                    MainPage = new PrincipalPage();
                }
                else
                {
                    DateTime today = DateTime.Now;
                    var expTime = user.getUserSessionTime();

                    if (today <= expTime)
                    {
                        //Console.WriteLine("guid: " + Preferences.Get("guid", null));
                        MainPage = new SelectionPage();
                    }
                    else
                    {
                        LogInPage client = new LogInPage();
                        MainPage = client;

                        
                        string socialPlatform = user.getUserPlatform();

                        if (socialPlatform.Equals(Constant.Facebook))
                        {
                            client.FacebookLogInClick(new object(), new EventArgs());
                        }
                        else if (socialPlatform.Equals(Constant.Google))
                        {
                            client.GoogleLogInClick(new object(), new EventArgs());
                        }
                        else if (socialPlatform.Equals(Constant.Apple))
                        {
                            client.AppleLogInClick(new object(), new EventArgs());
                        }
                        else
                        {
                            MainPage = new LogInPage();
                        }
                        
                    }
                }
            }
            catch (Exception autoLoginFailed)
            {
                MainPage = new PrincipalPage();
                Debug.WriteLine("ERROR ON AUTO LOGIN");
                Debug.WriteLine(autoLoginFailed.Message);
            }
        }

        protected override async void OnStart()
        {
            var appleSignInService = DependencyService.Get<IAppleSignInService>();

            if (appleSignInService != null)
            {
                userId = await SecureStorage.GetAsync(AppleUserIdKey);
                if (appleSignInService.IsAvailable && !string.IsNullOrEmpty(userId))
                {
                    var credentialState = await appleSignInService.GetCredentialStateAsync(userId);
                    switch (credentialState)
                    {
                        case AppleSignInCredentialState.Authorized:
                            break;
                        case AppleSignInCredentialState.NotFound:
                        case AppleSignInCredentialState.Revoked:
                            SecureStorage.Remove(AppleUserIdKey);
                            Preferences.Set(LoggedInKey, false);
                            MainPage = new LogInPage();
                            break;
                    }
                }
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
