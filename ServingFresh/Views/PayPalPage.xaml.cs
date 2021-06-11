using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServingFresh.Models;
using Xamarin.Forms;
using static ServingFresh.Views.CheckoutPage;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.Views.PrincipalPage;
using static ServingFresh.App;
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
            try
            {
                var source = webView.Source as UrlWebViewSource;
                Debug.WriteLine("WEBVIEW SOURCE: " + source.Url);
                if (source.Url.Contains("https://servingfresh.me/"))
                {
                    if (user.getUserType() == "GUEST")
                    {
                        Debug.WriteLine("SUCCESSFULL REDIRECT FROM PAYPAL TO SF WEB TO MOBILE APP");
                        purchase.setPurchasePaymentType("PAYPAL");
                        var client1 = new SignIn();
                        var isEmailUnused = await client1.ValidateExistingAccountFromEmail(purchase.getPurchaseEmail());
                        if (isEmailUnused == null)
                        {
                            var userID = await SignUp.SignUpNewUser(SignUp.GetUserFrom(purchase));
                            if (userID != "") {
                                user.setUserID(userID);
                                purchase.setPurchaseCustomerUID(userID); }
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
                                if (messageList != null)
                                {
                                    if (messageList.ContainsKey("701-000069"))
                                    {
                                        await DisplayAlert(messageList["701-000069"].title, messageList["701-000069"].message, messageList["701-000069"].responses);
                                    }
                                    else
                                    {
                                        await DisplayAlert("Issue with payment via PayPal", "", "OK");
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Issue with payment via PayPal", "", "OK");
                                }
                               
                            }
                        }
                        else
                        {
                            if (isEmailUnused.result.Count != 0)
                            {
                                var role = isEmailUnused.result[0].role;
                                if (role == "CUSTOMER")
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000070"))
                                        {
                                            await DisplayAlert(messageList["701-000070"].title, messageList["701-000070"].message, messageList["701-000070"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oops", "You are not a guest. We are sending you to the checkout page where you can sign in to proceed with your purchase", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "You are not a guest. We are sending you to the checkout page where you can sign in to proceed with your purchase", "OK");
                                    }
                                    
                                    await Application.Current.MainPage.Navigation.PopModalAsync();
                                    await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94, "1"), true);
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
                                        if (messageList != null)
                                        {
                                            if (messageList.ContainsKey("701-000071"))
                                            {
                                                await DisplayAlert(messageList["701-000071"].title, messageList["701-000071"].message, messageList["701-000071"].responses);
                                            }
                                            else
                                            {
                                                await DisplayAlert("Issue with payment via PayPal", "", "OK");
                                            }
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
                    else if (user.getUserType() == "CUSTOMER")
                    {
                        var paymentIsSuccessful = await paymentClient.captureOrder(paymentClient.getTransactionID());
                        await WriteFavorites(GetFavoritesList(), user.getUserID());
                        if (paymentIsSuccessful)
                        {
                            purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                            purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                            _ = paymentClient.SendPurchaseToDatabase(purchase);
                            order.Clear();
                            Application.Current.MainPage = new HistoryPage();
                        }
                        else
                        {
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000072"))
                                {
                                    await DisplayAlert(messageList["701-000072"].title, messageList["701-000072"].message, messageList["701-000072"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Issue with payment via PayPal", "", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Issue with payment via PayPal", "", "OK");
                            }
                        }
                    }
                }
            }catch(Exception errorWebViewPage)
            {
                var client = new Diagnostic();
                client.parseException(errorWebViewPage.ToString(), user);
            }
        }
    }
}
