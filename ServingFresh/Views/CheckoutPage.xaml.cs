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
using Application = Xamarin.Forms.Application;

using System.Threading.Tasks;
using Acr.UserDialogs;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.Views.SignUpPage;
using Xamarin.Auth;

namespace ServingFresh.Views
{

    public class User2
    {
        public string delivery_instructions { get; set; }
    }

    public class FavoritePost
    {
        public string customer_uid { get; set; }
        public IList<string> favorite { get; set; }
    }

    public class InfoObject
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<User2> result { get; set; }
        public string sql { get; set; }
    }


    public class PurchaseResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string sql { get; set; }
    }
    public partial class CheckoutPage : ContentPage
    {

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


        public static Purchase purchase = new Purchase(user);
        public static ObservableCollection<ItemObject> cartItems = new ObservableCollection<ItemObject>();
        public ObservableCollection<CouponItem> couponsList = new ObservableCollection<CouponItem>();
        public ObservableCollection<CouponItem> availableCoupons = new ObservableCollection<CouponItem>();
        public CouponItem appliedCoupon = null;

        public ObservableCollection<CouponItem> TempCouponsList = new ObservableCollection<CouponItem>();
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
        private AddressAutocomplete addressToValidate = null;
        // Coupons Lists
        private CouponResponse couponData = null;
        private List<double> unsortedNewTotals = new List<double>();
        private List<double> unsortedThresholds = new List<double>();
        private List<double> unsortedDiscounts = new List<double>();
        private List<double> sortedDiscounts = new List<double>();
        private int appliedIndex = -1;
        double savings = 0;
        public string deliveryDay = "";
        public Dictionary<string, ItemPurchased> orderCopy = new Dictionary<string,ItemPurchased>();
        public string cartEmpty = "";

        private Payments paymentClient = null;

        // =========================

        public CheckoutPage(string another)
        {
            InitializeComponent();
            if (user.getUserType() == "GUEST")
            {
                customerPaymentsView.HeightRequest = 0;
                customerStripeInformationView.HeightRequest = 0;
                customerDeliveryAddressView.HeightRequest = 0;
            }
            else
            {
                guestAddressInfoView.HeightRequest = 0;
                guestPaymentsView.HeightRequest = 0;
            }
        }

        public CheckoutPage()
        {
            InitializeComponent();
            SelectionPage.SetMenu(guestMenuSection, customerMenuSection, historyLabel, profileLabel);

            if (selectedDeliveryDate != null && order.Count != 0)
            {
                expectedDelivery.Text = selectedDeliveryDate.deliveryTimeStamp.ToString("dddd, MMM dd, yyyy, ") + " between " + selectedDeliveryDate.delivery_time;
                GetFees(selectedDeliveryDate.delivery_dayofweek, zone);
                GetAvailiableCoupons(user);
                if (user.getUserType() == "GUEST")
                {
                    InitializeMap();
                    customerPaymentsView.HeightRequest = 0;
                    customerStripeInformationView.HeightRequest = 0;
                    customerDeliveryAddressView.HeightRequest = 0;
                }
                else
                {
                    DeliveryAddress1.Text = purchase.getPurchaseAddress();
                    DeliveryAddress2.Text = purchase.getPurchaseCity() + ", " + purchase.getPurchaseState() + ", " + purchase.getPurchaseZipcode();
                    guestAddressInfoView.HeightRequest = 0;
                    guestPaymentsView.HeightRequest = 0;
                }

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
                    orderCopy.Add(key, order[key]);
                }

                CartItems.ItemsSource = cartItems;
                CartItems.HeightRequest = 50 * cartItems.Count;

                updateTotals(0, 0);
            }
            else
            {
                expectedDelivery.Text = "";
            }
        }

        public async Task<string> GetDeliveryInstructions(User user)
        {

            var result = "";
            var client = new System.Net.Http.HttpClient();
            var ID = user.getUserID();
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
                        result = data.result[0].delivery_instructions;
                    }
                }
            }
            return result;
        }

        public void InitializeMap()
        {
            map.MapType = MapType.Street;
            Position point = new Position(37.334789, -121.888138);
            var mapSpan = new MapSpan(point, 5, 5);
            map.MoveToRegion(mapSpan);
        }

        public async void GetFees(string day, string zone)
        {
            try
            {
                var client = new System.Net.Http.HttpClient();

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

        public async void GetAvailiableCoupons(User user)
        {
            var client = new System.Net.Http.HttpClient();
            var email = user.getUserEmail();
            var RDSResponse = new HttpResponseMessage();
            if (user.getUserType() != "GUEST")
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

                    var coupon = new CouponItem();
                    //Debug.WriteLine("COUPON IDS: " + c.coupon_uid);
                    coupon.couponId = c.coupon_uid;
                    // INITIALLY, THE IMAGE OF EVERY COUPON IS GRAY. (PLATFORM DEPENDENT)
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        coupon.image = "CouponIconGray.png";
                    }
                    else
                    {
                        coupon.image = "nonEligibleCoupon.png";
                    }

                    // SET TITLE LABEL OF COUPON
                    if(c.coupon_title != null)
                    {
                        coupon.title = (string)c.coupon_title;
                    }
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



                //couponsList.Clear();
                var activeCoupons = CouponItem.GetActiveCoupons(couponsList);
                var nonactiveCoupons = CouponItem.GetNonActiveCoupons(couponsList, initialSubTotal);
                CouponItem.SortActiveCoupons(activeCoupons);
                CouponItem.SortNonActiveCoupons(nonactiveCoupons);
                availableCoupons = ApplyBestAvailableCoupon(CouponItem.MergeActiveNonActiveCouponLists(activeCoupons, nonactiveCoupons));
                coupon_list.ItemsSource = availableCoupons;

            }
        }

        public ObservableCollection<CouponItem> ApplyBestAvailableCoupon(ObservableCollection<CouponItem> source)
        {
            if (source != null)
            {
                if (source.Count != 0)
                {
                    if(source[0].status == "ACTIVE")
                    {
                        source[0].image = "appliedCoupon.png";
                        source[0].isCouponEligible = "Applied";
                        source[0].textColor = Color.White;
                        updateTotals(source[0].discount, source[0].shipping);
                        appliedCoupon = source[0];
                    }
                    else
                    {
                        updateTotals(0, 0);
                    }
                }
            }
            return source;
        }

        void NavigateToStoreFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToStore(sender, e);
        }

        void NavigateToHistoryFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToHistory(sender, e);
        }

        void NavigateToRefundsFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToRefunds(sender, e);
        }

        void ApplyActiveCoupon(System.Object sender, System.EventArgs e)
        {
            Debug.WriteLine("APPLY ACTIVE COUPON FUNCTION");
            var elementUI = (RelativeLayout)sender;
            var gestureRecognizer = (TapGestureRecognizer)elementUI.GestureRecognizers[0];
            var selectedElement = (CouponItem)gestureRecognizer.CommandParameter;

            if(selectedElement.status == "ACTIVE")
            {
                foreach(CouponItem coupon in availableCoupons)
                {
                    if(coupon.status == "ACTIVE")
                    {
                        coupon.image = "eligibleCoupon.png";
                        coupon.textColor = Color.Black;
                        coupon.isCouponEligible = "Eligible";
                        
                    }
                    else
                    {
                        coupon.image = "nonEligibleCoupon.png";
                        coupon.textColor = Color.Gray;
                        coupon.isCouponEligible = "Non eligible";
                        
                    }
                    coupon.update();
                }

                selectedElement.image = "appliedCoupon.png";
                selectedElement.textColor = Color.White;
                selectedElement.isCouponEligible = "Applied";
                selectedElement.update();
                appliedCoupon = selectedElement;
                updateTotals(selectedElement.discount, selectedElement.shipping);
            }
        }

        public string GetCouponID()
        {
            var id = "";
            if(appliedCoupon != null)
            {
                id = appliedCoupon.couponId;
            }
            return id;
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
            //viewTotal.Text = "$" + total.ToString("N2");
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
        
        private void FinalizePurchase(Purchase purchase, ScheduleInfo selectedDeliveryDate)
        {
            string dateTime = DateTime.Parse(selectedDeliveryDate.delivery_date).ToString("yyyy-MM-dd");
            string t = selectedDeliveryDate.delivery_time;
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

            purchase.setPurchaseItems(GetOrder(cartItems));
            purchase.setPurchaseDeliveryDate(selectedDeliveryDate.deliveryTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
            purchase.setPurchaseCoupoID(GetCouponID());
            purchase.setPurchaseAmountDue(total.ToString("N2"));
            purchase.setPurchaseDiscount(discount.ToString("N2"));
            purchase.setPurchasePaid(total.ToString("N2")); 
            purchase.setPurchaseAddon("FALSE");
            purchase.setPurchaseSubtotal(GetSubTotal().ToString("N2"));
            purchase.setPurchaseServiceFee(service_fee.ToString("N2"));
            purchase.setPurchaseDeliveryFee(delivery_fee_db.ToString("N2"));
            purchase.setPurchaseDriveTip(driver_tips.ToString("N2"));
            purchase.setPurchaseTaxes(GetTaxes().ToString("N2"));
            purchase.setPurchaseDeliveryInstructions(deliveryInstructions.Text);
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
            
                
                //couponsList.Clear();

                double initialSubTotal = GetSubTotal();
                double initialDeliveryFee = GetDeliveryFee();
                double initialServiceFee = GetServiceFee();
                double initialTaxes = GetTaxes();
                double initialTotal = initialSubTotal + initialDeliveryFee + initialServiceFee + initialTaxes;

                int j = 0;
                foreach (Models.Coupon c in couponData.result)
                {
                    // IF THRESHOLD IS NULL SET IT TO ZERO, OTHERWISE INITIAL VALUE STAYS THE SAME
                    if (c.threshold == null) { c.threshold = 0.0; }
                    double discount = 0;
                    //double newTotal = 0;

                    var coupon = new CouponItem();
                    //Debug.WriteLine("COUPON IDS: " + c.coupon_uid);

                    if (c.coupon_title != null)
                    {
                        coupon.title = (string)c.coupon_title;
                    }

                    coupon.couponId = c.coupon_uid;
                    // INITIALLY, THE IMAGE OF EVERY COUPON IS GRAY. (PLATFORM DEPENDENT)
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        coupon.image = "CouponIconGray.png";
                    }
                    else
                    {
                        coupon.image = "nonEligibleCoupon.png";
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
                    couponsList[j++] = coupon;
                }

                var activeCoupons = CouponItem.GetActiveCoupons(couponsList);
                var nonactiveCoupons = CouponItem.GetNonActiveCoupons(couponsList, initialSubTotal);
                CouponItem.SortActiveCoupons(activeCoupons);
                CouponItem.SortNonActiveCoupons(nonactiveCoupons);
                availableCoupons = ApplyBestAvailableCoupon(CouponItem.MergeActiveNonActiveCouponLists(activeCoupons, nonactiveCoupons));
                coupon_list.ItemsSource = availableCoupons;
        }
   


        public async void ValidateAddressClick(System.Object sender, System.EventArgs e)
        {
            
       
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



        async void CompletePaymentWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
                FinalizePurchase(purchase,selectedDeliveryDate);
                purchase.setPurchasePaymentType("STRIPE");


                string mode = Payments.getMode(purchase.getPurchaseDeliveryInstructions(), "STRIPE");
                paymentClient = new Payments(mode);

                var paymentIsSuccessful = paymentClient.PayViaStripe(
                    purchase.getPurchaseEmail(),
                    cardHolderName.Text,
                    cardHolderNumber.Text,
                    cardCVV.Text,
                    cardExpMonth.Text,
                    cardExpYear.Text,
                    purchase.getPurchaseAmountDue()
                    ); 

                if (paymentIsSuccessful)
                {
                    _ = paymentClient.SendPurchaseToDatabase(purchase);
                    order.Clear();
                    await WriteFavorites(GetFavoritesList(), purchase.getPurchaseCustomerUID());
                    Application.Current.MainPage = new HistoryPage();
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

        void ProceedAsGuest(System.Object sender, System.EventArgs e)
        {
            FinalizePurchase(purchase, selectedDeliveryDate);
            Application.Current.MainPage = new DeliveryDetailsPage();
        }

        void CheckoutViaStripe(System.Object sender, System.EventArgs e)
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

        async void CheckoutViaPayPal(System.Object sender, System.EventArgs e)
        {
            paypalRow.Height = this.Height - 136;

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
                FinalizePurchase(purchase, selectedDeliveryDate);
                string mode = Payments.getMode(purchase.getPurchaseDeliveryInstructions(), "PAYPAL");
                paymentClient = new Payments(mode);
                purchase.setPurchasePaymentType("PAYPAL");
                var paymentIsSuccessful = await paymentClient.captureOrder(paymentClient.getTransactionID());
                await WriteFavorites(GetFavoritesList(), purchase.getPurchaseCustomerUID());
                if (paymentIsSuccessful)
                {
                    _ = paymentClient.SendPurchaseToDatabase(purchase);
                    order.Clear();
                    Application.Current.MainPage = new HistoryPage();
                }
                else
                {
                    await DisplayAlert("Issue with payment via PayPal", "", "OK");
                }
            }
        }


        public static async Task<bool> WriteFavorites(List<string> favorites, string userID)
        {
            var taskResponse = false;
            if(userID != null && userID != "")
            {
                var favoritePost = new FavoritePost()
                {
                    customer_uid = userID,
                    favorite = favorites
                };

                var client = new System.Net.Http.HttpClient();
                var serializedFavoritePostObject = JsonConvert.SerializeObject(favoritePost);
                var content = new StringContent(serializedFavoritePostObject, Encoding.UTF8, "application/json");
                var endpointResponse = await client.PostAsync(Constant.PostUserFavorites, content);

                Debug.WriteLine("FAVORITES CONTENT: " + serializedFavoritePostObject);

                if (endpointResponse.IsSuccessStatusCode)
                {
                    taskResponse = true;
                }
                return taskResponse;
            }
            return taskResponse;
        }

        Models.Address addr = new Models.Address();

        private ObservableCollection<AddressAutocomplete> _addresses;
        public ObservableCollection<AddressAutocomplete> Addresses
        {
            get => _addresses ?? (_addresses = new ObservableCollection<AddressAutocomplete>());
            set
            {
                if (_addresses != value)
                {
                    _addresses = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _addressText;
        public string AddressText
        {
            get => _addressText;
            set
            {
                if (_addressText != value)
                {
                    _addressText = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task GetPlacesPredictionsAsync()
        {
           await addr.GetPlacesPredictionsAsync(addressList, Addresses, AddressEntry.Text);
        }

        private void OnAddressChanged(object sender, EventArgs eventArgs)
        {
            addr.OnAddressChanged(addressList, Addresses, AddressEntry.Text);
        }

        void addressEntryFocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryFocused(addressList, addressFrame);
        }

        void addressEntryUnfocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryUnfocused(addressList, addressFrame);
        }

        async void addressSelected(System.Object sender, SelectedItemChangedEventArgs e)
        {
            addressToValidate = addr.addressSelected(addressList, AddressEntry, addressFrame, newUserUnitNumber, gridAddressView, newUserCity, newUserState, newUserZipcode);
            addressFrame.Margin = new Thickness(0, -75, 0, 0);
            // check if given address is with in zones

            if(addressToValidate != null)
            {
                // ask for unit
                addressToValidate.Unit = newUserUnitNumber.Text;

                var client = new AddressValidation();
                var location = await client.ValidateAddress(addressToValidate.Street, addressToValidate.Unit, addressToValidate.City, addressToValidate.State, addressToValidate.ZipCode);

                if (location != null)
                {
                    var isAddressInZones = await client.isLocationInZones(zone, location.Latitude.ToString(), location.Longitude.ToString());
                    if(isAddressInZones != "INSIDE CURRENT ZONE" && isAddressInZones != "")
                    {
                        // We can continue with payments since we can deliver to this location
                        await DisplayAlert("Great!", "We are able to deliver to your location! Proceed to payments", "OK");
                        client.SetPinOnMap(map, location, addressToValidate.Address);
                        purchase.setPurchaseAddress(addressToValidate.Street);
                        purchase.setPurchaseUnit(addressToValidate.Unit);
                        purchase.setPurchaseCity(addressToValidate.City);
                        purchase.setPurchaseState(addressToValidate.State);
                        purchase.setPurchaseZipcode(addressToValidate.ZipCode);
                        purchase.setPurchaseLatitude(location.Latitude.ToString());
                        purchase.setPurchaseLongitude(location.Longitude.ToString());
                    }
                    else if(isAddressInZones != "INSIDE DIFFERENT ZONE" && isAddressInZones != "")
                    {
                        // We have to reset cart or send user to store
                        await DisplayAlert("Great!", "We are able to deliver to your location! However, your new entered address is outside the initial given address.", "OK");
                    }
                    else if (isAddressInZones != "OUTSIDE ZONE RANGE" && isAddressInZones != "")
                    {
                        // User is outside zones
                        await DisplayAlert("Sorry", "Unfortunately, we can't deliver to this location.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Is your address missing a unit number?", "Please check your address and add unit number if missing", "OK");
                }
            }
        }

        void ShowLogInUI(System.Object sender, System.EventArgs e)
        {
            loginRow.Height = this.Height - 94;
        }

        void ShowSignUpUI(System.Object sender, System.EventArgs e)
        {
            signUpRow.Height = this.Height - 94;
        }

        void HideLogInUI(System.Object sender, System.EventArgs e)
        {
            loginRow.Height = 0;
        }

        void HideSignUpUI(System.Object sender, System.EventArgs e)
        {
            signUpRow.Height = 0;
        }

        void NavigateToInfoFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToInfo(sender, e);
        }

        void NavigateToProfileFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToProfile(sender, e);
        }

        void NagivateToSignInFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToSignIn(sender, e);
        }

        void NagivateToSignUpFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToSignUp(sender, e);
        }

        void NagigateToMainFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToMain(sender, e);
        }

        void ShowMenuFromCheckout(System.Object sender, System.EventArgs e)
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

        void ShowHidePassword(System.Object sender, System.EventArgs e)
        {
            Label label = (Label)sender;
            if(label.Text == "Show password")
            {
                userPassword.IsPassword = false;
                label.Text = "Hide password";
            }
            else
            {
                userPassword.IsPassword = true;
                label.Text = "Show password";
            }
        }

        public void SignInWithFacebook()
        {
            string clientID = string.Empty;
            string redirectURL = string.Empty;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientID = Constant.FacebookiOSClientID;
                    redirectURL = Constant.FacebookiOSRedirectUrl;
                    break;
                case Device.Android:
                    clientID = Constant.FacebookAndroidClientID;
                    redirectURL = Constant.FacebookAndroidRedirectUrl;
                    break;
            }

            var authenticator = new OAuth2Authenticator(clientID, Constant.FacebookScope, new Uri(Constant.FacebookAuthorizeUrl), new Uri(redirectURL), null, false);
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            authenticator.Completed += FacebookAuthenticatorCompleted;
            authenticator.Error += FacebookAutheticatorError;

            presenter.Login(authenticator);
        }

        public void FacebookAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
            else
            {
                Application.Current.MainPage = new PrincipalPage();
                //await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
            }
        }


        private void FacebookAutheticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }
            Application.Current.MainPage = new PrincipalPage();
            //await DisplayAlert("Authentication error: ", e.Message, "OK");
        }
    }
}
