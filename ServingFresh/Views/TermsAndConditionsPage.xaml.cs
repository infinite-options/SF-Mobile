using System;
using System.Collections.Generic;
using ServingFresh.Config;
using ServingFresh.Models;
using Xamarin.Forms;
using static ServingFresh.Views.PrincipalPage;
namespace ServingFresh.Views
{
    public partial class TermsAndConditionsPage : ContentPage
    {
        public TermsAndConditionsPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("AB000000");

            try
            {
                webView.Source = Constant.TermsAndConditions;
            }catch (Exception errorSettingTermsAndConditions)
            {
                errorMessage.IsVisible = true;
                webView.Source = Constant.ErrorPage;
                var client = new Diagnostic();
                client.parseException(errorSettingTermsAndConditions.ToString(), user);
            }
        }

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}
