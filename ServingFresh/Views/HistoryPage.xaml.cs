using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using Acr.UserDialogs;
using Newtonsoft.Json;
using ServingFresh.Config;
using Xamarin.Forms;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.Views.PrincipalPage;
using ServingFresh.Models;
using static ServingFresh.App;
using System.ComponentModel;

namespace ServingFresh.Views
{
    public partial class HistoryPage : ContentPage 
    {   // Class attributes
        public ObservableCollection<HistoryDisplayObject> historyList;

        // Constructor
        public HistoryPage()
        {
            InitializeComponent();

            historyList = new ObservableCollection<HistoryDisplayObject>();
            CartTotal.Text = CheckoutPage.total_qty.ToString();
            LoadHistory();
        }

        // This function calls the history endpoint to process its data and display it in the history page
        async void LoadHistory()
        {
            var userId = user.getUserID();
            var client = new HttpClient();
            var response = await client.GetAsync(Constant.GetHistoryUrl + userId);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<HistoryResponse>(result);
                    foreach (HistoryObject ho in data.result)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<HistoryItemObject>>(ho.items);
                        var date = "";
                        var subtotal = 0.0;
                        var promo_applied = 0.0;
                        var delivery_fee = 0.0;
                        var service_fee = 0.0;
                        var driver_tip = 0.0;
                        var taxes = 0.0;
                        var total = 0.0;
                        var ambassador_code = 0.0;
                        var deliveryStatus = "";
                        var couponApplied = "None";

                        if (ho.subtotal != null)
                        {
                            subtotal = ho.subtotal;
                        }
                        if (ho.amount_discount != null)
                        {
                            promo_applied = ho.amount_discount;
                        }
                        if (ho.delivery_fee != null)
                        {
                            delivery_fee = ho.delivery_fee;
                        }
                        if (ho.service_fee != null)
                        {
                            service_fee = ho.service_fee;
                        }
                        if (ho.taxes != null)
                        {
                            taxes = ho.taxes;
                        }
                        if (ho.amount_paid != null)
                        {
                            total = ho.amount_paid;
                        }
                        if (ho.driver_tip != null)
                        {
                            driver_tip = ho.driver_tip;
                        }
                        if (ho.start_delivery_date != null)
                        {
                            date = DateTime.Parse(ho.start_delivery_date).ToString("MMM dd, yyyy") + " at " + DateTime.Parse(ho.start_delivery_date).ToString("hh:mm tt");
                        }

                        if (ho.delivery_status != null && ho.delivery_status == "TRUE")
                        {
                            deliveryStatus = "Delivered";
                        }
                        else if (ho.delivery_status != null && ho.delivery_status == "FALSE")
                        {
                            deliveryStatus = "Confirmed";
                        }

                        if (ho.pay_coupon_id != null)
                        {
                            if (ho.pay_coupon_id != "")
                            {
                                couponApplied = ho.pay_coupon_id;
                            }
                        }

                        if (ho.ambassador_code != null)
                        {
                            ambassador_code = ho.ambassador_code;
                        }

                        DateTime today = DateTime.Parse(ho.purchase_date);

                        var localPurchaseDate = today.ToLocalTime();


                        historyList.Add(new HistoryDisplayObject()
                        {
                            items = items,

                            itemsHeight = 55 * items.Count,
                            delivery_date = date,
                            purchase_date = "Purchase Date: " + localPurchaseDate,
                            coupon_id = "Coupon ID: " + couponApplied,
                            purchase_id = "Order #" + ho.purchase_uid,
                            original_purchase_id = ho.purchase_uid,
                            purchase_status = "Order " + deliveryStatus,
                            subtotal = "$" + subtotal.ToString("N2"),
                            promo_applied = "$" + promo_applied.ToString("N2"),
                            delivery_fee = "$" + delivery_fee.ToString("N2"),
                            service_fee = "$" + service_fee.ToString("N2"),
                            driver_tip = "$" + driver_tip.ToString("N2"),
                            taxes = "$" + taxes.ToString("N2"),
                            total = "$" + total.ToString("N2"),
                            ambassador_code = "$" + ambassador_code.ToString("N2"),
                            isRateOrderButtonAvailable = EvaluateRatingState(ho.feedback_rating),
                            isRateIconAvailable = !EvaluateRatingState(ho.feedback_rating),
                            ratingSourceIcon = SetIcon(ho.feedback_rating),

                        });

                    }
                }
                catch (Exception errorLoadHistory)
                {
                    var client1 = new Diagnostic();
                    client1.parseException(errorLoadHistory.ToString(), user);
                    historyMessage.IsVisible = true;
                    historyMessage.Text = "Unfortunately, there was an error retrieving your history. Please check again later.";
                }
            }

            if (historyList.Count != 0)
            {
                HistoryList.ItemsSource = historyList;
                HistoryList.IsVisible = true;
            }
            else
            {
                historyMessage.Text = "You have no history at the moment.";
                historyMessage.IsVisible = true;
            }
        }

        // This function evalutes the state of the rating button based on the rating value
        bool EvaluateRatingState(int ratingValue)
        {
            bool result = false;
            if(ratingValue == -1)
            {
                result = true;
            }
            return result;
        }

        // This function return star icon based on the rating value
        string SetIcon(int ratingValue)
        {
            string result = "";

            if(ratingValue == 0)
            {
                result = "emptyStar";
            }
            else if (ratingValue == 1)
            {
                result = "oneStarRating";
            }
            else if (ratingValue == 2)
            {
                result = "twoStarRating";
            }
            else if (ratingValue == 3)
            {
                result = "threeStarRating";
            }
            else if (ratingValue == 4)
            {
                result = "fourStarRating";
            }
            else if (ratingValue == 5)
            {
                result = "fiveStarRating";
            }

            return result;
        }

        // NAVIGATION EVENT HANDLERS ___________________________________________

        //This function process a HistoryItemObject and creates a new order from previoud purchase items.
        async void Reorder(System.Object sender, System.EventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var purchase = (HistoryDisplayObject)button.CommandParameter;

                Debug.WriteLine("ORDER ID: " + purchase.purchase_id);
                Debug.WriteLine("ORDER ID: " + purchase.items);
                foreach (HistoryItemObject item in purchase.items)
                {
                    if (order.ContainsKey(item.name))
                    {
                        var itemToUpdate = order[item.name];
                        itemToUpdate.item_quantity += Int16.Parse(item.qty);
                        order[item.name] = itemToUpdate;
                    }
                    else
                    {
                        var itemToAdd = new ItemPurchased();
                        itemToAdd.pur_business_uid = item.itm_business_uid;
                        itemToAdd.item_uid = item.item_uid;
                        itemToAdd.item_name = item.name;
                        itemToAdd.item_quantity = Int16.Parse(item.qty);
                        itemToAdd.item_price = Double.Parse(item.price);
                        itemToAdd.img = item.img;
                        itemToAdd.unit = item.unit;
                        itemToAdd.description = item.description;
                        itemToAdd.business_price = Double.Parse(item.business_price);
                        itemToAdd.taxable = "FALSE";
                        itemToAdd.isItemAvailable = false;
                        order.Add(item.name, itemToAdd);
                    }
                }

                if (messageList != null)
                {
                    if (messageList.ContainsKey("701-000070"))
                    {
                        await DisplayAlert(messageList["701-000070"].title, messageList["701-000070"].message, messageList["701-000070"].responses);
                    }
                    else
                    {
                        await DisplayAlert("EZ Reorder", "Great! We’ve added everything that is in season to your cart.", "Continue");
                    }
                }
                else
                {
                    await DisplayAlert("EZ Reorder", "Great! We’ve added everything that is in season to your cart.", "Continue");
                }


                var client = new SelectionPage();

                client.CheckOutClickBusinessPage(new System.Object(), new System.EventArgs());
            }
            catch (Exception errorReordering)
            {
                if (messageList != null)
                {
                    if (messageList.ContainsKey("701-000071"))
                    {
                        await DisplayAlert(messageList["701-000071"].title, messageList["701-000071"].message, messageList["701-000071"].responses);
                    }
                    else
                    {
                        await DisplayAlert("Oops", "Not able to add order to your cart. Please try again.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "Not able to add order to your cart. Please try again.", "OK");
                }
                var client = new Diagnostic();
                client.parseException(errorReordering.ToString(), user);
            }
        }

        // This function navigates to from history page to rate order page (modally)
        void RateOrderButton(System.Object sender, System.EventArgs e)
        {
            if (sender.ToString() == "Xamarin.Forms.Button")
            {
                var button = (Button)sender;
                var objectSelected = (HistoryDisplayObject)button.CommandParameter;

                Navigation.PushModalAsync(new RateOrderPage(objectSelected), true);
            }
            else
            {
                var button = (ImageButton)sender;
                var objectSelected = (HistoryDisplayObject)button.CommandParameter;

                Navigation.PushModalAsync(new RateOrderPage(objectSelected), true);
            }
        }

        // This function is call when user wants to open and see app menu
        void ShowMenuFromHistory(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new MenuPage(), true);
        }

        // This function navigates from the history page to the checkout page
        void NavigateToCartFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender,e);
        }

        // This function navigates from history page to the refunds page
        void NavigateToRefundsFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToRefunds(sender, e);
        }

        // NAVIGATION EVENT HANDLERS ___________________________________________
    }
}
