using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using ServingFresh.Config;
using ServingFresh.LogIn.Classes;
using ServingFresh.Models;
using Stripe;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using static ServingFresh.Views.CheckoutPage;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.Views.SignUpPage;
using Application = Xamarin.Forms.Application;


namespace ServingFresh.Views
{
    public partial class DeliveryDetailsPage : ContentPage
    {

        private Payments paymentClient;

        public DeliveryDetailsPage()
        {
            InitializeComponent();
            paymentClient = new Payments(purchase.getPurchaseDeliveryInstructions());
            cartItemsNumber.Text = purchase.getPurchaseItems().Count.ToString();
            PlaceLocationOnMap(double.Parse(purchase.getPurchaseLatitude()), double.Parse(purchase.getPurchaseLatitude()));
        }

        void PlaceLocationOnMap(double latitude, double longitude)
        {
            Position position = new Position(latitude, longitude);
            map.MapType = MapType.Street;
            var mapSpan = new MapSpan(position, 0.001, 0.001);
            Pin address = new Pin();
            address.Label = "Delivery Address";
            address.Type = PinType.SearchResult;
            address.Position = position;
            map.MoveToRegion(mapSpan);
            map.Pins.Add(address);
        }

        void CheckoutWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
                stripeInformationView.HeightRequest = 194;

                purchase.setPurchaseFirstName(firstName.Text);
                purchase.setPurchaseLastName(lastName.Text);
                purchase.setPurchasePhoneNumber(phoneNumber.Text);
                purchase.setPurchaseEmail(emailAddress.Text);

            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
                stripeInformationView.HeightRequest = 0;
            }
        }



        async void CompletePaymentWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
                purchase.setPurchasePaymentType("STRIPE");

                var userID = await SignUpNewUser(GetUserFrom(purchase));
                if( userID != "") { purchase.setPurchaseCustomerUID(userID); }
                var paymentIsSuccessful = paymentClient.PayViaStripe(
                    purchase.getPurchaseEmail(),
                    cardHolderName.Text,
                    cardHolderNumber.Text,
                    cardCVV.Text,
                    cardExpMonth.Text,
                    cardExpYear.Text,
                    purchase.getPurchaseAmountDue()
                    ); ;

                await WriteFavorites(GetFavoritesList(),userID);

                if (paymentIsSuccessful)
                {
                    _ = paymentClient.SendPurchaseToDatabase(purchase);
                    Application.Current.MainPage = new ConfirmationPage();
                }
                else
                {
                    await DisplayAlert("Make sure you fill all entries", "", "OK");
                }
            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
            }
        }


        async void CheckoutWithPayPal(System.Object sender, System.EventArgs e)
        {
            paypalRow.Height = this.Height - 100;
            webView.Source = await paymentClient.PayViaPayPal(purchase.getPurchaseAmountDue());
            webView.Navigated += WebViewPage_Navigated;
        }



        private async void WebViewPage_Navigated(object sender, WebNavigatedEventArgs e)
        {
            var source = webView.Source as UrlWebViewSource;
            Debug.WriteLine("WEBVIEW SOURCE: " + source.Url);
            if (source.Url.Contains("https://servingfresh.me/"))
            {
                paypalRow.Height = 0;
                Debug.WriteLine("SUCCESSFULL REDIRECT FROM PAYPAL TO SF WEB TO MOBILE APP");
                purchase.setPurchasePaymentType("STRIPE");
                var userID = await SignUpNewUser(GetUserFrom(purchase));
                if (userID != "") { purchase.setPurchaseCustomerUID(userID); }
                var paymentIsSuccessful = await paymentClient.captureOrder(paymentClient.getTransactionID());
                await WriteFavorites(GetFavoritesList(), userID);
                if (paymentIsSuccessful)
                {
                    _ = paymentClient.SendPurchaseToDatabase(purchase);
                    Application.Current.MainPage = new ConfirmationPage();
                }
                else
                {
                    await DisplayAlert("Issue with payment via PayPal", "", "OK");
                }
            }
        }


        void NavigateToCartFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender, e);
        }
    }
}
