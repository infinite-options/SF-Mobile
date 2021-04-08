using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using ServingFresh.Config;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using ServingFresh.Models;
using System.Diagnostics;
using static ServingFresh.Views.ItemsPage;
using Application = Xamarin.Forms.Application;
using Stripe;
using ServingFresh.Effects;
using Xamarin.Essentials;

using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;

using System.Threading.Tasks;
using Acr.UserDialogs;
using static ServingFresh.Views.SelectionPage;

namespace ServingFresh.Views
{
    public class ItemObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public TextDecorations decorationType
        {
            get
            {
                if (isItemAvailable)
                {
                    return TextDecorations.None;
                }
                else
                {
                    return TextDecorations.Strikethrough;
                }
            }
        }
        public bool isItemAvailable { get; set; }
        public bool isUnavailableItem { get { return !isItemAvailable; } }
        public string description { get; set; }
        public double business_price { get; set; }
        public string item_uid { get; set; }
        public string business_uid { get; set; }
        public string name { get; set; }
        public string priceUnit { get; set; }
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

        public string img { get; set; }
        public string unit { get; set; }
        public string taxable { get; set; }

  


        // public string description { get; set;
        // business_price - double
    }

    public class User
    {
        public string delivery_instructions { get; set; }
    }

    public class InfoObject
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<User> result { get; set; }
        public string sql { get; set; }
    }

    public class PurchaseDataObject
    {
        public string pur_customer_uid { get; set; }
        public string pur_business_uid { get; set; }
        public ObservableCollection<PurchasedItem> items { get; set; }
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
        public string subtotal { get; set; }
        public string service_fee { get; set; }
        public string delivery_fee { get; set; }
        public string driver_tip { get; set; }
        public string taxes { get; set; }
    }
    public class PurchaseResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string sql { get; set; }
    }
    public partial class CheckoutPage : ContentPage
    {
        public class couponItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public string image { get; set; }
            public string couponNote { get; set; }
            public string thresholdNote { get; set; }
            public string expNote { get; set; }
            public string savingsOrSpendingNote { get; set; }
            public int index { get; set; }
            public string status { get; set; }
            public double threshold { get; set; }
            public double discount { get; set; }
            public double shipping { get; set; }
            public double totalDiscount { get; set; }
            public string couponId { get; set; }

            public void update()
            {
                PropertyChanged(this, new PropertyChangedEventArgs("image"));
            }
        }

        public class CouponObject
        {
            public string coupon_uid { get; set; }
        }

        public class Fee
        {
            public double service_fee { get; set; }
            public double tax_rate { get; set; }
            public double delivery_fee { get; set; }
            public string delivery_time { get; set; }
        }

        public class ZoneFees
        {
            public string message { get; set; }
            public int code { get; set; }
            public Fee result { get; set; }
            public string sql { get; set; }
        }

        public class Credentials
        {
            public string key { get;set; }
        }


        public PurchaseDataObject purchaseObject;
        public static ObservableCollection<ItemObject> cartItems = new ObservableCollection<ItemObject>();
        public ObservableCollection<couponItem> couponsList = new ObservableCollection<couponItem>();
        public ObservableCollection<couponItem> TempCouponsList = new ObservableCollection<couponItem>();
        public double subtotal;
        public double discount;
        public double delivery_fee_db = 0;
        public double delivery_fee = 0;
        public double service_fee = 0;
        public double taxes = 0;
        public double total = 0;
        public double driver_tips;
        public static int total_qty = 0;
        private bool isAddressValidated;
        private string latitude = "0";
        private string longitude = "0";

        // Coupons Lists
        private CouponResponse couponData = null;
        private List<double> unsortedNewTotals = new List<double>();
        private List<double> unsortedThresholds = new List<double>();
        private List<double> unsortedDiscounts = new List<double>();
        private List<double> sortedDiscounts = new List<double>();
        private int appliedIndex = -1;
        double savings = 0;
        public string deliveryDay = "";
        public IDictionary<string, ItemPurchased> orderCopy = new Dictionary<string,ItemPurchased>();
        public string cartEmpty = "";

        public ScheduleInfo deliveryInfo;
        // PAYPAL CREDENTIALS
        // =========================
        static string clientId = "";
        static string secret = "";
        static string mode = "";

        // =========================

        public CheckoutPage(IDictionary<string, ItemPurchased> order = null, ScheduleInfo deliveryInfo = null)
        {
            InitializeComponent();
            this.deliveryInfo = deliveryInfo;
            expectedDelivery.Text = deliveryInfo.deliveryTimeStamp.ToString("dddd, MMM dd, yyyy, ") + " between " + deliveryInfo.delivery_time;
            Application.Current.Properties["delivery_date"] = deliveryInfo.delivery_date;
            Application.Current.Properties["delivery_time"] = deliveryInfo.delivery_time;
            Application.Current.Properties["deliveryDate"] = deliveryInfo.deliveryTimeStamp;
            var day = deliveryInfo.delivery_dayofweek;
            GetPayPalCredentials();
            if(Device.RuntimePlatform == Device.Android)
            {
                DriverTip.HeightRequest = 25;
                DriverTip.Margin = new Thickness(0, -15, 0, 0);
                //tipStack.HeightRequest = 25;
                //deliveryInstructions.Margin = new Thickness(0, -5, 0, 0);
               
            }
            //GetFees(day);
            if (day != "")
            {
                GetFees(day);
            }

            if ((bool)Application.Current.Properties["guest"])
            {
                customerPaymentsView.HeightRequest = 0;
                customerStripeInformationView.HeightRequest = 0;
                customerDeliveryAddressView.HeightRequest = 0;
                GetAvailiableCoupons();
                //coupon_list.HeightRequest = 0;
                //couponsLabel.IsVisible = false;
                //couponSpace.IsVisible = false;
                //addInfo.Text = "Add Contact Info";
                //contactframe.Height = 500;
                InitializeMap();
                if ((string)Application.Current.Properties["day"] == "")
                {
                    order = null;
                    cartItems.Clear();
                }
                else
                {
                    day = (string)Application.Current.Properties["day"];
                }
                if (order != null)
                {
                    cartItems.Clear();
                    foreach (string key in order.Keys)
                    {
                        cartItems.Add(new ItemObject()
                        {
                            qty = order[key].item_quantity,
                            name = order[key].item_name,
                            priceUnit = "( $" + order[key].item_price.ToString("N2") + " / " + order[key].unit + " )",
                            price = order[key].item_price,
                            item_uid = order[key].item_uid,
                            business_uid = order[key].pur_business_uid,
                            img = order[key].img,
                            unit = order[key].unit,
                            description = order[key].description,
                            business_price = order[key].business_price,
                            taxable = order[key].taxable,
                            isItemAvailable = order[key].isItemAvailable,
                        });
                        //AvailableItem(order[key].img, order[key].item_name + " (" + order[key].unit +")", order[key].item_quantity.ToString(), ((double)order[key].item_quantity*order[key].item_price).ToString("N2"));
                        orderCopy.Add(key, order[key]);
                    }
                }

                purchaseObject = new PurchaseDataObject()
                {
                    pur_customer_uid = "GUEST",
                    pur_business_uid = "MOBILE",
                    items = GetOrder(cartItems),
                    order_instructions = "",

                    delivery_instructions = Application.Current.Properties.ContainsKey("user_delivery_instructions") ? (string)Application.Current.Properties["user_delivery_instructions"] : "",

                    order_type = "produce",
                    delivery_first_name = "",
                    delivery_last_name = "",
                    delivery_phone_num = "",
                    delivery_email = "",
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
                DeliveryAddress2.Text = purchaseObject.delivery_city + ", " + purchaseObject.delivery_state + ", " + purchaseObject.delivery_zip;
                //FullName.Text = purchaseObject.delivery_first_name + " " + purchaseObject.delivery_last_name;
                //PhoneNumber.Text = purchaseObject.delivery_phone_num;
                //EmailAddress.Text = purchaseObject.delivery_email;
                if (day != "")
                {
                    deliveryDay = day;
                    //deliveryDate.Text = day + ", ";
                    //deliveryDate.Text += Application.Current.Properties.ContainsKey("delivery_date") ? (string)Application.Current.Properties["delivery_date"] : "";
                    //deliveryTime.Text = "Between ";
                    //deliveryTime.Text += Application.Current.Properties.ContainsKey("delivery_time") ? (string)Application.Current.Properties["delivery_time"] : "";
                }
                else
                {
                    cartEmpty = "EMPTY";
                    //deliveryDate.Text = "";
                    //deliveryTime.Text = "";
                }


                CartItems.ItemsSource = cartItems;
                CartItems.HeightRequest = 50 * cartItems.Count;

                if (day == "")
                {
                    delivery_fee = Constant.deliveryFee;
                    service_fee = Constant.serviceFee;
                    ServiceFee.Text = "$" + service_fee.ToString("N2");
                    DeliveryFee.Text = "$" + delivery_fee.ToString("N2");
                }
                updateTotals(0, 0);
            }
            else
            {
                InitializeMap();
                GetAvailiableCoupons();
                GetDeliveryInstructions();

                if ((string)Application.Current.Properties["day"] == "")
                {
                    order = null;
                    cartItems.Clear();
                }
                else
                {
                    day = (string)Application.Current.Properties["day"];
                }
                if (order != null)
                {
                    cartItems.Clear();
                    foreach (string key in order.Keys)
                    {
                        cartItems.Add(new ItemObject()
                        {
                            qty = order[key].item_quantity,
                            name = order[key].item_name,
                            priceUnit = "( $" + order[key].item_price.ToString("N2") + " / " + order[key].unit + " )",
                            price = order[key].item_price,
                            item_uid = order[key].item_uid,
                            business_uid = order[key].pur_business_uid,
                            img = order[key].img,
                            unit = order[key].unit,
                            description = order[key].description,
                            business_price = order[key].business_price,
                            taxable = order[key].taxable,
                        });
                        orderCopy.Add(key, order[key]);
                    }
                }

                purchaseObject = new PurchaseDataObject()
                {
                    pur_customer_uid = Application.Current.Properties.ContainsKey("user_id") ? (string)Application.Current.Properties["user_id"] : "",
                    pur_business_uid = "MOBILE",
                    items = GetOrder(cartItems),
                    order_instructions = "",

                    delivery_instructions = Application.Current.Properties.ContainsKey("user_delivery_instructions") ? (string)Application.Current.Properties["user_delivery_instructions"] : "",

                    order_type = "produce",
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
                DeliveryAddress2.Text = purchaseObject.delivery_city + ", " + purchaseObject.delivery_state + ", " + purchaseObject.delivery_zip;
                //FullName.Text = purchaseObject.delivery_first_name + " " + purchaseObject.delivery_last_name;
                //PhoneNumber.Text = purchaseObject.delivery_phone_num;
                //EmailAddress.Text = purchaseObject.delivery_email;
                if (day != "")
                {
                    deliveryDay = day;
                    //deliveryDate.Text = day + ", ";
                    //deliveryDate.Text += Application.Current.Properties.ContainsKey("delivery_date") ? (string)Application.Current.Properties["delivery_date"] : "";
                    //deliveryTime.Text = "Between ";
                    //deliveryTime.Text += Application.Current.Properties.ContainsKey("delivery_time") ? (string)Application.Current.Properties["delivery_time"] : "";
                }
                else
                {
                    cartEmpty = "EMPTY";
                    //deliveryDate.Text = "";
                    //deliveryTime.Text = "";
                }


                CartItems.ItemsSource = cartItems;
                CartItems.HeightRequest = 50 * cartItems.Count;

                if (day == "")
                {
                    delivery_fee = Constant.deliveryFee;
                    service_fee = Constant.serviceFee;
                    ServiceFee.Text = "$" + service_fee.ToString("N2");
                    DeliveryFee.Text = "$" + delivery_fee.ToString("N2");
                }

                string CardNo = (string)Application.Current.Properties["CardNumber"];
                string expMonth = (string)Application.Current.Properties["CardExpMonth"];
                string expYear = (string)Application.Current.Properties["CardExpYear"];
                string cardCvv = (string)Application.Current.Properties["CardCVV"];

                if(CardNo != "")
                {
                    cardHolderNumber.Text = CardNo;
                    cardExpMonth.Text = expMonth;
                    cardExpYear.Text = expYear;
                    cardCVV.Text = cardCvv;
                }
 


                updateTotals(0, 0);
            }
        }

        public async void GetDeliveryInstructions()
        {
            try
            {
                var client = new System.Net.Http.HttpClient();
                var ID = (string)Application.Current.Properties["user_id"];
                Debug.WriteLine("USER ID: " + ID);
                var RDSResponse = await client.GetAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/last_delivery_instruction/" + ID);
                var content = await RDSResponse.Content.ReadAsStringAsync();
                if (RDSResponse.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<InfoObject>(content);
                    if(data.result.Count != 0)
                    {
                        Debug.WriteLine("USER DELIVERY INSTRUCTIONS: " + data.result[0].delivery_instructions);
                        if (data.result[0].delivery_instructions != "")
                        {
                            //deliveryInstructions.Text = data.result[0].delivery_instructions;
                        }
                    }
                }
            }
            catch (Exception deliveryInstructions)
            {
                Debug.WriteLine("DELIVERY INSTRUCTION ERROR: " + deliveryInstructions.Message);
            }
        }

        public void InitializeMap()
        {
            map.MapType = MapType.Street;
            Position point = new Position(37.334789, -121.888138);
            var mapSpan = new MapSpan(point, 5, 5);
            map.MoveToRegion(mapSpan);
        }

        public async void GetFees(string day)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();
                var zone = (string)Application.Current.Properties["zone"];
                if (zone != "")
                {
                    Debug.WriteLine("Fees from Zone: " + zone);

                    var RDSResponse = await client.GetAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/get_Fee_Tax/" + zone + "," + day);
                    var content = await RDSResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine("ZONES RESPONSE: " + content);
                    if (RDSResponse.IsSuccessStatusCode)
                    {
                        var data = JsonConvert.DeserializeObject<ZoneFees>(content);
                        Constant.deliveryFee = data.result.delivery_fee;
                        Constant.serviceFee = data.result.service_fee;
                        Constant.tax_rate = data.result.tax_rate;
                        delivery_fee = Constant.deliveryFee;
                        service_fee = Constant.serviceFee;
                    }

                    Debug.WriteLine("Delivery Fee: " + delivery_fee);
                    Debug.WriteLine("Service Fee: " + service_fee);

                    ServiceFee.Text = "$" + service_fee.ToString("N2");
                    DeliveryFee.Text = "$" + delivery_fee.ToString("N2");
                    // GetAvailiableCoupons();
                }
            }
            catch (Exception fees)
            {
                Debug.WriteLine(fees.Message);
            }
        }

        public async void GetAvailiableCoupons()
        {
            var client = new System.Net.Http.HttpClient();
            var email = (string)Application.Current.Properties["user_email"];
            var RDSResponse = new HttpResponseMessage();
            if (!(bool)Application.Current.Properties["guest"])
            {
                RDSResponse = await client.GetAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/available_Coupons/" + email);
            }
            else
            {
                RDSResponse = await client.GetAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/available_Coupons/guest");
            }
            
            if (RDSResponse.IsSuccessStatusCode)
            {
                var result = await RDSResponse.Content.ReadAsStringAsync();
                couponData = JsonConvert.DeserializeObject<CouponResponse>(result);

                couponsList.Clear();

                Debug.WriteLine(result);

                double initialSubTotal = GetSubTotal();
                double initialDeliveryFee = GetDeliveryFee();
                double initialServiceFee = GetServiceFee();
                double initialTaxes = GetTaxes();
                double initialTotal = initialSubTotal + initialDeliveryFee + initialServiceFee + initialTaxes;
                

                foreach (Models.Coupon c in couponData.result)
                {
                    // IF THRESHOLD IS NULL SET IT TO ZERO, OTHERWISE INITIAL VALUE STAYS THE SAME
                    if (c.threshold == null){c.threshold = 0.0;}
                    double discount = 0;
                    //double newTotal = 0;

                    var coupon = new couponItem();
                    //Debug.WriteLine("COUPON IDS: " + c.coupon_uid);
                    coupon.couponId = c.coupon_uid;
                    // INITIALLY, THE IMAGE OF EVERY COUPON IS GRAY. (PLATFORM DEPENDENT)
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        coupon.image = "CouponIconGray.png";
                    }
                    else
                    {
                        coupon.image = "CouponIcon.png";
                    }

                    // SET TITLE LABEL OF COUPON
                    coupon.couponNote = c.notes;
                    // SET THRESHOLD LABEL BASED ON THRESHOLD VALUE: 0 = NO MINIMUM PURCHASE, GREATER THAN 0 = SPEND THE AMOUNT OF THRESHOLD
                    if((double)c.threshold == 0)
                    {
                        coupon.threshold = 0;
                        coupon.thresholdNote = "No minimum purchase";
                    }
                    else
                    {
                        coupon.threshold = (double)c.threshold;
                        coupon.thresholdNote = "$" + coupon.threshold.ToString("N2") + " minimum purchase";
                    }

                    // SET EXPIRATION DATE
                    coupon.expNote = "Expires: "+ DateTime.Parse(c.expire_date).ToString("MM/dd/yyyy");
                    coupon.index = 0;
                    
                    // CALCULATING DISCOUNT, SHIPPING, AND COUPON STATUS
                    if (initialSubTotal >= (double)c.threshold)
                    {
                        if (initialSubTotal >= c.discount_amount)
                        {
                            // All
                            discount = initialSubTotal - ((initialSubTotal - c.discount_amount) * (1.0 - (c.discount_percent / 100.0)));
                        }
                        else
                        {
                            // Partly apply coupon: % discount and $ shipping
                            discount = initialSubTotal;
                        }
                        //newTotal = initialSubTotal - discount + initialServiceFee + (initialDeliveryFee - c.discount_shipping) + initialTaxes;
                        coupon.discount = discount;
                        coupon.shipping = c.discount_shipping;
                        coupon.status = "ACTIVE";
                        coupon.totalDiscount = coupon.discount + coupon.shipping;
                    }
                    else
                    {
                        coupon.discount = 0;
                        coupon.shipping = 0;
                        coupon.status = "NOT-ACTIVE";
                        coupon.totalDiscount = coupon.discount + coupon.shipping;
                    }
                    couponsList.Add(coupon);
                }

                var activeCoupons = new List<couponItem>();
                var nonactiveCoupons = new List<couponItem>();

                foreach(couponItem a in couponsList)
                {
                    if(a.status == "ACTIVE")
                    {
                        activeCoupons.Add(a);
                    }
                    else
                    {
                        nonactiveCoupons.Add(a);
                    }
                }

                // TESTING
                couponsList.Clear();
                Debug.WriteLine("");
                Debug.Write("LIST OF ACTIVE COUPONS: ");
                foreach(couponItem e in activeCoupons)
                {
                    Debug.Write(e.couponId + " ");
                    
                }
                Debug.WriteLine("");
                Debug.WriteLine("");
                Debug.Write("List OF NONACTIVE COUPONS: ");
                foreach (couponItem e in nonactiveCoupons)
                {
                    Debug.Write(e.couponId + " ");
                }

                Debug.WriteLine("");
                Debug.WriteLine("");

                // ALL COUPOUNS ARE ACTIVE
                if(nonactiveCoupons.Count == 0)
                {
                    // PLACE TOTAL DISCOUNT VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                    var TotalDiscountArray = new List<double>();

                    // TotalDiscountArray IS OUT OF ORDER FOR EXAMPLE = [5,3,6,4,5,2,1,]
                    foreach (couponItem ActiveCoupon in activeCoupons)
                    {
                        TotalDiscountArray.Add(ActiveCoupon.totalDiscount);
                    }

                    // TotalDiscountArray IS IN ORDER = [1,2,3,4,5,6]
                    TotalDiscountArray.Sort();

                    Debug.WriteLine("");
                    Debug.Write("SORTED TOTAL DISCOUNT VALUES: ");
                    foreach(double Value in TotalDiscountArray)
                    {
                        Debug.Write(Value + " ");
                    }

                    // RECORD REPETITIONS + READING SORTED ARRAY BACKWARDS + WRITING THE RIGHT MESSAGE ON THE COUPON
                    var Limit = TotalDiscountArray.Count;
                    var LastIndex = Limit - 1;
                    for(int i = 0; i < Limit; i++)
                    {
                        var Value = TotalDiscountArray[LastIndex];
                        for(int j = 0; j < activeCoupons.Count; j++)
                        {
                            if(Value == activeCoupons[j].totalDiscount)
                            {
                                activeCoupons[j].image = "CouponIconGreen.png";
                                activeCoupons[j].savingsOrSpendingNote = "You saved: $" + activeCoupons[j].totalDiscount.ToString("N2");
                                couponsList.Add(activeCoupons[j]);
                                activeCoupons.RemoveAt(j);
                                break;
                            }
                        }
                        LastIndex--;
                    }
                }

                // ALL COUPONS ARE NON-ACTIVE
                if(activeCoupons.Count == 0)
                {
                    // PLACE THRESHOLD VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                    var ThresholdArray = new List<double>();
                    foreach(couponItem NonActiveCoupon in nonactiveCoupons)
                    {
                        ThresholdArray.Add(NonActiveCoupon.threshold);
                    }

                    ThresholdArray.Sort();

                    var Limit = nonactiveCoupons.Count;
                    var FirstIndex = 0;
                    for(int i = 0; i < Limit; i++)
                    {
                        var Value = ThresholdArray[FirstIndex];
                        for(int j = 0; j < nonactiveCoupons.Count; j++)
                        {
                            if (Value == nonactiveCoupons[j].threshold)
                            {
                                nonactiveCoupons[j].savingsOrSpendingNote = "Spend $" + (nonactiveCoupons[j].threshold - initialSubTotal).ToString("N2") + " more to use";
                                couponsList.Add(nonactiveCoupons[j]);
                                nonactiveCoupons.RemoveAt(j);
                                break;
                            }
                        }
                        FirstIndex++;
                    }
                }

                // COUPONS ARE ACTIVE AND NON-ACTIVE
                if(activeCoupons.Count != 0 && nonactiveCoupons.Count != 0)
                {
                    // PLACE TOTAL DISCOUNT VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                    var TotalDiscountArray = new List<double>();

                    // TotalDiscountArray IS OUT OF ORDER FOR EXAMPLE = [5,3,6,4,5,2,1,]
                    foreach (couponItem ActiveCoupon in activeCoupons)
                    {
                        TotalDiscountArray.Add(ActiveCoupon.totalDiscount);
                    }

                    // TotalDiscountArray IS IN ORDER = [1,2,3,4,5,6]
                    TotalDiscountArray.Sort();

                    Debug.WriteLine("");
                    Debug.Write("SORTED TOTAL DISCOUNT VALUES: ");
                    foreach (double Value in TotalDiscountArray)
                    {
                        Debug.Write(Value + " ");
                    }

                    // RECORD REPETITIONS + READING SORTED ARRAY BACKWARDS + WRITING THE RIGHT MESSAGE ON THE COUPON
                    var Limit = TotalDiscountArray.Count;
                    var LastIndex = Limit - 1;
                    for (int i = 0; i < Limit; i++)
                    {
                        var Value = TotalDiscountArray[LastIndex];
                        for (int j = 0; j < activeCoupons.Count; j++)
                        {
                            if (Value == activeCoupons[j].totalDiscount)
                            {
                                activeCoupons[j].image = "CouponIconGreen.png";
                                activeCoupons[j].savingsOrSpendingNote = "You saved: $" + activeCoupons[j].totalDiscount.ToString("N2");
                                couponsList.Add(activeCoupons[j]);
                                activeCoupons.RemoveAt(j);
                                break;
                            }
                        }
                        LastIndex--;
                    }

                    // PLACE THRESHOLD VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                    var ThresholdArray = new List<double>();
                    foreach (couponItem NonActiveCoupon in nonactiveCoupons)
                    {
                        ThresholdArray.Add(NonActiveCoupon.threshold);
                    }

                    ThresholdArray.Sort();

                    Limit = nonactiveCoupons.Count;
                    var FirstIndex = 0;
                    for (int i = 0; i < Limit; i++)
                    {
                        var Value = ThresholdArray[FirstIndex];
                        for (int j = 0; j < nonactiveCoupons.Count; j++)
                        {
                            if (Value == nonactiveCoupons[j].threshold)
                            {
                                nonactiveCoupons[j].savingsOrSpendingNote = "Spend $" + (nonactiveCoupons[j].threshold - initialSubTotal).ToString("N2") + " more to use";
                                couponsList.Add(nonactiveCoupons[j]);
                                nonactiveCoupons.RemoveAt(j);
                                break;
                            }
                        }
                        FirstIndex++;
                    }
                }

                if (couponsList.Count != 0)
                {
                    if (couponsList[0].status == "ACTIVE")
                    {
                        couponsList[0].image = "CouponIconOrange.png";
                        Debug.WriteLine("COUPON DISCOUNT: {0}, COUPON SHIPPING: {1}", couponsList[0].discount, couponsList[0].shipping);
                        updateTotals(couponsList[0].discount, couponsList[0].shipping);
                        appliedIndex = 0;
                    }
                }
                else
                {
                    updateTotals(0, 0);
                }

                for (int i = 0; i < couponsList.Count; i++)
                {
                    couponsList[i].index = i;
                }

                coupon_list.ItemsSource = couponsList;
            }
        }

        void CartClick(System.Object sender, System.EventArgs e)
        {
            if(cartEmpty != "EMPTY")
            {
                foreach (ItemObject i in cartItems)
                {
                    foreach (string key in orderCopy.Keys)
                    {
                        if (orderCopy[key].item_name == i.name)
                        {
                            orderCopy[key].item_quantity = i.qty;
                        }
                    }
                }
                Application.Current.MainPage = new ItemsPage(orderCopy, deliveryDay);
            }
            else
            {
                Application.Current.MainPage = new SelectionPage();
            }
        }

        void ApplyActiveCoupon(System.Object sender, System.EventArgs e)
        {
            var element = (StackLayout)sender;
            var selectedElement = Int32.Parse(element.ClassId);
            
            if(couponsList[selectedElement].image == "CouponIconGreen.png")
            {
                couponsList[appliedIndex].image = "CouponIconGreen.png";
                // couponsList[defaultCouponIndex].image = "CouponIconGray.png";
                couponsList[appliedIndex].update();
                couponsList[selectedElement].image = "CouponIconOrange.png";
                couponsList[selectedElement].update();
                appliedIndex = selectedElement;
                updateTotals(couponsList[appliedIndex].discount,couponsList[appliedIndex].shipping);
            }
        }

        public void updateTotals(double discount, double discount_delivery_fee)
        {
            subtotal = 0.0;
            total_qty = 0;
            var tips = 0.0;
            delivery_fee = GetDeliveryFee();
            taxes = GetTaxes();
            service_fee = GetServiceFee();
            foreach (ItemObject item in cartItems)
            {
                if (item.isItemAvailable)
                {
                    total_qty += item.qty;
                    subtotal += (item.qty * item.price);
                }
            }

            SubTotal.Text = "$" + subtotal.ToString("N2");
            this.discount = discount;
            Discount.Text = "$" + discount.ToString("N2");
            
            if((delivery_fee - discount_delivery_fee <= 0))
            {
                DeliveryFee.Text = "$" + (0.00).ToString("N2");
                delivery_fee_db = 0.0;
            }
            else
            {
                DeliveryFee.Text = "$" + (delivery_fee - discount_delivery_fee).ToString("N2");
                delivery_fee_db = delivery_fee - discount_delivery_fee;
            }


            var cartTax = 0.0;
            foreach (ItemObject item in cartItems)
            {
                var tax = 0.0;
                if(item.taxable == "TRUE")
                {
                    tax = (item.qty * item.price) * GetTaxes() / 100;
                    cartTax += tax;
                }
            }

            taxes = cartTax;
            Taxes.Text = "$" + taxes.ToString("N2");

            if (DriverTip.Text == null)
            {
                if ((delivery_fee - discount_delivery_fee <= 0))
                {
                    total = subtotal - discount + (0.00) + taxes + service_fee;
                }
                else
                {
                    total = subtotal - discount + (delivery_fee - discount_delivery_fee) + taxes + service_fee;
                }
            }
            else
            {
                if(DriverTip.Text == null || DriverTip == null || DriverTip.Text.Length == 0)
                {
                    DriverTip.Text = "0.00";
                }
                Debug.WriteLine("Driver Tip: " + DriverTip.Text);
                if ((delivery_fee - discount_delivery_fee <= 0))
                {
                    //total = subtotal - discount + (0.00) + taxes + service_fee + Double.Parse(DriverTip.Text);
                    total = subtotal - discount + (0.00) + taxes + service_fee + driver_tips;
                    //tips = Double.Parse(DriverTip.Text);
                    tips = 0.0;
                }
                else
                {
                    //total = subtotal - discount + (delivery_fee - discount_delivery_fee) + taxes + service_fee + Double.Parse(DriverTip.Text);
                    total = subtotal - discount + (delivery_fee - discount_delivery_fee) + taxes + service_fee + driver_tips;
                    //tips = Double.Parse(DriverTip.Text);
                    tips = 0.0;
                }
                
            }

            GrandTotal.Text = "$" + total.ToString("N2");
            viewTotal.Text = "$" + total.ToString("N2");
            CartTotal.Text = total_qty.ToString();
            //driver_tips = tips;
        }

        // This function return the subtotal amount upon initial purchase
        public double GetSubTotal()
        {
            subtotal = 0.0;
            total_qty = 0;

            foreach (ItemObject item in cartItems)
            {
                if (item.isItemAvailable)
                {
                    total_qty += item.qty;
                    subtotal += (item.qty * item.price);
                }
            }
            return subtotal;
        }

        // This function return the delivery fee amount
        public double GetDeliveryFee()
        {
            return Constant.deliveryFee;
        }

        public double GetTaxes()
        {
            return taxes;
        }

        public double GetServiceFee()
        {
            return Constant.serviceFee;
        }

        public double GetTotal()
        {
            return total;
        }



        public void AddItems(object sender, EventArgs e)
        {
            if (cartEmpty != "EMPTY")
            {
                foreach (ItemObject i in cartItems)
                {
                    foreach (string key in orderCopy.Keys)
                    {
                        if (orderCopy[key].item_name == i.name)
                        {
                            orderCopy[key].item_quantity = i.qty;
                        }
                    }
                }
                Application.Current.MainPage = new ItemsPage(orderCopy, deliveryDay);
            }
            else
            {
                Application.Current.MainPage = new SelectionPage();
            }
        }

        public async void checkoutAsync(object sender, EventArgs e)
        {
            if (total > 0 && cartEmpty != "EMPTY")
            {
           
            //cardHolderEmail.Text = purchaseObject.delivery_email;
            //cardHolderName.Text = purchaseObject.delivery_first_name + " " + purchaseObject.delivery_last_name;
            //cardHolderAddress.Text = purchaseObject.delivery_address;
            //cardHolderUnit.Text = purchaseObject.delivery_unit;
            //cardState.Text = purchaseObject.delivery_state;
            //cardCity.Text = purchaseObject.delivery_city;
            cardZip.Text = purchaseObject.delivery_zip;
            //cardDescription.Text = "";

                //if ((bool)Application.Current.Properties["guest"])
                //{
                //    if (purchaseObject.delivery_first_name == "" || purchaseObject.delivery_last_name == "" || purchaseObject.delivery_email == "")
                //    {
                //        await DisplayAlert("Oops", "Please enter your delivery contact information!", "OK");
                //        //contactframe.Height = this.Height / 2;
                //        return;
                //    }
                //}

                if (Device.RuntimePlatform == Device.Android)
            {
                //cardframe.Height = this.Height - 136;
            }
            else
            {
                //cardframe.Height = this.Height;
            }
           
            //options.Height = 0;
            
            string dateTime = DateTime.Parse((string)Application.Current.Properties["delivery_date"]).ToString("yyyy-MM-dd");
            string t = (string)Application.Current.Properties["delivery_time"];
            Debug.WriteLine("DELIVERY DATE: " + dateTime);
            Debug.WriteLine("START DELIVERY TIME: " + t);
            string startTime = "";
            foreach(char a in t.ToCharArray())
            {
                if(a != '-')
                {
                    startTime += a;
                }
                else { break; }
            }
            var timeStamp = new DateTime();

            if (startTime != "")
            {
                timeStamp = DateTime.Parse(startTime.Trim());
            }
          
            
            purchaseObject.cc_num = "";
            purchaseObject.cc_exp_date = "";
            purchaseObject.cc_cvv = "";
            purchaseObject.cc_zip = "";
            purchaseObject.charge_id = "";
            purchaseObject.payment_type = ((Button)sender).Text == "Checkout with Paypal" ? "PAYPAL" : "STRIPE";
            purchaseObject.items = GetOrder(cartItems);
            purchaseObject.start_delivery_date = ((DateTime)Application.Current.Properties["deliveryDate"]).ToString("yyyy-MM-dd HH:mm:ss");
                if (!(bool)Application.Current.Properties["guest"])
            {
                if (appliedIndex != -1)
                {
                    Debug.WriteLine("COUPON ID: " + couponsList[appliedIndex].couponId);
                    if (couponsList[appliedIndex].couponId != null)
                    {
                        purchaseObject.pay_coupon_id = couponsList[appliedIndex].couponId;
                    }
                    else
                    {
                        purchaseObject.pay_coupon_id = "";
                    }
                }
                else
                {
                    purchaseObject.pay_coupon_id = "";
                }
                //purchaseObject.pay_coupon_id = couponData.result[defaultCouponIndex].coupon_uid;
            }
            else
            {
                purchaseObject.pay_coupon_id = "";
            }
            purchaseObject.amount_due = total.ToString("N2");
            purchaseObject.amount_discount = discount.ToString("N2"); ;
            purchaseObject.amount_paid = total.ToString("N2"); ;
            purchaseObject.info_is_Addon = "FALSE";

            //var purchaseString = JsonConvert.SerializeObject(purchaseObject);
            //System.Diagnostics.Debug.WriteLine(purchaseString);
            }
            else
            {
                await DisplayAlert("Oops", "Your total is zero or your shopping cart is empty. Please select a delivery day and add items to your cart!", "Thank you");
            }
        }

        public ObservableCollection<PurchasedItem> GetOrder(ObservableCollection<ItemObject> list)
        {
            ObservableCollection<PurchasedItem> purchasedOrder = new ObservableCollection<PurchasedItem>();
            foreach(ItemObject i in list)
            {
                if (i.isItemAvailable)
                {
                    purchasedOrder.Add(new PurchasedItem
                    {
                        img = i.img,
                        qty = i.qty,
                        name = i.name,
                        unit = i.unit,
                        price = i.price,
                        item_uid = i.item_uid,
                        itm_business_uid = i.business_uid,
                        description = i.description,
                        business_price = i.business_price,
                    });
                }
            }
            return purchasedOrder;
        }
        
        async void CompletePaymentClick(System.Object sender, System.EventArgs e)
        {
            //UserDialogs.Instance.ShowLoading("Processing your payment...");
            //cardframe.Height = 0;
            //options.Height = 65;

            if (Device.RuntimePlatform == Device.Android)
            {
                UpdateTotalAmount(sender, e);
            }
            UserDialogs.Instance.ShowLoading("Processing Payment...");
            await PayViaStripe();
            UserDialogs.Instance.HideLoading();
            //cardframe.Height = 0;
            //options.Height = 65;
            //UserDialogs.Instance.HideLoading();
        }

        public string OrderId = "";
        async void PayViaPayPal(System.Object sender, System.EventArgs e)
        {
            if (total > 0 && cartEmpty != "EMPTY")
            {
                if ((bool)Application.Current.Properties["guest"])
                {
                    if (purchaseObject.delivery_first_name == "" || purchaseObject.delivery_last_name == "" || purchaseObject.delivery_email == "")
                    {
                        await DisplayAlert("Oops", "Please enter your delivery contact information!", "OK");
                        //contactframe.Height = this.Height / 2;
                        return;
                    }
                }
                
                //if (deliveryInstructions.Text != null)
                //{
                //    if (deliveryInstructions.Text.Trim() == "SFTEST")
                //    {
                //        Debug.WriteLine("DELIVERY INSTRUCTIONS WERE SET 'SFTEST' ");
                //        mode = "TEST";
                //        clientId = Constant.TestClientId;
                //        secret = Constant.TestSecret;
                //    }
                //}

                if(Device.RuntimePlatform == Device.Android)
                {
                    UpdateTotalAmount(sender, e);
                }

                string dateTime = DateTime.Parse((string)Application.Current.Properties["delivery_date"]).ToString("yyyy-MM-dd");
                string t = (string)Application.Current.Properties["delivery_time"];
                Debug.WriteLine("DELIVERY DATE: " + dateTime);
                Debug.WriteLine("START DELIVERY TIME: " + t);
                string startTime = "";
                foreach (char a in t.ToCharArray())
                {
                    if (a != '-')
                    {
                        startTime += a;
                    }
                    else { break; }
                }
                var timeStamp = new DateTime();

                if (startTime != "")
                {
                    timeStamp = DateTime.Parse(startTime.Trim());
                }

                purchaseObject.cc_num = "";
                purchaseObject.cc_exp_date = "";
                purchaseObject.cc_cvv = "";
                purchaseObject.cc_zip = "";
                purchaseObject.charge_id = "";
                purchaseObject.payment_type = ((Button)sender).Text == "Checkout with Paypal" ? "PAYPAL" : "STRIPE";
                purchaseObject.items = GetOrder(cartItems);
                purchaseObject.start_delivery_date = ((DateTime)Application.Current.Properties["deliveryDate"]).ToString("yyyy-MM-dd HH:mm:ss");
                if (!(bool)Application.Current.Properties["guest"])
                {
                    if(appliedIndex != -1)
                    {
                        Debug.WriteLine("COUPON ID: " + couponsList[appliedIndex].couponId);
                        if (couponsList[appliedIndex].couponId != null)
                        {
                            purchaseObject.pay_coupon_id = couponsList[appliedIndex].couponId;
                        }
                        else
                        {
                            purchaseObject.pay_coupon_id = "";
                        }
                    }
                    else
                    {
                        purchaseObject.pay_coupon_id = "";
                    }
                    //purchaseObject.pay_coupon_id = couponData.result[defaultCouponIndex].coupon_uid;
                }
                else
                {
                    purchaseObject.pay_coupon_id = "";
                }
               
                purchaseObject.amount_due = total.ToString("N2"); ;
                purchaseObject.amount_discount = discount.ToString("N2"); ;
                purchaseObject.amount_paid = total.ToString("N2"); ;
                purchaseObject.info_is_Addon = "FALSE";
                //Paypal();

                // Step 1
                // Create a request to pay with PayPal using PayPal client

                var point = 200 / this.Height;
                var factor = 200 * point;

                paypalframe.Height = this.Height -136;
                webViewPage.HeightRequest = this.Height -136;
                //options.Height = 0;
                
                var response = await createOrder(purchaseObject.amount_due);
                var content = response.Result<PayPalCheckoutSdk.Orders.Order>();
                PayPalCheckoutSdk.Orders.Order result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                Console.WriteLine("Status: {0}", result.Status);
                Console.WriteLine("Order Id: {0}", result.Id);
                Console.WriteLine("Intent: {0}", result.CheckoutPaymentIntent);
                Console.WriteLine("Links:");
                foreach (LinkDescription link in result.Links)
                {
                    Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel, link.Href, link.Method);
                    if (link.Rel == "approve")
                    {
                        webViewPage.Source = link.Href;
                        //Debug.WriteLine("PAYPAL URL: "+ link.Href);
                        //webViewPage.Source = "https://servingfresh.me";
                    }
                }

                webViewPage.Navigated += WebViewPage_Navigated;
                
                OrderId = result.Id;
            }
            else
            {
                await DisplayAlert("Oops", "Your total is zero or your shopping cart is empty. Please select a delivery day and add items to your cart!", "Thank you");
            }
        }

        private void WebViewPage_Navigated(object sender, WebNavigatedEventArgs e)
        {
            var source = webViewPage.Source as UrlWebViewSource;
            Debug.WriteLine("Source From WebView: " + source.Url);
            if (source.Url.Contains("https://servingfresh.me/"))
            {
                paypalframe.Height = 0;
                //options.Height = 65;
                Debug.WriteLine("We got to serving fresh");
                _ = captureOrder(OrderId);
            }

            //else if(source.Url.Contains("https://servingfresh.me/"))
            //{
            //    //source.Url.Contains("https://devblogs.microsoft.com/xamarin/")
            //    paypalframe.Height = 0;
            //    options.Height = 65;
            //    Debug.WriteLine("We got to serving fresh");
            //    _ = captureOrder(OrderId);
            //}
        }

        public async void GetPayPalCredentials()
        {
            var c = new System.Net.Http.HttpClient();
            var paypal = new Credentials();
            // LIVE 
            paypal.key = Constant.LiveClientId;

            // TEST
            // paypal.key = Constant.TestClientId;

            var stripeObj = JsonConvert.SerializeObject(paypal);
            Debug.WriteLine("key to send JSON: " + stripeObj);
            var stripeContent = new StringContent(stripeObj, Encoding.UTF8, "application/json");
            var RDSResponse = await c.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/Paypal_Payment_key_checker", stripeContent);
            var content = await RDSResponse.Content.ReadAsStringAsync();
            Debug.WriteLine("Response key from paypal :" + content);

            if (RDSResponse.IsSuccessStatusCode)
            {
                if (content.Contains("Test"))
                {
                    mode = "TEST";
                    clientId = Constant.TestClientId;
                    secret = Constant.TestSecret;
                }
                else if (content.Contains("Live"))
                {
                    mode = "LIVE";
                    clientId = Constant.LiveClientId;
                    secret = Constant.LiveSecret;
                }
                else
                {
                    Debug.WriteLine("INVALID ENTRY");
                }
                Debug.WriteLine("MODE            : " + mode);
                Debug.WriteLine("PAYPAL CLIENT ID: " + clientId);
                Debug.WriteLine("PAYPAL SECRET   : " + secret);
            }
            else
            {
                await DisplayAlert("Oops", "We can't process your request at this moment.", "OK");
            }
        }


        public static PayPalHttp.HttpClient client()
        {
            
            if(mode == "TEST")
            {
                Debug.WriteLine("PAYPAL TEST ENVIROMENT");
                PayPalEnvironment environment = new SandboxEnvironment(clientId, secret);
                PayPalHttpClient payPalClient = new PayPalHttpClient(environment);
                return payPalClient;
            }else if (mode == "LIVE")
            {
                Debug.WriteLine("PAYPAL LIVE ENVIROMENT");
                PayPalEnvironment environment = new LiveEnvironment(clientId, secret);
                PayPalHttpClient payPalClient = new PayPalHttpClient(environment);
                return payPalClient;
            }
            return null;
        }

       public async Task PayViaStripe()
        {
            try
            {
                //if (deliveryInstructions.Text != null)
                if (deliveryInstructions.Text != null)
                {
                    //if (deliveryInstructions.Text.Trim() == "SFTEST")
                    if (deliveryInstructions.Text.Trim() == "SFTEST")
                    {
                        //UserDialogs.Instance.Loading("Processing Payment...");
                        //UserDialogs.Instance.ShowLoading("Processing Payment...");
                        Debug.Write("STRIPE MODE: " + "TEST");
                        Debug.WriteLine("SK     : " + Constant.TestSK);
                        StripeConfiguration.ApiKey = Constant.TestSK;

                        string CardNo = cardHolderNumber.Text.Trim();
                        string expMonth = cardExpMonth.Text.Trim();
                        string expYear = cardExpYear.Text.Trim();
                        string cardCvv = cardCVV.Text.Trim();

                        // STORE INFO
                        if (!(bool)Application.Current.Properties["guest"])
                        {
                            Application.Current.Properties["CardNumber"] = CardNo;
                            Application.Current.Properties["CardExpMonth"] = expMonth;
                            Application.Current.Properties["CardExpYear"] = expYear;
                            Application.Current.Properties["CardCVV"] = cardCvv;
                        }

                        // Step 1: Create Card
                        TokenCardOptions stripeOption = new TokenCardOptions();
                        stripeOption.Number = CardNo;
                        stripeOption.ExpMonth = Convert.ToInt64(expMonth);
                        stripeOption.ExpYear = Convert.ToInt64(expYear);
                        stripeOption.Cvc = cardCvv;

                        // Step 2: Assign card to token object
                        TokenCreateOptions stripeCard = new TokenCreateOptions();
                        stripeCard.Card = stripeOption;

                        TokenService service = new TokenService();
                        Stripe.Token newToken = service.Create(stripeCard);

                        // Step 3: Assign the token to the soruce 
                        var option = new SourceCreateOptions();
                        option.Type = SourceType.Card;
                        option.Currency = "usd";
                        option.Token = newToken.Id;

                        var sourceService = new SourceService();
                        Source source = sourceService.Create(option);

                        // Step 4: Create customer
                        CustomerCreateOptions customer = new CustomerCreateOptions();
                        customer.Name = cardHolderName.Text.Trim();
                        //customer.Email = cardHolderEmail.Text.ToLower().Trim();
                        //customer.Description = cardDescription.Text.Trim();
                        //if (cardHolderUnit.Text == null)
                        //{
                        //    cardHolderUnit.Text = "";
                        //}
                        //customer.Address = new AddressOptions { City = cardCity.Text.Trim(), Country = Constant.Contry, Line1 = cardHolderAddress.Text.Trim(), Line2 = cardHolderUnit.Text.Trim(), PostalCode = cardZip.Text.Trim(), State = cardState.Text.Trim() };

                        var customerService = new CustomerService();
                        var cust = customerService.Create(customer);

                        // Step 5: Charge option
                        var chargeOption = new ChargeCreateOptions();
                        chargeOption.Amount = (long)RemoveDecimalFromTotalAmount(total.ToString("N2"));
                        chargeOption.Currency = "usd";
                        //chargeOption.ReceiptEmail = cardHolderEmail.Text.ToLower().Trim();
                        chargeOption.Customer = cust.Id;
                        chargeOption.Source = source.Id;
                        //chargeOption.Description = cardDescription.Text.Trim();

                        // Step 6: charge the customer
                        var chargeService = new ChargeService();
                        Charge charge = chargeService.Create(chargeOption);
                        if (charge.Status == "succeeded")
                        {
                            //UserDialogs.Instance.ShowLoading("Processing your payment...");
                            // Successful Payment
                            //await DisplayAlert("Congratulations", "Payment was successful. We appreciate your business", "OK");
                            ClearCardInfo();

                            //if (deliveryInstructions.Text == null)
                            if (deliveryInstructions.Text == null)
                            {
                                Debug.WriteLine("STRIPE");
                                Debug.WriteLine("DELIVERY INSTRUCTIONS WERE NOT SET");
                                purchaseObject.delivery_instructions = "";
                            }
                            else
                            {
                                purchaseObject.delivery_instructions = deliveryInstructions.Text;
                                purchaseObject.delivery_instructions = "";
                            }
                            purchaseObject.subtotal = GetSubTotal().ToString("N2");
                            purchaseObject.service_fee = service_fee.ToString("N2");
                            purchaseObject.delivery_fee = delivery_fee_db.ToString("N2");
                            purchaseObject.driver_tip = driver_tips.ToString("N2");
                            purchaseObject.taxes = GetTaxes().ToString("N2");

                            var purchaseString = JsonConvert.SerializeObject(purchaseObject);
                            Debug.WriteLine("Purchase: " + purchaseString);
                            var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
                            var client = new System.Net.Http.HttpClient();
                            var Response = await client.PostAsync(Constant.PurchaseUrl, purchaseMessage);

                            Debug.WriteLine("Order was written to DB: " + Response.IsSuccessStatusCode);
                            //Debug.WriteLine("Coupon was succesfully updated (subtract)" + RDSCouponResponse);
                            if (Response.IsSuccessStatusCode)
                            {
                                var RDSResponseContent = await Response.Content.ReadAsStringAsync();

                                cartItems.Clear();
                                updateTotals(0, 0);
                                total = 00.00;
                                total_qty = 0;
                                Application.Current.Properties["day"] = "";
                                cartEmpty = "EMPTY";
                                //cartHeight.Height = 0;
                                if (!(bool)Application.Current.Properties["guest"])
                                {
                                    //UserDialogs.Instance.HideLoading();
                                    //UserDialogs.Instance.HideLoading();
                                    Application.Current.MainPage = new HistoryPage("SUCCESS");
                                    //await DisplayAlert("We appreciate your business", "Thank you for placing an order through Serving Fresh! Our Serving Fresh Team is processing your order!", "OK");
                                }
                                else
                                {
                                    Application.Current.Properties["guest"] = false;

                                    var firstName = (string)Application.Current.Properties["user_first_name"];
                                    var lastName = (string)Application.Current.Properties["user_last_name"];
                                    var email = (string)Application.Current.Properties["user_email"];
                                    var phone = (string)Application.Current.Properties["user_phone_num"];
                                    var address = (string)Application.Current.Properties["user_address"];
                                    var unit = (string)Application.Current.Properties["user_unit"];
                                    var city = (string)Application.Current.Properties["user_city"];
                                    var zipcode = (string)Application.Current.Properties["user_zip_code"];
                                    var state = (string)Application.Current.Properties["user_state"];
                                    var lat = (string)Application.Current.Properties["user_latitude"];
                                    var longitude = (string)Application.Current.Properties["user_longitude"];

                                    //UserDialogs.Instance.HideLoading();
                                    Application.Current.MainPage = new SignUpPage(firstName, lastName, phone, email, address, unit, city, state, zipcode, "guest", lat, longitude);
                                }
                            }
                            UserDialogs.Instance.HideLoading();
                        }
                        else
                        {
                            // Fail
                            UserDialogs.Instance.HideLoading();
                            await DisplayAlert("Oops", "Payment was not successful. Please try again", "OK");
                        }
                    }
                }
                else
                {
                    //UserDialogs.Instance.Loading("Processing Payment...");
                    UserDialogs.Instance.ShowLoading("Processing Payment...");
                    var c = new System.Net.Http.HttpClient();
                    var stripe = new Credentials();
                    // LIVE
                    stripe.key = Constant.LivePK;

                    // TEST
                    //stripe.key = Constant.TestSK;

                    var stripeObj = JsonConvert.SerializeObject(stripe);
                    Debug.WriteLine("key to send JSON: " + stripeObj);
                    var stripeContent = new StringContent(stripeObj, Encoding.UTF8, "application/json");
                    var RDSResponse = await c.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/Stripe_Payment_key_checker", stripeContent);
                    var content = await RDSResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine("Response from key: " + content);

                    if (RDSResponse.IsSuccessStatusCode)
                    {
                        if (content != "200")
                        {
                            string SK = "";
                            string type = "";
                            if (content.Contains("Test"))
                            {
                                type = "TEST";
                                SK = Constant.TestSK;
                            }
                            if (content.Contains("Live"))
                            {
                                type = "LIVE";
                                SK = Constant.LiveSK;
                            }
                            Debug.Write("STRIPE MODE: " + type);
                            Debug.WriteLine("SK     : " + SK);
                            //Debug.WriteLine("SK" + SK);
                            StripeConfiguration.ApiKey = SK;

                            string CardNo = cardHolderNumber.Text.Trim();
                            string expMonth = cardExpMonth.Text.Trim();
                            string expYear = cardExpYear.Text.Trim();
                            string cardCvv = cardCVV.Text.Trim();

                            // STORE INFO

                            if (!(bool)Application.Current.Properties["guest"])
                            {
                                Application.Current.Properties["CardNumber"] = CardNo;
                                Application.Current.Properties["CardExpMonth"] = expMonth;
                                Application.Current.Properties["CardExpYear"] = expYear;
                                Application.Current.Properties["CardCVV"] = cardCvv;
                            }

                            // Step 1: Create Card
                            TokenCardOptions stripeOption = new TokenCardOptions();
                            stripeOption.Number = CardNo;
                            stripeOption.ExpMonth = Convert.ToInt64(expMonth);
                            stripeOption.ExpYear = Convert.ToInt64(expYear);
                            stripeOption.Cvc = cardCvv;

                            // Step 2: Assign card to token object
                            TokenCreateOptions stripeCard = new TokenCreateOptions();
                            stripeCard.Card = stripeOption;

                            TokenService service = new TokenService();
                            Stripe.Token newToken = service.Create(stripeCard);

                            // Step 3: Assign the token to the soruce 
                            var option = new SourceCreateOptions();
                            option.Type = SourceType.Card;
                            option.Currency = "usd";
                            option.Token = newToken.Id;

                            var sourceService = new SourceService();
                            Source source = sourceService.Create(option);

                            // Step 4: Create customer
                            CustomerCreateOptions customer = new CustomerCreateOptions();
                            customer.Name = cardHolderName.Text.Trim();
                            //customer.Email = cardHolderEmail.Text.ToLower().Trim();
                            //customer.Description = cardDescription.Text.Trim();
                            //if (cardHolderUnit.Text == null)
                            //{
                            //    cardHolderUnit.Text = "";
                            //}
                            //customer.Address = new AddressOptions { City = cardCity.Text.Trim(), Country = Constant.Contry, Line1 = cardHolderAddress.Text.Trim(), Line2 = cardHolderUnit.Text.Trim(), PostalCode = cardZip.Text.Trim(), State = cardState.Text.Trim() };

                            var customerService = new CustomerService();
                            var cust = customerService.Create(customer);

                            // Step 5: Charge option
                            var chargeOption = new ChargeCreateOptions();
                            chargeOption.Amount = (long)RemoveDecimalFromTotalAmount(total.ToString("N2"));
                            chargeOption.Currency = "usd";
                            //chargeOption.ReceiptEmail = cardHolderEmail.Text.ToLower().Trim();
                            chargeOption.Customer = cust.Id;
                            chargeOption.Source = source.Id;
                            //chargeOption.Description = cardDescription.Text.Trim();

                            // Step 6: charge the customer
                            var chargeService = new ChargeService();
                            Charge charge = chargeService.Create(chargeOption);
                            if (charge.Status == "succeeded")
                            {
                                // Successful Payment
                                await DisplayAlert("Congratulations", "Payment was successful. We appreciate your business", "OK");
                                ClearCardInfo();

                                //if (deliveryInstructions.Text == null)
                                if (deliveryInstructions.Text == null)
                                {
                                    Debug.WriteLine("STRIPE");
                                    Debug.WriteLine("DELIVERY INSTRUCTIONS WERE NOT SET");
                                    purchaseObject.delivery_instructions = "";
                                }
                                else
                                {
                                    purchaseObject.delivery_instructions = deliveryInstructions.Text;
                                    //purchaseObject.delivery_instructions = "";
                                }
                                purchaseObject.subtotal = GetSubTotal().ToString("N2");
                                purchaseObject.service_fee = service_fee.ToString("N2");
                                purchaseObject.delivery_fee = delivery_fee_db.ToString("N2");
                                purchaseObject.driver_tip = driver_tips.ToString("N2");
                                purchaseObject.taxes = GetTaxes().ToString("N2");

                                var purchaseString = JsonConvert.SerializeObject(purchaseObject);
                                System.Diagnostics.Debug.WriteLine("Purchase: " + purchaseString);
                                var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
                                var client = new System.Net.Http.HttpClient();
                                var Response = await client.PostAsync(Constant.PurchaseUrl, purchaseMessage);

                                Debug.WriteLine("Order was written to DB: " + Response.IsSuccessStatusCode);
                                //Debug.WriteLine("Coupon was succesfully updated (subtract)" + RDSCouponResponse);
                                if (Response.IsSuccessStatusCode)
                                {
                                    var RDSResponseContent = await Response.Content.ReadAsStringAsync();

                                    cartItems.Clear();
                                    updateTotals(0, 0);
                                    total = 00.00;
                                    total_qty = 0;
                                    Application.Current.Properties["day"] = "";
                                    cartEmpty = "EMPTY";
                                    //cartHeight.Height = 0;
                                    if (!(bool)Application.Current.Properties["guest"])
                                    {
                                        //UserDialogs.Instance.HideLoading();
                                        Application.Current.MainPage = new HistoryPage("SUCCESS");
                                        //await DisplayAlert("We appreciate your business", "Thank you for placing an order through Serving Fresh! Our Serving Fresh Team is processing your order!", "OK");
                                    }
                                    else
                                    {
                                        Application.Current.Properties["guest"] = false;

                                        var firstName = (string)Application.Current.Properties["user_first_name"];
                                        var lastName = (string)Application.Current.Properties["user_last_name"];
                                        var email = (string)Application.Current.Properties["user_email"];
                                        var phone = (string)Application.Current.Properties["user_phone_num"];
                                        var address = (string)Application.Current.Properties["user_address"];
                                        var unit = (string)Application.Current.Properties["user_unit"];
                                        var city = (string)Application.Current.Properties["user_city"];
                                        var zipcode = (string)Application.Current.Properties["user_zip_code"];
                                        var state = (string)Application.Current.Properties["user_state"];
                                        var lat = (string)Application.Current.Properties["user_latitude"];
                                        var longitude = (string)Application.Current.Properties["user_longitude"];
                                        //UserDialogs.Instance.HideLoading();
                                        Application.Current.MainPage = new SignUpPage(firstName, lastName, phone, email, address, unit, city, state, zipcode, "guest", lat, longitude);
                                    }
                                }
                                UserDialogs.Instance.HideLoading();
                            }
                            else
                            {
                                // Fail
                                
                                UserDialogs.Instance.HideLoading();
                                await DisplayAlert("Oops", "Payment was not successful. Please try again", "OK");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                 await DisplayAlert("Alert!", ex.Message, "OK");
            }
        }

        void ClearCardInfo()
        {
            cardHolderNumber.Text = "";
            cardExpMonth.Text = "";
            cardExpYear.Text = "";
            cardCVV.Text = "";

            //cardCity.Text = "";
            //cardState.Text = "";
            //cardZip.Text = "";
            //cardHolderAddress.Text = "";
            //cardHolderUnit.Text = "";
            //cardHolderEmail.Text = "";
            //cardDescription.Text = "";
            //cardHolderName.Text = "";
        }

        public int RemoveDecimalFromTotalAmount(string amount)
        {
            var stringAmount = "";
            var arrayAmount = amount.ToCharArray();
            for(int i = 0; i < arrayAmount.Length; i++)
            {
                if((int)arrayAmount[i] != (int)'.')
                {
                    stringAmount += arrayAmount[i];
                }
            }
            Debug.WriteLine("STRIPE AMOUNT TO CHARGE: " + stringAmount);
            return Int32.Parse(stringAmount);
        }

        void CancelPaymentClick(System.Object sender, System.EventArgs e)
        {
            //cardframe.Height = 0;
            //options.Height = 65;
        }

        // ====================================================================
        public void increase_qty(object sender, EventArgs e)
        {
            //Label l = (Label)sender;
            //TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            var button = (Button)sender;
            ItemObject item = (ItemObject)button.CommandParameter;
            if (item != null)
            {
                item.increase_qty();
                GetNewDefaltCoupon();
            }
        }

        public void decrease_qty(object sender, EventArgs e)
        {
            //Label l = (Label)sender;
            //TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            var button = (Button)sender;
            ItemObject item = (ItemObject)button.CommandParameter;
            if (item != null)
            {
                item.decrease_qty();
                GetNewDefaltCoupon();
            }
        }
        // =====================================================================

        public void GetNewDefaltCoupon()
        {
            couponsList.Clear();
            TempCouponsList.Clear();
            double initialSubTotal = GetSubTotal();
            double initialDeliveryFee = GetDeliveryFee();
            double initialServiceFee = GetServiceFee();
            double initialTaxes = GetTaxes();
            double initialTotal = initialSubTotal + initialDeliveryFee + initialServiceFee + initialTaxes;

            foreach (Models.Coupon c in couponData.result)
            {
                // IF THRESHOLD IS NULL SET IT TO ZERO, OTHERWISE INITIAL VALUE STAYS THE SAME
                if (c.threshold == null) { c.threshold = 0.0; }
                double discount = 0;
                //double newTotal = 0;

                var coupon = new couponItem();
                //Debug.WriteLine("COUPON IDS: " + c.coupon_uid);
                coupon.couponId = c.coupon_uid;
                // INITIALLY, THE IMAGE OF EVERY COUPON IS GRAY. (PLATFORM DEPENDENT)
                if (Device.RuntimePlatform == Device.Android)
                {
                    coupon.image = "CouponIconGray.png";
                }
                else
                {
                    coupon.image = "CouponIcon.png";
                }

                // SET TITLE LABEL OF COUPON
                coupon.couponNote = c.notes;
                // SET THRESHOLD LABEL BASED ON THRESHOLD VALUE: 0 = NO MINIMUM PURCHASE, GREATER THAN 0 = SPEND THE AMOUNT OF THRESHOLD
                if ((double)c.threshold == 0)
                {
                    coupon.threshold = 0;
                    coupon.thresholdNote = "No minimum purchase";
                }
                else
                {
                    coupon.threshold = (double)c.threshold;
                    coupon.thresholdNote = "$" + coupon.threshold.ToString("N2") + " minimum purchase";
                }

                // SET EXPIRATION DATE
                coupon.expNote = "Expires: " + DateTime.Parse(c.expire_date).ToString("MM/dd/yyyy");
                coupon.index = 0;

                // CALCULATING DISCOUNT, SHIPPING, AND COUPON STATUS
                if (initialSubTotal >= (double)c.threshold)
                {
                    if (initialSubTotal >= c.discount_amount)
                    {
                        // All
                        discount = initialSubTotal - ((initialSubTotal - c.discount_amount) * (1.0 - (c.discount_percent / 100.0)));
                    }
                    else
                    {
                        // Partly apply coupon: % discount and $ shipping
                        discount = initialSubTotal;
                    }
                    //newTotal = initialSubTotal - discount + initialServiceFee + (initialDeliveryFee - c.discount_shipping) + initialTaxes;
                    coupon.discount = discount;
                    coupon.shipping = c.discount_shipping;
                    coupon.status = "ACTIVE";
                    coupon.totalDiscount = coupon.discount + coupon.shipping;
                }
                else
                {
                    coupon.discount = 0;
                    coupon.shipping = 0;
                    coupon.status = "NOT-ACTIVE";
                    coupon.totalDiscount = coupon.discount + coupon.shipping;
                }
                TempCouponsList.Add(coupon);
            }

            var activeCoupons = new List<couponItem>();
            var nonactiveCoupons = new List<couponItem>();

            foreach (couponItem a in TempCouponsList)
            {
                if (a.status == "ACTIVE")
                {
                    activeCoupons.Add(a);
                }
                else
                {
                    nonactiveCoupons.Add(a);
                }
            }

            TempCouponsList.Clear();
            // TESTING
            //Debug.WriteLine("");
            //Debug.Write("LIST OF ACTIVE COUPONS: ");
            //foreach (couponItem e in activeCoupons)
            //{
            //    Debug.Write(e.couponId + " ");

            //}
            //Debug.WriteLine("");
            //Debug.WriteLine("");
            //Debug.Write("List OF NONACTIVE COUPONS: ");
            //foreach (couponItem e in nonactiveCoupons)
            //{
            //    Debug.Write(e.couponId + " ");
            //}

            //Debug.WriteLine("");
            //Debug.WriteLine("");

            // ALL COUPOUNS ARE ACTIVE
            if (nonactiveCoupons.Count == 0)
            {
                // PLACE TOTAL DISCOUNT VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                var TotalDiscountArray = new List<double>();

                // TotalDiscountArray IS OUT OF ORDER FOR EXAMPLE = [5,3,6,4,5,2,1,]
                foreach (couponItem ActiveCoupon in activeCoupons)
                {
                    TotalDiscountArray.Add(ActiveCoupon.totalDiscount);
                }

                // TotalDiscountArray IS IN ORDER = [1,2,3,4,5,6]
                TotalDiscountArray.Sort();

                Debug.WriteLine("");
                Debug.Write("SORTED TOTAL DISCOUNT VALUES: ");
                foreach (double Value in TotalDiscountArray)
                {
                    Debug.Write(Value + " ");
                }

                // RECORD REPETITIONS + READING SORTED ARRAY BACKWARDS + WRITING THE RIGHT MESSAGE ON THE COUPON
                var Limit = TotalDiscountArray.Count;
                var LastIndex = Limit - 1;
                for (int i = 0; i < Limit; i++)
                {
                    var Value = TotalDiscountArray[LastIndex];
                    for (int j = 0; j < activeCoupons.Count; j++)
                    {
                        if (Value == activeCoupons[j].totalDiscount)
                        {
                            activeCoupons[j].image = "CouponIconGreen.png";
                            activeCoupons[j].savingsOrSpendingNote = "You saved: $" + activeCoupons[j].totalDiscount.ToString("N2");
                            TempCouponsList.Add(activeCoupons[j]);
                            activeCoupons.RemoveAt(j);
                            break;
                        }
                    }
                    LastIndex--;
                }
            }

            // ALL COUPONS ARE NON-ACTIVE
            if (activeCoupons.Count == 0)
            {
                // PLACE THRESHOLD VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                var ThresholdArray = new List<double>();
                foreach (couponItem NonActiveCoupon in nonactiveCoupons)
                {
                    ThresholdArray.Add(NonActiveCoupon.threshold);
                }

                ThresholdArray.Sort();

                var Limit = nonactiveCoupons.Count;
                var FirstIndex = 0;
                for (int i = 0; i < Limit; i++)
                {
                    var Value = ThresholdArray[FirstIndex];
                    for (int j = 0; j < nonactiveCoupons.Count; j++)
                    {
                        if (Value == nonactiveCoupons[j].threshold)
                        {
                            nonactiveCoupons[j].savingsOrSpendingNote = "Spend $" + (nonactiveCoupons[j].threshold - initialSubTotal).ToString("N2") + " more to use";
                            TempCouponsList.Add(nonactiveCoupons[j]);
                            nonactiveCoupons.RemoveAt(j);
                            break;
                        }
                    }
                    FirstIndex++;
                }
            }

            // COUPONS ARE ACTIVE AND NON-ACTIVE
            if (activeCoupons.Count != 0 && nonactiveCoupons.Count != 0)
            {
                // PLACE TOTAL DISCOUNT VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                var TotalDiscountArray = new List<double>();

                // TotalDiscountArray IS OUT OF ORDER FOR EXAMPLE = [5,3,6,4,5,2,1,]
                foreach (couponItem ActiveCoupon in activeCoupons)
                {
                    TotalDiscountArray.Add(ActiveCoupon.totalDiscount);
                }

                // TotalDiscountArray IS IN ORDER = [1,2,3,4,5,6]
                TotalDiscountArray.Sort();

                Debug.WriteLine("");
                Debug.Write("SORTED TOTAL DISCOUNT VALUES: ");
                foreach (double Value in TotalDiscountArray)
                {
                    Debug.Write(Value + " ");
                }

                // RECORD REPETITIONS + READING SORTED ARRAY BACKWARDS + WRITING THE RIGHT MESSAGE ON THE COUPON
                var Limit = TotalDiscountArray.Count;
                var LastIndex = Limit - 1;
                for (int i = 0; i < Limit; i++)
                {
                    var Value = TotalDiscountArray[LastIndex];
                    for (int j = 0; j < activeCoupons.Count; j++)
                    {
                        if (Value == activeCoupons[j].totalDiscount)
                        {
                            activeCoupons[j].image = "CouponIconGreen.png";
                            activeCoupons[j].savingsOrSpendingNote = "You saved: $" + activeCoupons[j].totalDiscount.ToString("N2");
                            TempCouponsList.Add(activeCoupons[j]);
                            activeCoupons.RemoveAt(j);
                            break;
                        }
                    }
                    LastIndex--;
                }

                // PLACE THRESHOLD VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
                var ThresholdArray = new List<double>();
                foreach (couponItem NonActiveCoupon in nonactiveCoupons)
                {
                    ThresholdArray.Add(NonActiveCoupon.threshold);
                }

                ThresholdArray.Sort();

                Limit = nonactiveCoupons.Count;
                var FirstIndex = 0;
                for (int i = 0; i < Limit; i++)
                {
                    var Value = ThresholdArray[FirstIndex];
                    for (int j = 0; j < nonactiveCoupons.Count; j++)
                    {
                        if (Value == nonactiveCoupons[j].threshold)
                        {
                            nonactiveCoupons[j].savingsOrSpendingNote = "Spend $" + (nonactiveCoupons[j].threshold - initialSubTotal).ToString("N2") + " more to use";
                            TempCouponsList.Add(nonactiveCoupons[j]);
                            nonactiveCoupons.RemoveAt(j);
                            break;
                        }
                    }
                    FirstIndex++;
                }
            }

            foreach(couponItem a in TempCouponsList)
            {
                couponsList.Add(a);
            }

            for (int i = 0; i < couponsList.Count; i++)
            {
                couponsList[i].index = i;
            }

            if (couponsList.Count != 0)
            {
                if (couponsList[0].status == "ACTIVE")
                {
                    couponsList[0].image = "CouponIconOrange.png";
                    Debug.WriteLine("COUPON DISCOUNT: {0}, COUPON SHIPPING: {1}", couponsList[0].discount, couponsList[0].shipping);
                    updateTotals(couponsList[0].discount, couponsList[0].shipping);
                    appliedIndex = 0;
                }
                else
                {
                    updateTotals(0, 0);
                }
            }
            else
            {
                updateTotals(0, 0);
            }

            coupon_list.ItemsSource = couponsList;
        }
   
        public void openHistory(object sender, EventArgs e)
        {
            if (!(bool)Application.Current.Properties["guest"])
            {
                if(deliveryDay != "")
                {
                    Application.Current.MainPage = new HistoryPage(deliveryDay);
                }
                else
                {
                    Application.Current.MainPage = new HistoryPage();
                }
                
            }
        }

        public void ChangeAddressClick(System.Object sender, System.EventArgs e)
        {
            //addresframe.Height = this.Height / 2;
        }

        void DriverTip_Completed(System.Object sender, System.EventArgs e)
        {
            //DriverTip.Focus();
            Debug.WriteLine("Tap on tip driverTip_completed");
            UpdateTotalAmount(sender, e);
        }

        void DriverTip_Focused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            Debug.WriteLine("Focus");
        }


        void DriverTip_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            UpdateTotalAmount(sender, e);
        }

        void UpdateTotalAmount(System.Object sender, System.EventArgs e)
        {
            Debug.WriteLine("TOUCH TO UPDATE");
            if (total != 0)
            {
                if(appliedIndex != -1)
                {
                    updateTotals(couponsList[appliedIndex].discount, couponsList[appliedIndex].shipping);
                }
                else
                {
                    updateTotals(0, 0);
                }
            }
            else
            {
                updateTotals(0, 0);
            }
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
            if (!(bool)Application.Current.Properties["guest"])
            {
                Application.Current.MainPage = new InfoPage();
            }
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            if (!(bool)Application.Current.Properties["guest"])
            {
                Application.Current.MainPage = new ProfilePage();
            }
            
        }

        public async void ValidateAddressClick(System.Object sender, System.EventArgs e)
        {
            if (newUserAddress.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter an address", "OK");
            }

            if (newUserUnitNumber.Text == null)
            {
                newUserUnitNumber.Text = "";
            }

            if(newUserCity.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter a city", "OK");
            }

            if(newUserState.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter a state", "OK");
            }

            if(newUserZipcode.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter a zipcode", "OK");
            }

            // Setting request for USPS API
            XDocument requestDoc = new XDocument(
                new XElement("AddressValidateRequest",
                new XAttribute("USERID", "400INFIN1745"),
                new XElement("Revision", "1"),
                new XElement("Address",
                new XAttribute("ID", "0"),
                new XElement("Address1", newUserAddress.Text.Trim()),
                new XElement("Address2", newUserUnitNumber.Text.Trim()),
                new XElement("City", newUserCity.Text.Trim()),
                new XElement("State", newUserState.Text.Trim()),
                new XElement("Zip5", newUserZipcode.Text.Trim()),
                new XElement("Zip4", "")
                     )
                 )
             );
            var url = "http://production.shippingapis.com/ShippingAPI.dll?API=Verify&XML=" + requestDoc;
            Console.WriteLine(url);
            var client = new WebClient();
            var response = client.DownloadString(url);

            var xdoc = XDocument.Parse(response.ToString());
            Console.WriteLine(xdoc);

            foreach (XElement element in xdoc.Descendants("Address"))
            {
                if (GetXMLElement(element, "Error").Equals(""))
                {
                    if (GetXMLElement(element, "DPVConfirmation").Equals("Y") && GetXMLElement(element, "Zip5").Equals(newUserZipcode.Text.Trim()) && GetXMLElement(element, "City").Equals(newUserCity.Text.Trim().ToUpper())) // Best case
                    {
                        // Get longitude and latitide because we can make a deliver here. Move on to next page.
                        // Console.WriteLine("The address you entered is valid and deliverable by USPS. We are going to get its latitude & longitude");
                        //GetAddressLatitudeLongitude();
                        Geocoder geoCoder = new Geocoder();

                        IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(newUserAddress.Text.Trim() + "," + newUserCity.Text.Trim() + "," + newUserState.Text.Trim());
                        Position position = approximateLocations.FirstOrDefault();

                        latitude = $"{position.Latitude}";
                        longitude = $"{position.Longitude}";

                        map.MapType = MapType.Street;
                        var mapSpan = new MapSpan(position, 0.001, 0.001);

                        Pin address = new Pin();
                        address.Label = "Delivery Address";
                        address.Type = PinType.SearchResult;
                        address.Position = position;

                        map.MoveToRegion(mapSpan);
                        map.Pins.Add(address);

                        break;
                    }
                    else if (GetXMLElement(element, "DPVConfirmation").Equals("D"))
                    {
                        //await DisplayAlert("Alert!", "Address is missing information like 'Apartment number'.", "Ok");
                        //return;
                    }
                    else
                    {
                        //await DisplayAlert("Alert!", "Seems like your address is invalid.", "Ok");
                        //return;
                    }
                }
                else
                {   // USPS sents an error saying address not found in there records. In other words, this address is not valid because it does not exits.
                    //Console.WriteLine("Seems like your address is invalid.");
                    //await DisplayAlert("Alert!", "Error from USPS. The address you entered was not found.", "Ok");
                    //return;
                }
            }
            if (latitude == "0" || longitude == "0")
            {
                await DisplayAlert("We couldn't find your address", "Please check for errors.", "OK");
            }
            else
            {

                purchaseObject.delivery_address = newUserAddress.Text;
                purchaseObject.delivery_unit = newUserUnitNumber.Text;
                purchaseObject.delivery_city = newUserCity.Text;
                purchaseObject.delivery_state = newUserState.Text;
                purchaseObject.delivery_zip = newUserZipcode.Text;
                purchaseObject.delivery_latitude = latitude;
                purchaseObject.delivery_longitude = longitude;
                isAddressValidated = true;
                addressButton.Text = "Address Verified";
                addressButton.BackgroundColor = Color.FromHex("#136D74");
                await Application.Current.SavePropertiesAsync();
            }
        }

        public static string GetXMLElement(XElement element, string name)
        {
            var el = element.Element(name);
            if (el != null)
            {
                return el.Value;
            }
            return "";
        }

        public static string GetXMLAttribute(XElement element, string name)
        {
            var el = element.Attribute(name);
            if (el != null)
            {
                return el.Value;
            }
            return "";
        }

        async void SaveAddressClick(System.Object sender, System.EventArgs e)
        {
            //addresframe.Height = 0;
            if (isAddressValidated)
            {
                Application.Current.Properties["user_address"] = newUserAddress.Text;
                Application.Current.Properties["user_unit"] = newUserUnitNumber.Text;
                Application.Current.Properties["user_city"] = newUserCity.Text;
                Application.Current.Properties["user_state"] = newUserState.Text;
                Application.Current.Properties["user_zip_code"] = newUserZipcode.Text;
                Application.Current.Properties["user_latitude"] = latitude;
                Application.Current.Properties["user_longitude"] = longitude;

                string address = (string)Application.Current.Properties["user_address"];
                string city = (string)Application.Current.Properties["user_city"];
                string state = (string)Application.Current.Properties["user_state"];
                string zipcode = (string)Application.Current.Properties["user_zip_code"];

                DeliveryAddress1.Text = address;
                DeliveryAddress2.Text = city + ", " + state + ", " + zipcode;

                ResetChangeAddress();
            }
            else
            {
                await DisplayAlert("Oops!", "We weren't able to save your changes","OK");
            }
        }

        public void ResetChangeAddress()
        {
            newUserAddress.Text = "";
            newUserUnitNumber.Text = "";
            newUserCity.Text = "";
            newUserState.Text = "";
            newUserZipcode.Text = "";
            latitude = "0";
            longitude = "0";
        }

        public void ChangeContactInfoClick(System.Object sender, System.EventArgs e)
        {
            //contactframe.Height = this.Height / 2;
        }

        void ChangeContactInfoCancelClick(System.Object sender, System.EventArgs e)
        {
            //contactframe.Height = 0;
        }

        void ChangeAddressCancelClick(System.Object sender, System.EventArgs e)
        {
            //addresframe.Height = 0;
        }

        async void SaveChangesClick(System.Object sender, System.EventArgs e)
        {
            //contactframe.Height = 0;

            //if(newUserFirstName.Text == null || newUserLastName.Text == null || newUserPhoneNum.Text == null || newUserEmailAddress.Text == null)
            //{
            //    await DisplayAlert("Oops","You forgot to enter your first name, last name, phone number, or email address", "OK");
            //    return;
            //}
            //if (newUserFirstName.Text != null)
            //{
            //    Application.Current.Properties["user_first_name"] = newUserFirstName.Text.Trim();
            //    purchaseObject.delivery_first_name = newUserFirstName.Text.Trim();
            //}

            //if (newUserLastName.Text != null)
            //{
            //    Application.Current.Properties["user_last_name"] = newUserLastName.Text.Trim();
            //    purchaseObject.delivery_last_name = newUserLastName.Text.Trim();
            //}

            //if (newUserPhoneNum.Text != null)
            //{
            //    Application.Current.Properties["user_phone_num"] = newUserPhoneNum.Text.Trim();
            //    purchaseObject.delivery_phone_num = newUserPhoneNum.Text.Trim();
            //}

            //if (newUserEmailAddress.Text != null)
            //{
            //    Application.Current.Properties["user_email"] = newUserEmailAddress.Text.Trim();
            //    purchaseObject.delivery_email = newUserEmailAddress.Text.Trim(); 
            //}

            string firstName = (string)Application.Current.Properties["user_first_name"];
            string lastName = (string)Application.Current.Properties["user_last_name"];

            //FullName.Text = firstName + " " + lastName;
            //PhoneNumber.Text = (string)Application.Current.Properties["user_phone_num"];
            //EmailAddress.Text = (string)Application.Current.Properties["user_email"];

            ResetContactInfo();
        }

        public void ResetContactInfo()
        {
            //newUserFirstName.Text = "";
            //newUserLastName.Text = "";
            //newUserPhoneNum.Text = "";
            //newUserEmailAddress.Text = "";
        }

        public async static Task<HttpResponse> createOrder(string amount)
        {
            HttpResponse response;
            // Construct a request object and set desired parameters
            // Here, OrdersCreateRequest() creates a POST request to /v2/checkout/orders


            var order = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>()
                    {
                        new PurchaseUnitRequest()
                        {
                            AmountWithBreakdown = new AmountWithBreakdown()
                            {
                                CurrencyCode = "USD",
                                Value = amount
                            }
                        }
                    },
                ApplicationContext = new ApplicationContext()
                {
                    ReturnUrl = "https://servingfresh.me",
                    CancelUrl = "https://servingfresh.me"
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);
            response = await client().Execute(request);

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    var order = new OrderRequest()
            //    {
            //        CheckoutPaymentIntent = "CAPTURE",
            //        PurchaseUnits = new List<PurchaseUnitRequest>()
            //        {
            //            new PurchaseUnitRequest()
            //            {
            //                AmountWithBreakdown = new AmountWithBreakdown()
            //                {
            //                    CurrencyCode = "USD",
            //                    Value = amount
            //                }
            //            }
            //        },
            //        ApplicationContext = new ApplicationContext()
            //        {
            //            ReturnUrl = "https://servingfresh.me",
            //            CancelUrl = "https://servingfresh.me"
            //        }
            //    };

            //    var request = new OrdersCreateRequest();
            //    request.Prefer("return=representation");
            //    request.RequestBody(order);
            //    response = await client().Execute(request);
            //}
            //else
            //{
            //    var order = new OrderRequest()
            //    {
            //        CheckoutPaymentIntent = "CAPTURE",
            //        PurchaseUnits = new List<PurchaseUnitRequest>()
            //        {
            //            new PurchaseUnitRequest()
            //            {
            //                AmountWithBreakdown = new AmountWithBreakdown()
            //                {
            //                    CurrencyCode = "USD",
            //                    Value = amount
            //                }
            //            }
            //        },
            //        ApplicationContext = new ApplicationContext()
            //        {
            //            //ReturnUrl = "https://devblogs.microsoft.com/xamarin",
            //            //CancelUrl = "https://devblogs.microsoft.com/xamarin"
            //            ReturnUrl = "https://servingfresh.me",
            //            CancelUrl = "https://servingfresh.me"
            //        }
            //    };

            //    var request = new OrdersCreateRequest();
            //    request.Prefer("return=representation");
            //    request.RequestBody(order);
            //    response = await client().Execute(request);
            //}

            // Call API with your client and get a response for your call
            //var request = new OrdersCreateRequest();
            //request.Prefer("return=representation");
            //request.RequestBody(order);
            //response = await client().Execute(request);
            return response;
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            var source = webViewPage.Source as UrlWebViewSource;
            Debug.WriteLine("Source From WebView: " + source.Url);
            captureOrder(OrderId);
        }

        public async Task<HttpResponse> captureOrder(string id)
        {
            // Construct a request object and set desired parameters
            // Replace ORDER-ID with the approved order id from create order
            // UserDialogs.Instance.Loading("Processing Payment...");
            Debug.WriteLine("id: " + id);
            var request = new OrdersCaptureRequest(id);
            request.RequestBody(new OrderActionRequest());
            
            HttpResponse response = await client().Execute(request);
            var statusCode = response.StatusCode;
            string code = statusCode.ToString();
            Debug.WriteLine("StatusCode: " + code);
            PayPalCheckoutSdk.Orders.Order result = response.Result<PayPalCheckoutSdk.Orders.Order>();
            Debug.WriteLine("Status: " +  result.Status);
            Debug.WriteLine("Capture Id: "+  result.Id);
            Debug.WriteLine("id: " + id);
            
            if(result.Status == "COMPLETED")
            {
                Debug.WriteLine("WRITE DATA TO DATA BASE");
                // Successful Payment
                //await DisplayAlert("Congratulations", "Payment was successful. We appreciate your business", "OK");
                //ClearCardInfo();

                if(deliveryInstructions.Text == null)
                // if(deliveryInstructions.Text == null)
                {
                    Debug.WriteLine("PAYPAL");
                    Debug.WriteLine("DELIVERY INSTRUCTIONS WERE NOT SET");
                    purchaseObject.delivery_instructions = "";
                }
                else
                {
                    //purchaseObject.delivery_instructions = deliveryInstructions.Text;
                    purchaseObject.delivery_instructions = "";
                }
                


                // purchaseObject.delivery_instructions = deliveryInstructions.Text;
                purchaseObject.subtotal = GetSubTotal().ToString("N2");
                purchaseObject.service_fee = service_fee.ToString("N2");
                purchaseObject.delivery_fee = delivery_fee_db.ToString("N2");
                purchaseObject.driver_tip = driver_tips.ToString("N2");
                purchaseObject.taxes = GetTaxes().ToString("N2");

                var purchaseString = JsonConvert.SerializeObject(purchaseObject);
                Debug.WriteLine("Purchase: " + purchaseString);
                var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
                var client = new System.Net.Http.HttpClient();

                //CouponObject coupon = new CouponObject();
                //coupon.coupon_uid = couponData.result[defaultCouponIndex].coupon_uid;

                //var couponSerialized = JsonConvert.SerializeObject(coupon);
                //System.Diagnostics.Debug.WriteLine("Coupon to update: " + couponSerialized);
                //var couponContent = new StringContent(couponSerialized, Encoding.UTF8, "application/json");

                var Response = await client.PostAsync(Constant.PurchaseUrl, purchaseMessage);
                //var RDSCouponResponse = await client.PostAsync(Constant.UpdateCouponUrl, couponContent);
                Debug.WriteLine("RESPONSE" + Response.Content);
                Debug.WriteLine("REASON PHRASE: " + Response.ReasonPhrase);
                Debug.WriteLine("Order was written to DB: " + Response.IsSuccessStatusCode);
                //Debug.WriteLine("Coupon was succesfully updated (subtract)" + RDSCouponResponse);
                if (Response.IsSuccessStatusCode)
                {
                    var RDSResponseContent = await Response.Content.ReadAsStringAsync();
                    //System.Diagnostics.Debug.WriteLine(RDSResponseContent);

                    cartItems.Clear();
                    updateTotals(0, 0);
                    total = 00.00;
                    total_qty = 0;
                    Application.Current.Properties["day"] = "";
                    cartEmpty = "EMPTY";
                    //cartHeight.Height = 0;
                    if (!(bool)Application.Current.Properties["guest"])
                    {
                        Application.Current.MainPage = new HistoryPage("SUCCESS");
                        //await DisplayAlert("We appreciate your business", "Thank you for placing an order through Serving Fresh! Our Serving Fresh Team is processing your order!", "OK");
                    }
                    else
                    {
                        Application.Current.Properties["guest"] = false;

                        var firstName = (string)Application.Current.Properties["user_first_name"];
                        var lastName = (string)Application.Current.Properties["user_last_name"];
                        var email = (string)Application.Current.Properties["user_email"];
                        var phone = (string)Application.Current.Properties["user_phone_num"];
                        var address = (string)Application.Current.Properties["user_address"];
                        var unit = (string)Application.Current.Properties["user_unit"];
                        var city = (string)Application.Current.Properties["user_city"];
                        var zipcode = (string)Application.Current.Properties["user_zip_code"];
                        var state = (string)Application.Current.Properties["user_state"];
                        var lat = (string)Application.Current.Properties["user_latitude"];
                        var longitude = (string)Application.Current.Properties["user_longitude"];

                        Application.Current.MainPage = new SignUpPage(firstName, lastName, phone, email, address, unit, city, state, zipcode, "guest", lat, longitude);
                    }
                }
                //UserDialogs.Instance.HideLoading();
            }
            else
            {
                //UserDialogs.Instance.HideLoading();
                await DisplayAlert("Oops", "Your payment was canceled or was not successful. Please try again", "OK");
            }

            return response;
        }


        void GetDeliveryTips(System.Object sender, System.EventArgs e)
        {

            foreach (View element in driverTipOptions.Children)
            {
                var tip = (Button)element;
                tip.BackgroundColor = Color.White;
                tip.TextColor = Color.Black;
            }

            var tipOption = (Button)sender;
            tipOption.BackgroundColor = Color.FromHex("#FF8500");
            tipOption.TextColor = Color.White;

            SetTips(tipOption.Text);
            UpdateTotalAmount(sender, e);
        }

        void SetTips(string value)
        {
            if(value == "No tip")
            {
                driver_tips = 0;
                DriverTip.Text = "$0.00";
            }
            else if(value == "$2")
            {
                driver_tips = 2.0;
                DriverTip.Text = "$2.00";
            }
            else if (value == "$3")
            {
                driver_tips = 3.0;
                DriverTip.Text = "$3.00";
            }
            else if (value == "$5")
            {
                driver_tips = 5.0;
                DriverTip.Text = "$5.00";
            }
        }



        void CheckoutWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
                customerStripeInformationView.HeightRequest = 194;

            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
                customerStripeInformationView.HeightRequest = 0;
            }
        }

        void CheckoutWithPayPal(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
            }
        }

        void CompletePaymentWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
            }
        }



        void TapGestureRecognizer_Tapped_1(System.Object sender, System.EventArgs e)
        {
            string source = webViewPage.Source.ToString();

            Debug.WriteLine("Source From WebView: " + source);
        }

        void TapGestureRecognizer_Tapped_2(System.Object sender, System.EventArgs e)
        {
            string source = webViewPage.Source.ToString();
            
            Debug.WriteLine("Source From WebView: " + source);
        }


        void AvailableItem(string imageSource, string name, string quantity, string price)
        {

            Grid grid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 0,
                RowDefinitions =
            {
                new RowDefinition(),
                new RowDefinition(),
                new RowDefinition()
            },
                ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition()
            }
            };

            var gridView = new Grid
            {
                ColumnSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition()
                    {
                        Height = new GridLength(1, GridUnitType.Auto)
                    }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition()
                    {
                        Width = 31
                    },
                    new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    },
                    new ColumnDefinition()
                    {
                        Width = 80
                    },
                    new ColumnDefinition()
                    {
                        Width = 60
                    },
                }
                
            };
            //gridView.ColumnSpacing = 0;

            //gridView.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            //gridView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(31) });
            //gridView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            //gridView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
            //gridView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });



            var itemImageView = new StackLayout();
            itemImageView.VerticalOptions = LayoutOptions.Center;
            
            var imageFrame = new Frame();

            imageFrame.Padding = 0;
            imageFrame.HeightRequest = 31;
            imageFrame.HasShadow = false;
            imageFrame.IsClippedToBounds = true;
            

            var image = new Image();
            image.Source = imageSource;
            image.Aspect = Aspect.Fill;
            imageFrame.Content = image;
            itemImageView.Children.Add(imageFrame);
            
            var itemNameView = new StackLayout();
            itemNameView.VerticalOptions = LayoutOptions.Center;
            itemNameView.Padding = new Thickness(10, 0, 10, 0);

            var labelNameView = new Label();
            labelNameView.Text = name;
            labelNameView.MaxLines = 2;
            labelNameView.LineBreakMode = LineBreakMode.TailTruncation;
            labelNameView.TextColor = Color.Black;
            labelNameView.FontSize = 10;
            var v = 1;
            if (v == 0)
            {
                labelNameView.TextDecorations = TextDecorations.Strikethrough;
            }


            itemNameView.Children.Add(labelNameView);

            var itemQuantityView = new StackLayout();
            itemQuantityView.Orientation = StackOrientation.Horizontal;
            itemQuantityView.VerticalOptions = LayoutOptions.Center;

            var minusButtonView = new Button();
            minusButtonView.Text = "-";
            minusButtonView.Padding = new Thickness(0, -3, 0, 0);
            minusButtonView.TextColor = Color.FromHex("#FF8500");
            minusButtonView.FontSize = 22;
            minusButtonView.FontAttributes = FontAttributes.Bold;
            minusButtonView.HeightRequest = 22;
            minusButtonView.WidthRequest = 24;
            minusButtonView.VerticalOptions = LayoutOptions.CenterAndExpand;
            minusButtonView.BackgroundColor = Color.FromHex("#2B6D74");

            var labelQuantityView = new Label();
            labelQuantityView.Text = quantity;
            labelQuantityView.TextColor = Color.Black;
            labelQuantityView.WidthRequest = 35;
            labelQuantityView.HorizontalTextAlignment = TextAlignment.Center;
            labelQuantityView.VerticalTextAlignment = TextAlignment.Center;
            labelQuantityView.FontSize = 18;
            labelQuantityView.FontAttributes = FontAttributes.Bold;

            var plusButtonView = new Button();
            plusButtonView.Text = "+";
            plusButtonView.Padding = new Thickness(0, -3, 0, 0);
            plusButtonView.TextColor = Color.FromHex("#FF8500");
            plusButtonView.FontSize = 22;
            plusButtonView.FontAttributes = FontAttributes.Bold;
            plusButtonView.HeightRequest = 22;
            plusButtonView.WidthRequest = 24;
            plusButtonView.VerticalOptions = LayoutOptions.CenterAndExpand;
            plusButtonView.BackgroundColor = Color.FromHex("#2B6D74");

            itemQuantityView.Children.Add(minusButtonView);
            itemQuantityView.Children.Add(labelQuantityView);
            itemQuantityView.Children.Add(plusButtonView);

            var itemPriceView = new StackLayout();
            var labelPriceView = new Label();

            itemPriceView.VerticalOptions = LayoutOptions.Center;
            labelPriceView.Text = "$" + price;
            labelPriceView.TextColor = Color.Black;
            labelPriceView.HorizontalTextAlignment = TextAlignment.End;
            labelPriceView.FontSize = 10;
            labelPriceView.WidthRequest = 60;

            if (v == 0)
            {
                labelPriceView.TextDecorations = TextDecorations.Strikethrough;
            }
            itemPriceView.Children.Add(labelPriceView);

            gridView.Children.Add(itemImageView, 0, 0);
            gridView.Children.Add(itemNameView, 1, 0);
            
            if (v == 0)
            {
                gridView.Children.Add(itemQuantityView, 2, 0);
            }
            gridView.Children.Add(itemQuantityView, 2, 0);
            
            gridView.Children.Add(itemPriceView, 3, 0);

            var seperator = new BoxView();
            seperator.HeightRequest = 1;
            seperator.Color = Color.LightGray;

            var unvailableLabelView = new Label()
            {
                Margin = new Thickness(0, -15, 0, 0),
                Text = "Item unavaiable on this day",
                TextColor = Color.FromHex("#FF8500"),
                FontSize = 10,
                HorizontalTextAlignment = TextAlignment.End,
            };

            //itemList.Children.Add(gridView);

            if (v == 0)
            {
                //itemList.Children.Add(unvailableLabelView);
            }

            //itemList.Children.Add(seperator);
        }

        void ProceedAsGuest(System.Object sender, System.EventArgs e)
        {
            checkoutAsync(sender,e);

            purchaseObject.subtotal = GetSubTotal().ToString("N2");
            purchaseObject.service_fee = service_fee.ToString("N2");
            purchaseObject.delivery_fee = delivery_fee_db.ToString("N2");
            purchaseObject.driver_tip = driver_tips.ToString("N2");
            purchaseObject.taxes = GetTaxes().ToString("N2");
            purchaseObject.delivery_instructions = deliveryInstructions.Text;

            Application.Current.MainPage = new DeliveryDetailsPage(purchaseObject, this.deliveryInfo);
        }

        //void EntryClick(System.Object sender, System.EventArgs e)
        //{
        //    Debug.WriteLine("Tip typing");
        //}
    }
}
