using System;
using System.Collections.Generic;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.Views.PrincipalPage;
using static ServingFresh.Views.CheckoutPage;
using Xamarin.Forms;
using ServingFresh.Models;
using System.Collections.ObjectModel;

namespace ServingFresh.Views
{
    public partial class GiftCardPage : ContentPage
    {

        double giftCardAmount = 10;
        public GiftCardPage()
        {
            InitializeComponent();
            SetCartLabel(CartTotal);
            if(user.getUserType() == "CUSTOMER")
            {
                fromSection.IsVisible = false;
                purchase = new Purchase(user);
                purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                purchase.setPurchaseDeliveryDate("2021-05-26 10:00:00");
                purchase.setPurchaseCoupoID("");
                purchase.setPurchaseDiscount("0.00");
                purchase.setPurchaseAddon("FALSE");
                purchase.setPurchaseSubtotal(giftCardAmount.ToString("N2"));
                purchase.setPurchaseServiceFee("0.00");
                purchase.setPurchaseDeliveryFee("0.00");
                purchase.setPurchaseDriveTip("0.00");
                purchase.setPurchaseTaxes("0.00");
                purchase.setAmbassadorCode("0.00");
                
            }
        }

        void ShowMenuFromInfo(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new MenuPage(), true);
        }

        void CheckoutViaStripe(System.Object sender, System.EventArgs e)
        {
            if (AreTermsAccepted.IsChecked)
            {
                ObservableCollection<PurchasedItem> purchasedOrder = new ObservableCollection<PurchasedItem>();
                purchasedOrder.Add(new PurchasedItem
                {
                    img = "giftCardA",
                    qty = 1,
                    name = "Serving Fresh eGift card",
                    unit = "each",
                    price = giftCardAmount,
                    item_uid = "",
                    itm_business_uid = "",
                    description = "",
                    business_price = giftCardAmount,
                });

                purchase.setPurchaseItems(purchasedOrder);
                purchase.setPurchaseDeliveryInstructions(message.Text);
                purchase.setPurchaseAmountDue(giftCardAmount.ToString("N2"));
                purchase.setPurchasePaid(giftCardAmount.ToString("N2"));
                purchase.setPurchaseChargeID("test");
                purchase.setPurchasePaymentType("STRIPE");

                purchase.printPurchase();
                var paymentClient = new Payments("SFTEST");
                _ = paymentClient.SendPurchaseToDatabase(purchase);
            }
        }

        async void CheckoutViaPayPal(System.Object sender, System.EventArgs e)
        {
            if (AreTermsAccepted.IsChecked)
            {
                ObservableCollection<PurchasedItem> purchasedOrder = new ObservableCollection<PurchasedItem>();
                purchasedOrder.Add(new PurchasedItem
                {
                    img = "giftCardA",
                    qty = 1,
                    name = "Serving Fresh eGift card",
                    unit = "each",
                    price = giftCardAmount,
                    item_uid = "",
                    itm_business_uid = "",
                    description = "",
                    business_price = giftCardAmount,
                });

                purchase.setPurchaseItems(purchasedOrder);
                purchase.setPurchaseDeliveryInstructions(message.Text);
                purchase.setPurchaseAmountDue(giftCardAmount.ToString("N2"));
                purchase.setPurchasePaid(giftCardAmount.ToString("N2"));
                purchase.setPurchaseChargeID("test");
                purchase.setPurchasePaymentType("PAYPAL");
                await Application.Current.MainPage.Navigation.PushModalAsync(new PayPalPage(), true);
            }
            else
            {

            }
        }

        void SelectGiftCardCreditAmount(System.Object sender, System.EventArgs e)
        {
            foreach (View child in giftCardAmountOptions.Children)
            {
                var viewButton = (Button)child;
                viewButton.TextColor = Color.Black;
                viewButton.BackgroundColor = Color.White;
            }

            var button = (Button)sender;
            var amount = Double.Parse(button.ClassId);

            giftCardAmount = amount;

            button.TextColor = Color.White;
            button.BackgroundColor = Color.FromHex("#FF8500");
            giftCardCreditAmount.Text = "$ " + amount.ToString("N2");
        }

        async void ValidateAmountGiftCardAmountEntered(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            if (!String.IsNullOrEmpty(userGiftCardAmount.Text))
            {
                if (IsValidAmount(userGiftCardAmount.Text))
                {
                    var doubleAmount = Double.Parse(userGiftCardAmount.Text);
                    giftCardAmount = doubleAmount;
                    giftCardCreditAmount.Text = "$ " + giftCardAmount.ToString("N2");
                }
                else
                {
                    await DisplayAlert("Oops","The value you enter is invalid.","OK");
                }
            }
        }

        bool IsValidAmount(string amount)
        {
            // check that amount has zero or 1 dot
            var numOfDots = 0;
            foreach(char i in amount.ToCharArray())
            {
                if(i == '.')
                {
                    numOfDots++;
                }
            }

            if(numOfDots == 0 || numOfDots == 1)
            {
                foreach (char i in amount.ToCharArray())
                {
                    if(i != '.')
                    {
                        if (i < '0' || i > '9')
                        {
                            return false;
                        }
                    }
                }

                var doubleAmount = Double.Parse(amount);
                if(doubleAmount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
