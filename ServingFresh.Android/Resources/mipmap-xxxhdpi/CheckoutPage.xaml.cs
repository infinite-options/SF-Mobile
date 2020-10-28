using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;

using static ServingFresh.Views.ItemsPage;

namespace ServingFresh.Views
{
    public class ItemObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string item_uid { get; set; }
        public string business_uid { get; set; }
        public string name { get; set; }
        public int qty { get; set; }
        public double price { get; set; }
        public string total_price { get { return "$ " + (qty * price).ToString("N2"); } }
        public void increase_qty()
        {
            qty++;
            PropertyChanged(this, new PropertyChangedEventArgs("qty"));
            PropertyChanged(this, new PropertyChangedEventArgs("total_price"));
        }
        public void decrease_qty()
        {
            if (qty == 0) return;
            qty--;
            PropertyChanged(this, new PropertyChangedEventArgs("qty"));
            PropertyChanged(this, new PropertyChangedEventArgs("total_price"));
        }
    }
    public class PurchaseDataObject
    {
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
        public string start_delivery_date { get; set; }
        public string pay_coupon_id { get; set; }
        public string amount_due { get; set; }
        public string amount_discount { get; set; }
        public string amount_paid { get; set; }
        public string info_is_Addon { get; set; }
        public string cc_num { get; set; }
        public string cc_exp_date { get; set; }
        public string cc_cvv { get; set; }
        public string cc_zip { get; set; }
        public string charge_id { get; set; }
        public string payment_type { get; set; }
    }
    public class PurchaseResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string sql { get; set; }
    }
    public partial class CheckoutPage : ContentPage
    {
        public PurchaseDataObject purchaseObject;
        public static ObservableCollection<ItemObject> cartItems = new ObservableCollection<ItemObject>();
        public double subtotal;
        public double discount;
        public double delivery_fee;
        public double taxes;
        public double total;
        public static int total_qty = 0;

        public CheckoutPage(IDictionary<string, ItemPurchased> order = null, string day = "")
        {
            InitializeComponent();

            if (order != null)
            {
                cartItems.Clear();
                foreach (string key in order.Keys)
                {
                    cartItems.Add(new ItemObject()
                    {
                        qty = order[key].item_quantity,
                        name = order[key].item_name,
                        price = order[key].item_price,
                        item_uid = order[key].item_uid,
                        business_uid = order[key].pur_business_uid
                    });
                }
            }
            purchaseObject = new PurchaseDataObject()
            {
                pur_customer_uid = Application.Current.Properties.ContainsKey("user_id") ? (string)Application.Current.Properties["user_id"] : "",
                pur_business_uid = "",
                items = "",
                order_instructions = "fast",
                delivery_instructions = "",
                order_type = "meal",
                delivery_first_name = (string)Application.Current.Properties["user_first_name"],
                delivery_last_name = (string)Application.Current.Properties["user_last_name"],
                delivery_phone_num = (string)Application.Current.Properties["user_phone_num"],
                delivery_email = (string)Application.Current.Properties["user_email"],
                delivery_address = (string)Application.Current.Properties["user_address"],
                delivery_unit = (string)Application.Current.Properties["user_unit"],
                delivery_city = (string)Application.Current.Properties["user_city"],
                delivery_state = (string)Application.Current.Properties["user_state"],
                delivery_zip = (string)Application.Current.Properties["user_zip_code"],
                delivery_latitude = (string)Application.Current.Properties["user_latitude"],
                delivery_longitude = (string)Application.Current.Properties["user_longitude"],
                purchase_notes = "purchase_notes"

            };
            DeliveryAddress1.Text = purchaseObject.delivery_address;
            DeliveryAddress2.Text = purchaseObject.delivery_city + ", "
                + purchaseObject.delivery_state + ", " + purchaseObject.delivery_zip;
            FullName.Text = purchaseObject.delivery_first_name + " " + purchaseObject.delivery_last_name;
            PhoneNumber.Text = purchaseObject.delivery_phone_num;
            EmailAddress.Text = purchaseObject.delivery_email;
            CartItems.ItemsSource = cartItems;
            CartItems.HeightRequest = 56 * cartItems.Count;
            updateTotals();
        }
        public void updateTotals()
        {
            subtotal = 0.0;
            total_qty = 0;
            foreach (ItemObject item in cartItems)
            {
                total_qty += item.qty;
                subtotal += (item.qty * item.price);
            }
            SubTotal.Text = "$ " + subtotal.ToString("N2");
            discount = subtotal * .1;
            Discount.Text = "-$ " + discount.ToString("N2");
            delivery_fee = 1.50;
            DeliveryFee.Text = "$ " + delivery_fee.ToString("N2");
            taxes = subtotal * 0.085;
            Taxes.Text = "$ " + taxes.ToString("N2");
            total = subtotal - discount + delivery_fee + taxes;
            GrandTotal.Text = "$ " + total.ToString("N2");

            CartTotal.Text = total_qty.ToString();
        }
        public string createItemJSON(ItemObject item)
        {
            return "{\"item_uid\": " + "\"" + item.item_uid + "\"," + "\"qty\": " + "\"" + item.qty +
             "\"," + "\"name\": " + "\"" + item.name + "\"," + "\"price\": " + "\"" + item.price +
             "\"," + "\"itm_business_uid\": " + "\"" + item.business_uid + "\"}";
        }
        public string createItemsJSON(IList<ItemObject> items)
        {
            string itemsJSON = "[";
            foreach (ItemObject item in items)
            {
                if (item.qty == 0) continue;
                itemsJSON += createItemJSON(item);
                if (items.IndexOf(item) != items.Count - 1) itemsJSON += ",";
            }
            itemsJSON += "]";
            return itemsJSON;
        }
        public void TestDateFormat(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            Console.WriteLine(now);
            Console.WriteLine(now.Year.ToString("D4") + "-" +
                              now.Month.ToString("D2") + "-" +
                              now.Day.ToString("D2") + " " +
                              now.Hour.ToString("D2") + ":" +
                              now.Minute.ToString("D2") + ":" +
                              now.Second.ToString("D2"));
        }
        public void checkoutAsync(object sender, EventArgs e)
        {

            cardframe.Height = this.Height / 2;

            purchaseObject.items = createItemsJSON(cartItems);
            purchaseObject.start_delivery_date = DateTime.Today.ToString();
            purchaseObject.pay_coupon_id = "";
            purchaseObject.amount_due = total.ToString("N2");
            purchaseObject.amount_discount = discount.ToString("N2");
            purchaseObject.amount_paid = total.ToString("N2");
            purchaseObject.info_is_Addon = "FALSE";

            // HERE I HAVE ASSIGNED THE INFORMATION WE WILL BE RECORDING FROM
            // USER'S CARD INFO
            purchaseObject.cc_num = cardNumber.Text;
            purchaseObject.cc_exp_date = cardExpMonth.Text + "/" + cardExpYear;
            purchaseObject.cc_cvv = cardCVV.Text;
            purchaseObject.cc_zip = cardZip.Text;


            purchaseObject.charge_id = "";
            purchaseObject.payment_type = ((Button)sender).Text == "Checkout with Paypal" ? "PAYPAL" : "STRIPE";

            //purchaseObject.items = createItemsJSON(cartItems);
            //purchaseObject.start_delivery_date = DateTime.Today.ToString();
            //purchaseObject.pay_coupon_id = "";
            //purchaseObject.amount_due = total.ToString("N2");
            //purchaseObject.amount_discount = discount.ToString("N2");
            //purchaseObject.amount_paid = total.ToString("N2");
            //purchaseObject.info_is_Addon = "FALSE";
            //purchaseObject.cc_num = "4545";
            //purchaseObject.cc_exp_date = DateTime.Today.AddYears(1).ToString();
            //purchaseObject.cc_cvv = "666";
            //purchaseObject.cc_zip = purchaseObject.delivery_zip;
            //purchaseObject.charge_id = "";
            //purchaseObject.payment_type = ((Button)sender).Text == "Checkout with Paypal" ? "PAYPAL" : "STRIPE";
            //var purchaseString = JsonConvert.SerializeObject(purchaseObject);
            //Console.WriteLine(purchaseString);
            //var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
            //var httpClient = new HttpClient();
            //var request = new HttpRequestMessage();
            //request.Method = HttpMethod.Post;
            //request.Content = purchaseMessage;
            //var httpResponse = await httpClient.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/purchase_Data_SF", purchaseMessage);
            //string responseStr = await httpResponse.Content.ReadAsStringAsync();
            //PurchaseResponse data = JsonConvert.DeserializeObject<PurchaseResponse>(responseStr);
            //Console.WriteLine(data.message);
        }

        // COMPLETE PAYMENT
        // I THINK THE FLOW OF PROCESSING A PAYMENT WITH STRIPE SHOULD BE
        // 1. SEND A JSON OBJECT WITH USERS CREDENTIALS AND AMOUNT
        // 2. RECEIVE A RESPONSE UP ON PROCESS
        // 3. IF THE RESPONSE IS PAID, THEN WRITE EVERYTHING ABOUT THE ORDER AND DELIVERY INFO TO DATABASE
        // 4. IF THE RESPONSE IS NOT PAID, THEN DO NOT WRITE THE ORDER TO THE BATABASE AND LET USER KNOW THAT ORDER WAS NOT SUCESSFUL

        // LASTLY, WE NEED TO MODIFY THE PURCHASE DATA SF JSON OBJECT

        // COMPLETE PAYMENT
        async void CompletePaymentClick(System.Object sender, System.EventArgs e)
        {
            var purchaseString = JsonConvert.SerializeObject(purchaseObject);
            Console.WriteLine(purchaseString);
            var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = purchaseMessage;

            //var httpResponse = await httpClient.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/purchase_Data_SF", purchaseMessage);
            //string responseStr = await httpResponse.Content.ReadAsStringAsync();
            //PurchaseResponse data = JsonConvert.DeserializeObject<PurchaseResponse>(responseStr);
            //Console.WriteLine(data.message);

            cardframe.Height = 0;
            await DisplayAlert("You have successfully paid your order!", "", "OK");
        }

        // CANCEL
        void CancelPaymentClick(System.Object sender, System.EventArgs e)
        {
            cardframe.Height = 0;
        }
        public void increase_qty(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            ItemObject item = (ItemObject)tgr.CommandParameter;
            if (item != null) item.increase_qty();
            Console.WriteLine(createItemsJSON((ObservableCollection<ItemObject>)CartItems.ItemsSource));
            updateTotals();
        }
        public void decrease_qty(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            ItemObject item = (ItemObject)tgr.CommandParameter;
            if (item != null) item.decrease_qty();
            Console.WriteLine(createItemsJSON((ObservableCollection<ItemObject>)CartItems.ItemsSource));
            updateTotals();
        }
        public void openHistory(object sender, EventArgs e)
        {
            Application.Current.MainPage = new HistoryPage();
        }

        public async void ChangeAddressClick(System.Object sender, System.EventArgs e)
        {
            await DisplayAlert("You want to change your address", "", "OK");
        }

        void DriverTip_Completed(System.Object sender, System.EventArgs e)
        {
            DriverTip.Focus();
        }

        public async void ChangeContactInfoClick(System.Object sender, System.EventArgs e)
        {
            await DisplayAlert("You want to change your contact info", "", "OK");
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
            //Application.Current.MainPage = new InfoPage();
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            //Application.Current.MainPage = new profileUser();
        }
    }
}
