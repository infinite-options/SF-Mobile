using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ServingFresh.Config;
using ServingFresh.Models.Interfaces;
using Xamarin.Essentials;
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

        async void GoToPrivacyPolicy(System.Object sender, System.EventArgs e)
        {

            try
            {
                await Browser.OpenAsync(Constant.PrivacyPolicy, BrowserLaunchMode.SystemPreferred);
            }
            catch
            {
                // An unexpected error occured. No browser may be installed on the device.
            }
        }
    }
}
