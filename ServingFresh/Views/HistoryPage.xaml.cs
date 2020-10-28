using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Newtonsoft.Json;
using ServingFresh.Config;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class HistoryPage : ContentPage
    {
        public class HistoryItemObject
        {
            public string qty { get; set; }
            public string name { get; set; }
            public string price { get; set; }
            public string item_uid { get; set; }
            public string itm_business_uid { get; set; }
            public string total_price
            {
                get
                {
                    return "$" + (Double.Parse(qty) * Double.Parse(price)).ToString("N2");
                }
            }
        }
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
            public double amount_paid { get; set; }
            public string info_is_Addon { get; set; }
            public int cc_num { get; set; }
            public string cc_exp_date { get; set; }
            public string cc_cvv { get; set; }
            public string cc_zip { get; set; }
            public string charge_id { get; set; }
            public string payment_type { get; set; }
        }
        public class HistoryDisplayObject
        {
            public ObservableCollection<HistoryItemObject> items { get; set; }
            public int itemsHeight { get; set; }
            public string purchase_id { get; set; }
            public string purchase_date { get; set; }
            public string amount_due { get; set; }
        }
        public class HistoryResponse
        {
            public string message { get; set; }
            public int code { get; set; }
            public ObservableCollection<HistoryObject> result { get; set; }
            public string sql { get; set; }
        }
        public ObservableCollection<HistoryDisplayObject> historyList;

        public HistoryPage()
        {
            InitializeComponent();
            historyList = new ObservableCollection<HistoryDisplayObject>();
            LoadHistory();
        }
        public async void LoadHistory()
        {
            string email = (string)Application.Current.Properties["user_email"];
            var client = new HttpClient();
            var response = await client.GetAsync(Constant.GetHistoryUrl + email);
            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            var data = JsonConvert.DeserializeObject<HistoryResponse>(result);
            foreach (HistoryObject ho in data.result)
            {
                var items = JsonConvert.DeserializeObject<ObservableCollection<HistoryItemObject>>(ho.items);
                historyList.Add(new HistoryDisplayObject()
                {
                    items = items,
                    itemsHeight = 55 * items.Count,
                    purchase_date = ho.purchase_date,
                    purchase_id = "Order #" + ho.purchase_uid,
                    amount_due = "$" + ho.amount_due.ToString("N2")
                });
            }
            HistoryList.ItemsSource = historyList;
        }
        public void openCheckout(object sender, EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage();
        }
        public void openRefund(object sender, EventArgs e)
        {
            Application.Current.MainPage = new RefundPage();
        }
        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            // Already on orders page
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
