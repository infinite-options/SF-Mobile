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
using static ServingFresh.Views.PrincipalPage;
using Application = Xamarin.Forms.Application;


namespace ServingFresh.Views
{
    public partial class DeliveryDetailsPage : ContentPage
    {

        private Payments paymentClient = null;

        public DeliveryDetailsPage()
        {
            InitializeComponent();
            SelectionPage.SetMenu(guestMenuSection, customerMenuSection, historyLabel, profileLabel);

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

                SetNextSteps(purchaseProcess, "Complete Payment");
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

                string mode = Payments.getMode(purchase.getPurchaseDeliveryInstructions(), "STRIPE");
                paymentClient = new Payments(mode);

                var isEmailUnused = await ValidateExistingAccountFromEmail(purchase.getPurchaseEmail());
                if(isEmailUnused == null)
                {
                    var userID = await SignUp.SignUpNewUser(SignUp.GetUserFrom(purchase));
                    if (userID != "")
                    {
                        purchase.setPurchaseCustomerUID(userID);
                        var paymentIsSuccessful = paymentClient.PayViaStripe(
                            purchase.getPurchaseEmail(),
                            cardHolderName.Text,
                            cardHolderNumber.Text,
                            cardCVV.Text,
                            cardExpMonth.Text,
                            cardExpYear.Text,
                            purchase.getPurchaseAmountDue()
                            );

                        await WriteFavorites(GetFavoritesList(), userID);

                        if (paymentIsSuccessful)
                        {
                            purchase.setPurchaseZipcode("95120");
                            purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                            purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                            purchase.printPurchase();
                            _ = paymentClient.SendPurchaseToDatabase(purchase);
                            order.Clear();
                            Application.Current.MainPage = new ConfirmationPage();
                        }
                        else
                        {
                            await DisplayAlert("Make sure you fill all entries", "", "OK");
                        }
                    }
                }
                else
                {
                    if(isEmailUnused.result.Count != 0)
                    {
                        var role = isEmailUnused.result[0].role;
                        if(role == "CUSTOMER")
                        {
                            await DisplayAlert("Oops", "You are not a guest. We are sending you to the checkout page where you can sign in to proceed with your purchase", "OK");
                            Application.Current.MainPage = new CheckoutPage();
                        }
                        else if (role == "GUEST")
                        {
                            // we don't sign up but get user id
                            user.setUserID(isEmailUnused.result[0].customer_uid);
                            purchase.setPurchaseCustomerUID(user.getUserID());
                            user.setUserFromProfile(isEmailUnused);
                            var paymentIsSuccessful = paymentClient.PayViaStripe(
                                purchase.getPurchaseEmail(),
                                cardHolderName.Text,
                                cardHolderNumber.Text,
                                cardCVV.Text,
                                cardExpMonth.Text,
                                cardExpYear.Text,
                                purchase.getPurchaseAmountDue()
                                );

                            await WriteFavorites(GetFavoritesList(), user.getUserID());
                            if (paymentIsSuccessful)
                            {
                                purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                                purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                                _ = paymentClient.SendPurchaseToDatabase(purchase);
                                order.Clear();
                                Application.Current.MainPage = new ConfirmationPage();
                            }
                            else
                            {
                                await DisplayAlert("Make sure you fill all entries", "", "OK");
                            }
                        }
                    }
                }
            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
            }
        }

        public static async Task<UserProfile> ValidateExistingAccountFromEmail(string email)
        {
            UserProfile result = null;

            var client = new System.Net.Http.HttpClient();
            var endpointCall = await client.GetAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/email_info/" + email);

            
            if (endpointCall.IsSuccessStatusCode)
            {
                var endpointContent = await endpointCall.Content.ReadAsStringAsync();
                Debug.WriteLine("PROFILE: " + endpointContent);
                var profile = JsonConvert.DeserializeObject<UserProfile>(endpointContent);
                if(profile.result.Count != 0)
                {
                    result = profile;
                }
                
            }

            return result;
        }

        async void CheckoutWithPayPal(System.Object sender, System.EventArgs e)
        {
            paypalRow.Height = this.Height - 100;
            SetNextSteps(purchaseProcess, "Complete Payment");
            string mode = Payments.getMode(purchase.getPurchaseDeliveryInstructions(), "PAYPAL");
            paymentClient = new Payments(mode);
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
                purchase.setPurchasePaymentType("PAYPAL");

                var isEmailUnused = await ValidateExistingAccountFromEmail(purchase.getPurchaseEmail());
                if(isEmailUnused == null)
                {
                    var userID = await SignUp.SignUpNewUser(SignUp.GetUserFrom(purchase));
                    if (userID != "") { purchase.setPurchaseCustomerUID(userID); }
                    var paymentIsSuccessful = await paymentClient.captureOrder(paymentClient.getTransactionID());
                    await WriteFavorites(GetFavoritesList(), userID);
                    if (paymentIsSuccessful)
                    {
                        purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                        purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                        _ = paymentClient.SendPurchaseToDatabase(purchase);
                        order.Clear();
                        Application.Current.MainPage = new ConfirmationPage();
                    }
                    else
                    {
                        await DisplayAlert("Issue with payment via PayPal", "", "OK");
                    }
                }
                else
                {
                    if (isEmailUnused.result.Count != 0)
                    {
                        var role = isEmailUnused.result[0].role;
                        if (role == "CUSTOMER")
                        {
                            await DisplayAlert("Oops", "You are not a guest. We are sending you to the checkout page where you can sign in to proceed with your purchase", "OK");
                            Application.Current.MainPage = new CheckoutPage();
                        }
                        else if (role == "GUEST")
                        {
                            // we don't sign up but get user id
                            user.setUserID(isEmailUnused.result[0].customer_uid);
                            purchase.setPurchaseCustomerUID(user.getUserID());
                            user.setUserFromProfile(isEmailUnused);
                            var paymentIsSuccessful = await paymentClient.captureOrder(paymentClient.getTransactionID());
                            await WriteFavorites(GetFavoritesList(), user.getUserID());
                            if (paymentIsSuccessful)
                            {
                                purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                                purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                                _ = paymentClient.SendPurchaseToDatabase(purchase);
                                order.Clear();
                                Application.Current.MainPage = new ConfirmationPage();
                            }
                            else
                            {
                                await DisplayAlert("Issue with payment via PayPal", "", "OK");
                            }
                        }
                    }
                }
            }
        }

        void ShowMenuFromDeliveryDetails(System.Object sender, System.EventArgs e)
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

        void SetNextSteps(StackLayout layout, string step)
        {
            foreach(View child in layout.Children)
            {
                child.Opacity = 0.5;

                var payment = child as Label;
                if(payment.Text != step)
                {
                    child.Opacity = 0.5;
                }
                else
                {
                    child.Opacity = 1.0;
                }

            }
        }

        void NavigateToCartFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender, e);
        }


        void NavigateToStoreFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToStore(sender, e);
        }

        void NavigateToHistoryFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToHistory(sender, e);
        }

        void NavigateToRefundsFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToRefunds(sender, e);
        }

        void NavigateToInfoFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToInfo(sender, e);
        }

        void NavigateToProfileFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToProfile(sender, e);
        }

        void NavigateToSignInFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToSignIn(sender, e);
        }

        void NavigateToSignUpFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToSignUp(sender, e);
        }

        void NavigateToMainFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToMain(sender, e);
        }
    }
}
