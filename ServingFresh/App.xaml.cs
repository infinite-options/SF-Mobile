using System;
using ServingFresh.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ServingFresh.Config;
using Xamarin.Essentials;
using ServingFresh.LogIn.Apple;

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
            // Application.Current.Properties.Clear();                                              // Resets user info in the app.  Use for debug
            // SecureStorage.RemoveAll();                                                           // Allows Xamarin to reset Apple security storage info stored in hardware.  Use for debug
            if (Application.Current.Properties.ContainsKey("user_id"))                              // Additional parameters defined in LoginPage.xaml.cs.  You can add more on the fly
            {
                if (Application.Current.Properties.ContainsKey("time_stamp"))
                {
                    DateTime today = DateTime.Now;
                    DateTime expTime = (DateTime)Application.Current.Properties["time_stamp"];      // DateTime) is casting the data in Date Time format

                    if (today <= expTime)
                    {
                        MainPage = new SelectionPage(); 
                    }
                    else                                                                            // Could use an else if statment here
                    {
                        LogInPage client = new LogInPage();                                         // Why not simply MainPage = new LogInPage();  What is the advantage of client?
                        MainPage = client;                                                          // Perhaps need client to check client.* below

                        if (Application.Current.Properties.ContainsKey("platform"))                 // Check for Platform
                        {
                            string socialPlatform = (string)Application.Current.Properties["platform"];
                            
                            if (socialPlatform.Equals(Constant.Facebook))                           // Compares two strings.  Same as "Facebook".Equals"Facebook".  C# syntax
                            {
                                client.FacebookLogInClick(new object(), new EventArgs());           // Event Handlers.  Calls *LogInClick Function in LogInPage.xaml.cs as if clicked
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
                else
                {
                    MainPage = new LogInPage();
                }
            }
            else
            {
                MainPage = new LogInPage();
            }
        }

        // Initialization function that checks if a user has logged in through Apple
        protected override async void OnStart()
        {
            var appleSignInService = DependencyService.Get<IAppleSignInService>();

            // Retrieve user info if user is signed on via Apple ID)
            if (appleSignInService != null)
            {
                userId = await SecureStorage.GetAsync(AppleUserIdKey);
                System.Diagnostics.Debug.WriteLine("This is the Apple userID :" + userId);
                if (appleSignInService.IsAvailable && !string.IsNullOrEmpty(userId))
                {
                    var credentialState = await appleSignInService.GetCredentialStateAsync(userId);
                    switch (credentialState)
                    {
                        case AppleSignInCredentialState.Authorized:
                            break;
                        case AppleSignInCredentialState.NotFound:
                        case AppleSignInCredentialState.Revoked:
                            //Logout;
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
