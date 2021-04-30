﻿using System;
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
namespace ServingFresh.Views
{
    public partial class HistoryPage : ContentPage
    {

        public class HistoryObject
        {
            public string purchase_uid { get; set; }
            public string purchase_date { get; set; }
            public string purchase_id { get; set; }
            public string purchase_status { get; set; }
            public string pur_customer_uid { get; set; }
            public string pur_business_uid { get; set; }
            public string items { get; set; }
            public string order_instructions { get; set; }
            public string delivery_instructions { get; set; }
            public string order_type { get; set; }
            public string delivery_first_name { get; set; }
            public string delivery_last_name { get; set; }
            public string delivery_phone_num { get; set; }
            public string delivery_email { get; set; }
            public string delivery_address { get; set; }
            public string delivery_unit { get; set; }
            public string delivery_city { get; set; }
            public string delivery_state { get; set; }
            public string delivery_zip { get; set; }
            public string delivery_latitude { get; set; }
            public string delivery_longitude { get; set; }
            public string purchase_notes { get; set; }
            public string delivery_status { get; set; }
            public int feedback_rating { get; set; }
            public object feedback_notes { get; set; }
            public string payment_uid { get; set; }
            public string payment_id { get; set; }
            public string pay_purchase_uid { get; set; }
            public string pay_purchase_id { get; set; }
            public string payment_time_stamp { get; set; }
            public string start_delivery_date { get; set; }
            public string pay_coupon_id { get; set; }
            public double amount_due { get; set; }
            public double amount_discount { get; set; }
            public double subtotal { get; set; }
            public double service_fee { get; set; }
            public double delivery_fee { get; set; }
            public double driver_tip { get; set; }
            public double taxes { get; set; }
            public double amount_paid { get; set; }
            public string info_is_Addon { get; set; }
            public string cc_num { get; set; }
            public string cc_exp_date { get; set; }
            public string cc_cvv { get; set; }
            public string cc_zip { get; set; }
            public string charge_id { get; set; }
            public string payment_type { get; set; }
        }

        public class HistoryResponse
        {
            public string message { get; set; }
            public int code { get; set; }
            public IList<HistoryObject> result { get; set; }
            public string sql { get; set; }
        }

        public class HistoryItemObject
        {
            public string img { get; set; }
            public string unit { get; set; }
            public string qty { get; set; }
            public string name { get; set; }
            public string price { get; set; }
            public string item_uid { get; set; }
            public string itm_business_uid { get; set; }

            public string namePriceUnit
            {
                get
                {
                    return "(" + unit  +")";
                }
            }

            public string total_price
            {
                get
                {
                    return "$" + (Double.Parse(qty) * Double.Parse(price)).ToString("N2");
                }
            }
        }

        public class HistoryDisplayObject
        {
            public ObservableCollection<HistoryItemObject> items { get; set; }
            public string delivery_date { get; set; }
            public int itemsHeight { get; set; }
            public string purchase_status { get; set; }
            public string purchase_id { get; set; }
            public string purchase_date { get; set; }
            public string subtotal { get; set; }
            public string promo_applied { get; set; }
            public string delivery_fee { get; set; }
            public string service_fee { get; set; }
            public string driver_tip { get; set; }
            public string taxes { get; set; }
            public string total { get; set; }
            public string coupon_id { get; set; }
        }
                   

        public ObservableCollection<HistoryDisplayObject> historyList;

        public HistoryPage()
        {
            InitializeComponent();
            SelectionPage.SetMenu(guestMenuSection, customerMenuSection, historyLabel, profileLabel);
            historyList = new ObservableCollection<HistoryDisplayObject>();
            CartTotal.Text = CheckoutPage.total_qty.ToString();
            LoadHistory();
        }

        public async void ShowSuccessfullPayment()
        {
            await DisplayAlert("Congratulations", "Payment was successful. We appreciate your business", "OK");
        }

        public async void LoadHistory()
        {
            string userId = user.getUserID();
            var client = new HttpClient();
            var response = await client.GetAsync(Constant.GetHistoryUrl + userId);
            Debug.WriteLine("HISTORY: " + Constant.GetHistoryUrl + userId);
            string result = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(result);
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
                    var deliveryStatus = "";
                    var couponApplied = "None";
                    if(ho.subtotal != null)
                    {
                        subtotal = ho.subtotal;
                    }
                    if(ho.amount_discount != null)
                    {
                        promo_applied = ho.amount_discount;
                    }
                    if(ho.delivery_fee != null)
                    {
                        delivery_fee = ho.delivery_fee;
                    }
                    if(ho.service_fee != null)
                    {
                        service_fee = ho.service_fee;
                    }
                    if(ho.taxes != null)
                    {
                        taxes = ho.taxes;
                    }
                    if(ho.amount_paid != null)
                    {
                        total = ho.amount_paid;
                    }
                    if(ho.driver_tip != null)
                    {
                        driver_tip = ho.driver_tip;
                    }
                    if(ho.start_delivery_date != null)
                    {
                        date = DateTime.Parse(ho.start_delivery_date).ToString("MMM dd, yyyy") + " at " + DateTime.Parse(ho.start_delivery_date).ToString("hh:mm tt");
                    }

                    if(ho.delivery_status != null && ho.delivery_status == "TRUE")
                    {
                        deliveryStatus = "Delivered";
                    }else if(ho.delivery_status != null && ho.delivery_status == "FALSE")
                    {
                        deliveryStatus = "Confirmed";
                    }

                    if(ho.pay_coupon_id != null)
                    {
                        if(ho.pay_coupon_id != "")
                        {
                            couponApplied = ho.pay_coupon_id;
                        }
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
                        purchase_status = "Order " + deliveryStatus,
                        subtotal = "$" + subtotal.ToString("N2"),
                        promo_applied = "$" + promo_applied.ToString("N2"),
                        delivery_fee = "$" + delivery_fee.ToString("N2"),
                        service_fee = "$" + service_fee.ToString("N2"),
                        driver_tip = "$" + driver_tip.ToString("N2"),
                        taxes = "$" + taxes.ToString("N2"),
                        total = "$" + total.ToString("N2"),

                    }) ;
                }
            }
            catch (Exception history)
            {
                Debug.WriteLine(history.Message);
                
            }

            HistoryList.ItemsSource = historyList;
        }

        void ShowMenuFromHistory(System.Object sender, System.EventArgs e)
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

        void NavigateToCartFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender,e);
        }

        void NavigateToRefundsFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToRefunds(sender, e);
        }

        void NavigateToStoreFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToStore(sender, e);
        }

        void NavigateToInfoFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToInfo(sender, e);
        }

        void NavigateToProfileFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToProfile(sender, e);
        }

        void NavigateToSignInFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToSignIn(sender, e);
        }

        void NavigateToSignUpFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToSignUp(sender, e);
        }

        void NavigateToMainFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToMain(sender, e);
        }

        void NavigateToHistoryFromHistory(System.Object sender, System.EventArgs e)
        {
            NavigateToHistory(sender, e);
        }
    }
}
