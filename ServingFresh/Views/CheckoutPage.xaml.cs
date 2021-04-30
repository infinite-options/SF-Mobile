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
using static ServingFresh.Views.PrincipalPage;
using Xamarin.Auth;
using ServingFresh.LogIn.Classes;

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
        public ObservableCollection<ItemObject> cartItems = new ObservableCollection<ItemObject>();
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

                    if(user.getUserAddress() != "")
                    {
                        addressGrid.HeightRequest = 230;
                        newUserUnitNumber.IsVisible = true;
                        gridAddressView.IsVisible = true;
                        addressFrame.Margin = new Thickness(0, -190, 0, 0);
                        var validatedAddress = new AddressAutocomplete();

                        validatedAddress.Street = user.getUserAddress();
                        validatedAddress.Unit = user.getUserUnit() == "" ? "" : user.getUserUnit();
                        validatedAddress.City = user.getUserCity();
                        validatedAddress.State = user.getUserState();
                        validatedAddress.ZipCode = user.getUserZipcode();

                        addressToValidate = validatedAddress;
                        addr.addressSelectedFillEntries(addressToValidate, AddressEntry, newUserUnitNumber, newUserCity, newUserState, newUserZipcode);
                        var client = new AddressValidation();
                        client.SetPinOnMap(map, user.getUserLatitude(), user.getUserLongitude(), addressToValidate.Street);
                    }
                }
                else
                {
                    purchase.setPurchaseAddress(user.getUserAddress());
                    purchase.setPurchaseCity(user.getUserCity());
                    purchase.setPurchaseState(user.getUserState());
                    purchase.setPurchaseZipcode(user.getUserZipcode());
                    DeliveryAddress1.Text = purchase.getPurchaseAddress() + ",";
                    DeliveryAddress2.Text = purchase.getPurchaseCity() + ", " + purchase.getPurchaseState() + ", " + purchase.getPurchaseZipcode();
                    guestAddressInfoView.HeightRequest = 0;
                    guestPaymentsView.HeightRequest = 0;
                    addressFrame.Margin = new Thickness(0, 0, 0, 0);
                    guestDeliveryAddressLabel.IsVisible = false;
                    guestMap.IsVisible = false;
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
                SetTips("$2");
                updateTotals(0, 0);
            }
            else
            {
                SetTips("$2");
                GetAvailiableCoupons(user);
                customerPaymentsView.HeightRequest = 0;
                customerStripeInformationView.HeightRequest = 0;
                customerDeliveryAddressView.HeightRequest = 0;
                guestAddressInfoView.HeightRequest = 0;
                guestPaymentsView.HeightRequest = 0;
                expectedDelivery.Text = "";
                
            }
        }

        public async Task<string> GetDeliveryInstructions(Models.User user)
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
            map.Pins.Clear();
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
                        //Constant.deliveryFee = data.result.delivery_fee;
                        //Constant.serviceFee = data.result.service_fee;
                        //Constant.tax_rate = data.result.tax_rate;
                        delivery_fee = data.result.delivery_fee;
                        service_fee = data.result.service_fee;
                    }

                    Debug.WriteLine("Delivery Fee: " + delivery_fee);
                    Debug.WriteLine("Service Fee: " + service_fee);

                    ServiceFee.Text = "$" + service_fee.ToString("N2");
                    DeliveryFee.Text = "$" + delivery_fee.ToString("N2");
                    // GetAvailiableCoupons();
                    SetTips("$2");
                    updateTotals(0, 0);
                }
            }
            catch (Exception fees)
            {
                Debug.WriteLine(fees.Message);
            }
        }

        public async void GetAvailiableCoupons(Models.User user)
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
            return delivery_fee;
        }

        public double GetTaxes()
        {
            return taxes;
        }

        public double GetServiceFee()
        {
            return service_fee;
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
                if (order.ContainsKey(item.name))
                {
                    var itemToUpdate = order[item.name];
                    itemToUpdate.item_quantity = item.qty;
                    order[item.name] = itemToUpdate;
                }
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
                if (order.ContainsKey(item.name))
                {
                    var itemToUpdate = order[item.name];
                    itemToUpdate.item_quantity = item.qty;
                    order[item.name] = itemToUpdate;
                }
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
            if(appliedCoupon != null)
            {
                updateTotals(appliedCoupon.discount, appliedCoupon.shipping);
            }
            else
            {
                updateTotals(0, 0);
            }
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
                    purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                    purchase.setPurchaseChargeID(paymentClient.getTransactionID());
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

        async void ProceedAsGuest(System.Object sender, System.EventArgs e)
        {
            var client = new AddressValidation();
            if(client.ValidateGuestDeliveryInfo(purchase.getPurchaseAddress(), purchase.getPurchaseCity(), purchase.getPurchaseState(), purchase.getPurchaseZipcode(), purchase.getPurchaseLatitude(), purchase.getPurchaseLongitude()))
            {
                FinalizePurchase(purchase, selectedDeliveryDate);
                var button = (Button)sender;

                if (button.BorderColor == Color.FromHex("#FF8500"))
                {
                    bool animate = true;
                    var y = scrollView.ScrollY;
                    y = y + 210;
                    await scrollView.ScrollToAsync(0, y, animate);
                    button.BorderColor = Color.FromHex("#2F787F");
                    guestRequiredInfoView.IsVisible = true;
                    guestCheckoutView.IsVisible = true;

                }
                else
                {
                    button.BorderColor = Color.FromHex("#FF8500");
                    guestCheckoutView.IsVisible = false;
                    guestRequiredInfoView.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("Please verify your address!","It looks like we were not able to validate your address. Make sure that your delivery address shows in the map","OK");
            }
            
        }

        async void GuestCheckoutWithStripe(System.Object sender, System.EventArgs e)
        {
            var client1 = new SignUp();

            if (client1.GuestCheckAllRequiredEntries(firstName, lastName, emailAddress, phoneNumber))
            {

                purchase.setPurchaseFirstName(firstName.Text);
                purchase.setPurchaseLastName(lastName.Text);
                purchase.setPurchaseEmail(emailAddress.Text);
                purchase.setPurchasePhoneNumber(phoneNumber.Text);
                guestCardHolderName.Text = firstName.Text + " " +lastName.Text;
                guestCardZipcode.Text = purchase.getPurchaseZipcode();

                var button = (Button)sender;

                if (button.BorderColor == Color.FromHex("#FF8500"))
                {
                    var y = scrollView.ScrollY + 120;
                    y = y + 60;
                    await scrollView.ScrollToAsync(0, y, true);
                    button.BorderColor = Color.FromHex("#2F787F");
                    guestStripeView.IsVisible = true;
                }
                else
                {
                    button.BorderColor = Color.FromHex("#FF8500");
                    guestStripeView.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
            }
        }

        //await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94), true);

        async void GuestCompletePaymentWithPayPal(System.Object sender, System.EventArgs e)
        {
            var client1 = new SignUp();
            
            if (client1.GuestCheckAllRequiredEntries(firstName, lastName, emailAddress, phoneNumber))
            {

                purchase.setPurchaseFirstName(firstName.Text);
                purchase.setPurchaseLastName(lastName.Text);
                purchase.setPurchaseEmail(emailAddress.Text);
                purchase.setPurchasePhoneNumber(phoneNumber.Text);

                var button = (Button)sender;

                if (button.BackgroundColor == Color.FromHex("#FF8500"))
                {
                    button.BackgroundColor = Color.FromHex("#2B6D74");
                    await Application.Current.MainPage.Navigation.PushModalAsync(new PayPalPage(), true);
                }
                else
                {
                    button.BackgroundColor = Color.FromHex("#FF8500");
                }
            }
            else
            {
                await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
            }
        }

        async void GuestCompletePaymentWithStripe(System.Object sender, System.EventArgs e)
        {
            var client1 = new SignUp();
            if (client1.GuestCheckAllStripeRequiredEntries(guestCardHolderName,guestCardHolderNumber,guestCardCVV,guestCardExpMonth,guestCardExpYear,guestCardZipcode))
            {
                var button = (Button)sender;

                if (button.BackgroundColor == Color.FromHex("#FF8500"))
                {
                    button.BackgroundColor = Color.FromHex("#2B6D74");
                    purchase.setPurchasePaymentType("STRIPE");
                    UserDialogs.Instance.ShowLoading("Your payment is processing...");
                    string mode = Payments.getMode(purchase.getPurchaseDeliveryInstructions(), "STRIPE");
                    paymentClient = new Payments(mode);
                    var client = new SignIn();
                    var isEmailUnused = await client.ValidateExistingAccountFromEmail(purchase.getPurchaseEmail());
                    if (isEmailUnused == null)
                    {
                        var userID = await SignUp.SignUpNewUser(SignUp.GetUserFrom(purchase));
                        if (userID != "")
                        {
                            purchase.setPurchaseCustomerUID(userID);
                            var paymentIsSuccessful = paymentClient.PayViaStripe(
                                purchase.getPurchaseEmail(),
                                guestCardHolderName.Text,
                                guestCardHolderNumber.Text,
                                guestCardCVV.Text,
                                guestCardExpMonth.Text,
                                guestCardExpYear.Text,
                                purchase.getPurchaseAmountDue()
                                );

                            await WriteFavorites(GetFavoritesList(), userID);

                            if (paymentIsSuccessful)
                            {
                                purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                                purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                                purchase.printPurchase();
                                _ = paymentClient.SendPurchaseToDatabase(purchase);
                                order.Clear();
                                UserDialogs.Instance.HideLoading();
                                Application.Current.MainPage = new ConfirmationPage();
                            }
                            else
                            {
                                UserDialogs.Instance.HideLoading();
                                await DisplayAlert("Oop", "It seems that your card is invalid. Try again", "OK");
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
                                UserDialogs.Instance.HideLoading();
                                await DisplayAlert("Great!", "It looks like you already have an account. Please log in to find out if you have any additional discounts", "OK");
                                await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94), true);
                            }
                            else if (role == "GUEST")
                            {
                                // we don't sign up but get user id
                                user.setUserID(isEmailUnused.result[0].customer_uid);
                                purchase.setPurchaseCustomerUID(user.getUserID());
                                user.setUserFromProfile(isEmailUnused);
                                var paymentIsSuccessful = paymentClient.PayViaStripe(
                                    purchase.getPurchaseEmail(),
                                    guestCardHolderName.Text,
                                    guestCardHolderNumber.Text,
                                    guestCardCVV.Text,
                                    guestCardExpMonth.Text,
                                    guestCardExpYear.Text,
                                    purchase.getPurchaseAmountDue()
                                    );

                                await WriteFavorites(GetFavoritesList(), user.getUserID());
                                if (paymentIsSuccessful)
                                {
                                    purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                                    purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                                    _ = paymentClient.SendPurchaseToDatabase(purchase);
                                    order.Clear();
                                    UserDialogs.Instance.HideLoading();
                                    Application.Current.MainPage = new ConfirmationPage();
                                }
                                else
                                {
                                    UserDialogs.Instance.HideLoading();
                                    await DisplayAlert("Oop", "It seems that your card is invalid. Try again", "OK");
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
            else
            {
                await DisplayAlert("Oops","You seem to forgot to fill in all entries. Please fill in all entries to continue","OK");
            }
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
                    purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                    purchase.setPurchaseChargeID(paymentClient.getTransactionID());
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


        public async void OnAddressChanged(object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(AddressEntry.Text))
            {
                if(addressToValidate != null)
                {
                    if (addressToValidate.Street != AddressEntry.Text)
                    {
                        addressList.ItemsSource = await addr.GetPlacesPredictionsAsync(AddressEntry.Text);
                        addressEntryFocused(sender, eventArgs);
                        InitializeMap();
                        purchase.setPurchaseLatitude("");
                        purchase.setPurchaseLongitude("");
                    }
                }
                else
                {
                    addressList.ItemsSource = await addr.GetPlacesPredictionsAsync(AddressEntry.Text);
                    addressEntryFocused(sender, eventArgs);
                    InitializeMap();
                    purchase.setPurchaseLatitude("");
                    purchase.setPurchaseLongitude("");
                }
            }
            else
            {
                addressEntryUnfocused(sender, eventArgs);
                addr.resetAddressEntries(newUserUnitNumber, newUserCity, newUserState, newUserZipcode);
            }
        }

        void addressEntryFocused(object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(AddressEntry.Text))
            {
                addr.addressEntryFocused(addressList, addressFrame);
            }
            else
            {
                addressEntryUnfocused(sender, eventArgs);
            }
        }

        void addressEntryUnfocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryUnfocused(addressList, addressFrame);
        }

        async void addressList_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            addressGrid.HeightRequest = 140 + 40 + 40;
            newUserUnitNumber.IsVisible = true;
            gridAddressView.IsVisible = true;
            addressToValidate = addr.addressSelected(addressList, addressFrame);
            AddressEntry.Text = addressToValidate.Street;
            if(user.getUserType() == "GUEST")
            {
                addressFrame.Margin = new Thickness(0, -105 - 40 - 40, 0, 0);
            }
            else
            {
                addressFrame.Margin = new Thickness(0, - 40 - 40, 0, 0);
            }
            //addressToValidate = addr.addressSelected(addressList, AddressEntry, addressFrame);
            string zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if (zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;
            }
            addr.addressSelectedFillEntries(addressToValidate, AddressEntry, newUserUnitNumber, newUserCity, newUserState, newUserZipcode);
            //addressFrame.IsVisible = false;

            var client = new AddressValidation();
            var addressStatus = client.ValidateAddressString(AddressEntry.Text, newUserUnitNumber.Text, newUserCity.Text, newUserState.Text, newUserZipcode.Text);

            if (addressStatus != null)
            {
                if (addressStatus == "Y" || addressStatus == "S")
                {

                    var location = await client.ConvertAddressToGeoCoordiantes(AddressEntry.Text, newUserUnitNumber.Text, newUserState.Text);
                    if (location != null)
                    {
                        var isAddressInZones = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());

                        if (isAddressInZones != "" && isAddressInZones != "OUTSIDE ZONE RANGE")
                        {
                            addressToValidate.Unit = newUserUnitNumber.Text == null ? "" : newUserUnitNumber.Text;
                            client.SetPinOnMap(map, location, addressToValidate.Street);
                            purchase.setPurchaseAddress(addressToValidate.Street);
                            purchase.setPurchaseUnit(addressToValidate.Unit);
                            purchase.setPurchaseCity(addressToValidate.City);
                            purchase.setPurchaseState(addressToValidate.State);
                            purchase.setPurchaseZipcode(addressToValidate.ZipCode);
                            purchase.setPurchaseLatitude(location.Latitude.ToString());
                            purchase.setPurchaseLongitude(location.Longitude.ToString());
                            if (user.getUserType() == "CUSTOMER")
                            {
                                DeliveryAddress1.Text = purchase.getPurchaseAddress() + ",";
                                DeliveryAddress2.Text = purchase.getPurchaseCity() + ", " + purchase.getPurchaseState() + ", " + purchase.getPurchaseZipcode();
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "You address is outside our delivery areas", "OK");
                            return;
                        }
                    }
                    else
                    {
                        await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                        return;
                    }

                }
                else if (addressStatus == "D")
                {
                    var unit = await DisplayPromptAsync("It looks like your address is missing its unit number", "Please enter your address unit number in the space below", "OK", "Cancel");
                    if (unit != null)
                    {
                        newUserUnitNumber.Text = unit;

                        addressStatus = client.ValidateAddressString(AddressEntry.Text, newUserUnitNumber.Text, newUserCity.Text, newUserState.Text, newUserZipcode.Text);

                        if (addressStatus != null)
                        {
                            if (addressStatus == "Y" || addressStatus == "S")
                            {

                                var location = await client.ConvertAddressToGeoCoordiantes(AddressEntry.Text, newUserUnitNumber.Text, newUserState.Text);
                                if (location != null)
                                {
                                    var isAddressInZones = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());

                                    if (isAddressInZones != "" && isAddressInZones != "OUTSIDE ZONE RANGE")
                                    {
                                        addressToValidate.Unit = newUserUnitNumber.Text == null ? "" : newUserUnitNumber.Text;
                                        client.SetPinOnMap(map, location, addressToValidate.Street);
                                        purchase.setPurchaseAddress(addressToValidate.Street);
                                        purchase.setPurchaseUnit(addressToValidate.Unit);
                                        purchase.setPurchaseCity(addressToValidate.City);
                                        purchase.setPurchaseState(addressToValidate.State);
                                        purchase.setPurchaseZipcode(addressToValidate.ZipCode);
                                        purchase.setPurchaseLatitude(location.Latitude.ToString());
                                        purchase.setPurchaseLongitude(location.Longitude.ToString());
                                        if(user.getUserType() == "CUSTOMER")
                                        {
                                            DeliveryAddress1.Text = purchase.getPurchaseAddress() + ",";
                                            DeliveryAddress2.Text = purchase.getPurchaseCity() + ", " + purchase.getPurchaseState() + ", " + purchase.getPurchaseZipcode();
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "You address is outside our delivery areas", "OK");
                                        return;
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                                    return;
                                }

                            }
                            else if (addressStatus == "D")
                            {
                                 unit = await DisplayPromptAsync("It looks like your address is missing its unit number", "Please enter your address unit number in the space below", "OK", "Cancel");
                                if (unit != null)
                                {
                                    newUserUnitNumber.Text = unit;
                                }
                                return;
                            }
                        }
                    }
                    // let them go and write D in delivery purchase notes
                    return;
                }
            }
            else
            {
                await DisplayAlert("Oops", "This address was not confirm by USPS. Try a different address", "OK");
            }
            
        }

        async void addressSelected(System.Object sender, SelectedItemChangedEventArgs e)
        {
            addressToValidate = addr.addressSelected(addressList, AddressEntry, addressFrame, newUserUnitNumber, gridAddressView, newUserCity, newUserState, newUserZipcode);
            addressFrame.Margin = new Thickness(0, -75, 0, 0);
            // check if given address is with in zones
            var zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if (zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;

                if (addressToValidate != null)
                {
                    // ask for unit
                    addressToValidate.Unit = newUserUnitNumber.Text;

                    var client = new AddressValidation();
                    var location = await client.ValidateAddress(addressToValidate.Street, addressToValidate.Unit, addressToValidate.City, addressToValidate.State, addressToValidate.ZipCode);

                    if (location != null)
                    {
                        var isAddressInZones = await client.isLocationInZones(zone, location.Latitude.ToString(), location.Longitude.ToString());
                        if (isAddressInZones != "INSIDE CURRENT ZONE" && isAddressInZones != "")
                        {
                            // We can continue with payments since we can deliver to this location
                            await DisplayAlert("Great!", "We are able to deliver to your location! Proceed to payments", "OK");
                            client.SetPinOnMap(map, location, addressToValidate.Address);
                            purchase.setPurchaseAddress(addressToValidate.Street);
                            purchase.setPurchaseUnit(addressToValidate.Unit == null?"": addressToValidate.Unit);
                            purchase.setPurchaseCity(addressToValidate.City);
                            purchase.setPurchaseState(addressToValidate.State);
                            purchase.setPurchaseZipcode(addressToValidate.ZipCode);
                            purchase.setPurchaseLatitude(location.Latitude.ToString());
                            purchase.setPurchaseLongitude(location.Longitude.ToString());
                        }
                        else if (isAddressInZones != "INSIDE DIFFERENT ZONE" && isAddressInZones != "")
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
        }

        void ShowLogInModal(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94), true);
        }

        void ShowSignUpModal(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new SignUpPage(94), true);
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
            //Application.Current.MainPage.Navigation.PopModalAsync();
            //Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(), true);
        }

        void NagivateToSignUpFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToSignUp(sender, e);
            //Application.Current.MainPage.Navigation.PopModalAsync();
            //Application.Current.MainPage.Navigation.PushModalAsync(new SignUpPage(), true);
        }

        void NagigateToMainFromCheckout(System.Object sender, System.EventArgs e)
        {
            NavigateToMain(sender, e);
        }

        void ShowMenuFromCheckout(System.Object sender, System.EventArgs e)
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


        async void SignInDirectUser(System.Object sender, System.EventArgs e)
        {
            var logInClient = new PrincipalPage();
            if(logInClient.ValidateDirectSignInCredentials(userEmailAddress, userPassword))
            {
                var isEmailValid = await DeliveryDetailsPage.ValidateExistingAccountFromEmail(userEmailAddress.Text);
                if (isEmailValid != null)
                {
                    if (isEmailValid.result.Count != 0)
                    {
                        var role = isEmailValid.result[0].role;
                        if (role == "CUSTOMER")
                        {
                            var status = await SignInDirectUser(logInButton, userEmailAddress, userPassword);
                            if (status)
                            {
                                await DisplayAlert("Great!", "You have signed in to your account", "OK");
                            }
                            else
                            {
                                await DisplayAlert("Oops!", "We were not able to sign you in", "OK");
                            }
                        }
                        else if (role == "GUEST")
                        {
                            // we don't sign up but get user id
                            user.setUserID(isEmailValid.result[0].customer_uid);
                            user.setUserFromProfile(isEmailValid);
                            var updateClient = new SignUp();
                            var content = updateClient.UpdateDirectUser(user, userPassword.Text);
                            var signUpStatus = await SignUp.SignUpNewUser(content);

                            if (signUpStatus)
                            {
                                await DisplayAlert("Great!", "We have created your account! Congratulations", "OK");
                                user.setUserType("CUSTOMER");
                                Application.Current.MainPage = new SelectionPage();
                            }
                            else
                            {
                                await DisplayAlert("Oops", "We were not able to sign you up. Try again.", "OK");
                            }
                        }
                    }
                }
            }
        }

        async Task<bool> SignInDirectUser(Button logInButton, Entry userEmailAddress, Entry userPassword)
        {
            var status = false;
            var client = new SignIn();
            var result = await client.SignInDirectUser(logInButton, userEmailAddress, userPassword);
            if (result != null)
            {
                Debug.WriteLine("You have an acccount");

                user.setUserID(result.getUserID());
                user.setUserSessionTime(result.getUserSessionTime());
                user.setUserPlatform(result.getUserPlatform());
                user.setUserType(result.getUserType());
                user.setUserEmail(result.getUserEmail());
                user.setUserFirstName(result.getUserFirstName());
                user.setUserLastName(result.getUserLastName());
                user.setUserPhoneNumber(result.getUserPhoneNumber());
                user.setUserAddress(result.getUserAddress());
                user.setUserUnit(result.getUserUnit());
                user.setUserCity(result.getUserCity());
                user.setUserState(result.getUserState());
                user.setUserZipcode(result.getUserZipcode());
                user.setUserLatitude(result.getUserLatitude());
                user.setUserLongitude(result.getUserLongitude());
                
                status = true;
            }
            return status;
        }

        void SignInWithFacebook(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            var authenticator = client.GetFacebookAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            authenticator.Completed += SignInFacebookAuth;
            authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        void SignInWithGoogle(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            var authenticator = client.GetGoogleAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            AuthenticationState.Authenticator = authenticator;
            authenticator.Completed += SignInGoogleAuth;
            authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        private async void SignInFacebookAuth(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var clientLogIn = new SignIn();
                    var facebookUser = clientLogIn.GetFacebookUser(e.Account.Properties["access_token"]);
                    var result = await clientLogIn.VerifyUserCredentials(facebookUser.email, facebookUser.id,"FACEBOOK");
                    if(result != null)
                    {
                        user.setUserID(result.getUserID());
                        user.setUserSessionTime(result.getUserSessionTime());
                        user.setUserPlatform(result.getUserPlatform());
                        user.setUserType(result.getUserType());
                        user.setUserEmail(result.getUserEmail());
                        user.setUserFirstName(result.getUserFirstName());
                        user.setUserLastName(result.getUserLastName());
                        user.setUserPhoneNumber(result.getUserPhoneNumber());
                        user.setUserAddress(result.getUserAddress());
                        user.setUserUnit(result.getUserUnit());
                        user.setUserCity(result.getUserCity());
                        user.setUserState(result.getUserState());
                        user.setUserZipcode(result.getUserZipcode());
                        user.setUserLatitude(result.getUserLatitude());
                        user.setUserLongitude(result.getUserLongitude());

                        if(user.getUserType() == "GUEST")
                        {
                            var updateClient = new SignUp();
                            var content = updateClient.UpdateSocialUser(user, e.Account.Properties["access_token"], "", facebookUser.id, "FACEBOOK");
                            var signUpStatus = await SignUp.SignUpNewUser(content);

                            if (signUpStatus)
                            {
                                user.setUserType("CUSTOMER");
                            }
                            else
                            {
                                await DisplayAlert("Oops", "We were not able to sign you up. Try again.", "OK");
                            }
                        }
                        await DisplayAlert("Great!", "You have signed in to your account", "OK");
                        Application.Current.MainPage = new CheckoutPage();
                    }
                    else
                    {
                        await DisplayAlert("Oops","We were not able to sign you in. Please sign in witht the correct social medial sign in button","OK");
                    }
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
        }

        private async void SignInGoogleAuth(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var clientLogIn = new SignIn();
                    var googleUser = await clientLogIn.GetGoogleUser(e);
                    var result = await clientLogIn.VerifyUserCredentials(googleUser.email, googleUser.id, "GOOGLE");
                    if (result != null)
                    {
                        user.setUserID(result.getUserID());
                        user.setUserSessionTime(result.getUserSessionTime());
                        user.setUserPlatform(result.getUserPlatform());
                        user.setUserType(result.getUserType());
                        user.setUserEmail(result.getUserEmail());
                        user.setUserFirstName(result.getUserFirstName());
                        user.setUserLastName(result.getUserLastName());
                        user.setUserPhoneNumber(result.getUserPhoneNumber());
                        user.setUserAddress(result.getUserAddress());
                        user.setUserUnit(result.getUserUnit());
                        user.setUserCity(result.getUserCity());
                        user.setUserState(result.getUserState());
                        user.setUserZipcode(result.getUserZipcode());
                        user.setUserLatitude(result.getUserLatitude());
                        user.setUserLongitude(result.getUserLongitude());

                        if (user.getUserType() == "GUEST")
                        {
                            var updateClient = new SignUp();
                            var content = updateClient.UpdateSocialUser(user, e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], googleUser.id, "GOOGLE");
                            var signUpStatus = await SignUp.SignUpNewUser(content);

                            if (signUpStatus)
                            {
                                user.setUserType("CUSTOMER");
                            }
                            else
                            {
                                await DisplayAlert("Oops", "We were not able to sign you up. Try again.", "OK");
                            }
                        }
                        await DisplayAlert("Great!", "You have signed in to your account", "OK");
                        Application.Current.MainPage = new CheckoutPage();

                    }
                    else
                    {
                        await DisplayAlert("Oops", "We were not able to sign you in. Please sign in witht the correct social medial sign in button", "OK");
                    }
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
        }

        async void SignUpDirectUser(System.Object sender, System.EventArgs e)
        {
            var client = new PrincipalPage();

            if (client.ValidateSignUpInfo(newUserFirstName, newUserLastName, newUserEmail1, newUserEmail2, newUserPassword1, newUserPassword2))
            {
                if (client.ValidateEmail(newUserEmail1, newUserEmail2))
                {
                    if (client.ValidatePassword(newUserPassword1, newUserPassword2))
                    {
                        // user is ready to be sign in.
                        var clientSignUp = new SignUp();
                        var content = clientSignUp.SetDirectUser(user, newUserPassword1.Text);
                        var signUpStatus = await SignUp.SignUpNewUser(content);

                        if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                        {
                            user.setUserID(signUpStatus);
                            user.setUserPlatform("DIRECT");
                            user.setUserType("CUSTOMER");
                            clientSignUp.SendUserToCheckoutPage();
                        }
                        else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                        {
                            await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "Please check that your password is the same in both entries", "OK");
                        return;
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "Please check that your email is the same in both entries", "OK");
                    return;
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                return;
            }
        }

        void UpdateAddressForCustomer(System.Object sender, System.EventArgs e)
        {
            if(guestAddressInfoView.HeightRequest != 0)
            {
                guestAddressInfoView.HeightRequest = 0;
                guestDeliveryAddressLabel.IsVisible = false;
                guestMap.IsVisible = false;
            }
            else
            {
                guestAddressInfoView.HeightRequest = 140;
            }
        }

        void SignUpWithFacebook(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            var authenticator = client.GetFacebookAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            authenticator.Completed += FacebookAuthetication;
            authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        void SignUpWithGoogle(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            var authenticator = client.GetGoogleAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            AuthenticationState.Authenticator = authenticator;
            authenticator.Completed += GoogleAuthetication;
            authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        private async void FacebookAuthetication(object sender, Xamarin.Auth.AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var clientLogIn = new SignIn();
                    var clientSignUp = new SignUp();

                    var facebookUser = clientLogIn.GetFacebookUser(e.Account.Properties["access_token"]);
                    var content = clientSignUp.SetDirectUser(user, e.Account.Properties["access_token"], "", facebookUser.id, facebookUser.email, "FACEBOOK");
                    var signUpStatus = await SignUp.SignUpNewUser(content);

                    if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                    {
                        user.setUserID(signUpStatus);
                        user.setUserPlatform("FACEBOOK");
                        user.setUserType("CUSTOMER");
                        clientSignUp.SendUserToCheckoutPage();
                    }
                    else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                    {
                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                    }
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
        }

        private async void GoogleAuthetication(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var clientLogIn = new SignIn();
                    var clientSignUp = new SignUp();

                    var googleUser = await clientLogIn.GetGoogleUser(e);
                    var content = clientSignUp.SetDirectUser(user, e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], googleUser.id, googleUser.email, "GOOGLE");
                    var signUpStatus = await SignUp.SignUpNewUser(content);

                    if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                    {
                        user.setUserID(signUpStatus);
                        user.setUserPlatform("GOOGLE");
                        user.setUserType("CUSTOMER");
                        clientSignUp.SendUserToCheckoutPage();
                    }
                    else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                    {
                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                    }
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
        }

        private async void Authenticator_Error(object sender, Xamarin.Auth.AuthenticatorErrorEventArgs e)
        {
            await DisplayAlert("An error occur when authenticating", "Please try again", "OK");
        }
    }
}
