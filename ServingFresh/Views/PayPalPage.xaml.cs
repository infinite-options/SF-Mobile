using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServingFresh.Models;
using Xamarin.Forms;
using static ServingFresh.Views.CheckoutPage;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.Views.PrincipalPage;
namespace ServingFresh.Views
{
    public partial class PayPalPage : ContentPage
    {
        private Payments paymentClient = null;
        public PayPalPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("AB000000");
            var sender = new System.Object();
            var e = new System.EventArgs();
            CheckoutWithPayPal(sender, e);
        }

        async void CheckoutWithPayPal(System.Object sender, System.EventArgs e)
        {
            
            string mode = Payments.getMode(purchase.getPurchaseDeliveryInstructions(), "PAYPAL");
            paymentClient = new Payments(mode);
            webView.Source = await paymentClient.PayViaPayPal(purchase.getPurchaseAmountDue());
            webView.Navigated += WebViewPage_Navigated;
        }

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }

        private async void WebViewPage_Navigated(object sender, WebNavigatedEventArgs e)
        {
            var source = webView.Source as UrlWebViewSource;
            Debug.WriteLine("WEBVIEW SOURCE: " + source.Url);
            if (source.Url.Contains("https://servingfresh.me/"))
            {
               
                Debug.WriteLine("SUCCESSFULL REDIRECT FROM PAYPAL TO SF WEB TO MOBILE APP");
                purchase.setPurchasePaymentType("PAYPAL");
                var client1 = new SignIn();
                var isEmailUnused = await client1.ValidateExistingAccountFromEmail(purchase.getPurchaseEmail());
                if (isEmailUnused == null)
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
                            await Application.Current.MainPage.Navigation.PopModalAsync();
                            await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94), true);
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
    }
}
