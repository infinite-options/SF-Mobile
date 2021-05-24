using System;
using System.Collections.Generic;
using static ServingFresh.Views.SelectionPage;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class GiftCardPage : ContentPage
    {
        public GiftCardPage()
        {
            InitializeComponent();
            SetCartLabel(CartTotal);
        }

        void ShowMenuFromInfo(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new MenuPage(), true);
            //var height = new GridLength(0);
            //if (menuFrame.Height.Equals(height))
            //{
            //    menuFrame.Height = this.Height - 180;
            //}
            //else
            //{
            //    menuFrame.Height = 0;
            //}
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
           // var purchase = new 
        }
    }
}
