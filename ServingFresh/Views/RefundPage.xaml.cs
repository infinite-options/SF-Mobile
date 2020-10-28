using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class RefundPage : ContentPage
    {
        public RefundPage()
        {
            InitializeComponent();
        }
        public void onTap(object sender, EventArgs e)
        {

        }
        public void openCheckout(object sender, EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage();
        }
        public void openHistory(object sender, EventArgs e)
        {
            Application.Current.MainPage = new HistoryPage();
        }

        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage();
        }

        void InfoClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new InfoPage();
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new ProfilePage();
        }
    }
}
