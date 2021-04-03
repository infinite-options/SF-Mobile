﻿using System;
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
            //Application.Current.Properties.Clear();
            //SecureStorage.RemoveAll();
            //Preferences.Clear();

            Application.Current.MainPage = new CartPage();

            Application.Current.Properties["guest"] = false;
            Application.Current.Properties["user_email"] = "";
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
            CardInfo();

            //if (Application.Current.Properties.ContainsKey("user_id"))
            //{
            //    if (Application.Current.Properties.ContainsKey("time_stamp"))
            //    {
            //        DateTime today = DateTime.Now;
            //        DateTime expTime = (DateTime)Application.Current.Properties["time_stamp"];

            //        if (today <= expTime)
            //        {
            //            Console.WriteLine("guid: " + Preferences.Get("guid", null));
            //            MainPage = new SelectionPage();
            //        }
            //        else
            //        {
            //            LogInPage client = new LogInPage();
            //            MainPage = client;

            //            if (Application.Current.Properties.ContainsKey("time_stamp"))
            //            {
            //                string socialPlatform = (string)Application.Current.Properties["platform"];

            //                if (socialPlatform.Equals(Constant.Facebook))
            //                {
            //                    client.FacebookLogInClick(new object(), new EventArgs());
            //                }
            //                else if (socialPlatform.Equals(Constant.Google))
            //                {
            //                    client.GoogleLogInClick(new object(), new EventArgs());
            //                }
            //                else if (socialPlatform.Equals(Constant.Apple))
            //                {
            //                    client.AppleLogInClick(new object(), new EventArgs());
            //                }
            //                else
            //                {
            //                    MainPage = new LogInPage();
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        MainPage = new LogInPage();
            //    }
            //}
            //else
            //{
            //    MainPage = new LogInPage();
            //}
        }

        public void CardInfo()
        {
            Application.Current.Properties["CardNumber"] = "";
            Application.Current.Properties["CardExpMonth"] = "";
            Application.Current.Properties["CardExpYear"] = "";
            Application.Current.Properties["CardCVV"] = "";
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
