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

        public string GetAppVersion()
        {
            string versionStr = "";
            string buildStr = "";

            versionStr = DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
            buildStr = DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();

            return versionStr + ", " + buildStr;
        }

        void ShowMenuFromInfo(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new MenuPage(), true);
        }

        void NavigateToCartFromInfo(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender, e);
        }
    }
}
