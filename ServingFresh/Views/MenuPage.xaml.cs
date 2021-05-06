using System;
using System.Collections.Generic;
using static ServingFresh.Views.PrincipalPage;
using static ServingFresh.Views.SelectionPage;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("AB000000");
            SetMenu(guestMenuSection, customerMenuSection, historyLabel, profileLabel);
        }

        public static void SetMenu(StackLayout guest, StackLayout customer, Label history, Label profile)
        {
            if (user.getUserType() == "GUEST")
            {
                customer.HeightRequest = 0;
                SetMenuLabel(history, "History (sign in required)");
                SetMenuLabel(profile, "Profile (sign in required)");
            }
            else if (user.getUserType() == "CUSTOMER")
            {
                guest.HeightRequest = 0;
            }
        }

        public static void NavigateToStore(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

        public static void NavigateToCart(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage();
        }

        public static void NavigateToHistory(System.Object sender, System.EventArgs e)
        {
            if (user.getUserType() == "CUSTOMER")
            {
                Application.Current.MainPage = new HistoryPage();
            }
        }

        public static void NavigateToRefunds(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new RefundPage();
        }

        public static void NavigateToInfo(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new InfoPage();
        }

        public static void NavigateToProfile(System.Object sender, System.EventArgs e)
        {
            if (user.getUserType() == "CUSTOMER")
            {
                Application.Current.MainPage = new ProfilePage();
            }
        }

        public static void NavigateToSignIn(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94, "1"), true);
        }

        public static void NavigateToSignUp(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
            Application.Current.MainPage.Navigation.PushModalAsync(new SignUpPage(94), true);
        }

        public static void NavigateToMain(System.Object sender, System.EventArgs e)
        {
            ResetUser(user);
            order.Clear();
            Application.Current.MainPage = new PrincipalPage();
        }

        static void ResetUser(Models.User user)
        {
            user.setUserType("");
            user.setUserID("");
            user.setUserFirstName("");
            user.setUserLastName("");
            user.setUserAddress("");
            user.setUserUnit("");
            user.setUserCity("");
            user.setUserState("");
            user.setUserZipcode("");
            user.setUserPhoneNumber("");
            user.setUserLatitude("");
            user.setUserLongitude("");
            user.setUserPlatform("");
            user.setUserDeviceID("");
            user.setUserSessionTime(new DateTime());
        }

        void NavigateToMainFromSelection(System.Object sender, System.EventArgs e)
        {
            NavigateToMain(sender, e);
        }

        public static void SetMenuLabel(Label section, string title)
        {
            section.Text = title;
            section.TextColor = Color.FromHex("#FF8500");
        }

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}
