using System;
using ServingFresh.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ServingFresh.Config;
using Xamarin.Essentials;
using ServingFresh.LogIn.Apple;
using System.Diagnostics;
using static ServingFresh.Views.PrincipalPage;
using ServingFresh.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ServingFresh
{
    public partial class App : Application
    {
        //

        public const string LoggedInKey = "LoggedIn";
        public const string AppleUserIdKey = "AppleUserIdKey";
        string userId;
        public static Dictionary<string, MessageResult> messageList = null;

        // 

        public App()
        {
            InitializeComponent();            
            try
            {
                SetAlertMessageList();

                if (Application.Current.Properties.Keys.Contains(Constant.Autheticatior))
                {
                    var tempUser = JsonConvert.DeserializeObject<User>(Current.Properties[Constant.Autheticatior].ToString());

                    DateTime today = DateTime.Now;
                    var expTime = tempUser.getUserSessionTime();

                    if (today <= expTime)
                    {
                        SetUser(tempUser);
                        MainPage = new SelectionPage();
                    }
                    else
                    {
                        MainPage = new PrincipalPage();

                        string socialPlatform = tempUser.getUserPlatform();

                        if (socialPlatform.Equals(Constant.Facebook))
                        {
                            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(Constant.Facebook));
                        }
                        else if (socialPlatform.Equals(Constant.Google))
                        {
                            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(Constant.Google));
                        }
                        else if (socialPlatform.Equals(Constant.Apple))
                        {
                            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(Constant.Apple));
                        }
                    }
                }
                else
                {
                    MainPage = new PrincipalPage();
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
                            MainPage = new PrincipalPage();
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

        void SetUser(User temp)
        {
            user.setUserID(temp.getUserID());
            user.setUserSessionTime(temp.getUserSessionTime());
            user.setUserPlatform(temp.getUserPlatform());
            user.setUserType(temp.getUserType());
            user.setUserEmail(temp.getUserEmail());
            user.setUserFirstName(temp.getUserFirstName());
            user.setUserLastName(temp.getUserLastName());
            user.setUserPhoneNumber(temp.getUserPhoneNumber());
            user.setUserAddress(temp.getUserAddress());
            user.setUserUnit(temp.getUserUnit());
            user.setUserCity(temp.getUserCity());
            user.setUserState(temp.getUserState());
            user.setUserZipcode(temp.getUserZipcode());
            user.setUserLatitude(temp.getUserLatitude());
            user.setUserLongitude(temp.getUserLongitude());
            user.setUserUSPSType(temp.getUserUSPSType());
            user.setUserImage(temp.getUserImage());
        }

        async void SetAlertMessageList()
        {
            var messageClient = new AlertMessage();
            messageList = await messageClient.GetMessageList();
        }
    }
}
