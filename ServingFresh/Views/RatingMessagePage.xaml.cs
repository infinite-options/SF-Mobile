using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class RatingMessagePage : ContentPage
    {
        public RatingMessagePage(bool ratingLevel)
        {
            InitializeComponent();
            if (ratingLevel)
            {
                UIGreatRating.IsVisible = true;
            }
            else
            {
                UIPoorRating.IsVisible = true;
            }
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync(true);
            Navigation.PopModalAsync(true);
            //Navigation.PopToRootAsync(false);
            //Application.Current.MainPage = new HistoryPage();
        }

        async void SendUserToReviewLinks(System.Object sender, System.EventArgs e)
        {
            var button = (ImageButton)sender;
            Debug.WriteLine(button.Source.ToString());
            string url = "";
            if (button.Source.ToString().Contains("facebookReviewIcon"))
            {
                url = "https://www.facebook.com/ServingFresh/reviews";
            }
            else if (button.Source.ToString().Contains("googleReviewIcon"))
            {
                url = "https://g.page/r/CWwV02OoSKgzEAg/review";
            }

            try
            {
                await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            }
            catch
            {
                // An unexpected error occured. No browser may be installed on the device.
            }
        }

        void CloseRatingModal(System.Object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
            Navigation.PopModalAsync();

            //Application.Current.MainPage = new HistoryPage();
        }
    }
}
