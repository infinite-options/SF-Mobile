using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServingFresh.Models.Interfaces;
using Xamarin.Forms;
using static ServingFresh.Views.SelectionPage;
namespace ServingFresh.Views
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
            SelectionPage.SetMenu(guestMenuSection, customerMenuSection, historyLabel, profileLabel);
            SetAppVersion(versionNumber, buildNumber);
            SetCartLabel(CartTotal);
        }

        void SetAppVersion(Label version, Label build)
        {
            string versionStr = "";
            string buildStr = "";

            versionStr = DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
            buildStr = DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();

            version.Text = "Running App version: " + versionStr;
            build.Text = "Running App build: " + buildStr;
        }

        void ShowMenuFromInfo(System.Object sender, System.EventArgs e)
        {
            var height = new GridLength(0);
            if (menuFrame.Height.Equals(height))
            {
                menuFrame.Height = this.Height - 180;
            }
            else
            {
                menuFrame.Height = 0;
            }
        }

        void NavigateToCartFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender, e);
        }

        void NavigateToStoreFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToStore(sender, e);
        }

        void NavigateToHistoryFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToHistory(sender, e);
        }

        void NavigateToRefundsFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToRefunds(sender, e);
        }

        void NavigateToInfoFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToInfo(sender, e);
        }

        void NavigateToProfileFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToProfile(sender, e);
        }

        void NavigateToSignInFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToSignIn(sender, e);
        }

        void NavigateToSignUpFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToSignUp(sender, e);
        }

        void NavigateToMainFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToMain(sender, e);
        }
    }
}
