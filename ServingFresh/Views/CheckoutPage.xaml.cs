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
using static ServingFresh.App;
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
        public double ambassadorDiscount = 0;
        public string couponsUIDs = "";
        public static Purchase purchase = new Purchase(user);
        public ObservableCollection<ItemObject> cartItems = new ObservableCollection<ItemObject>();
        public ObservableCollection<CouponItem> couponsList = new ObservableCollection<CouponItem>();
        public ObservableCollection<CouponItem> availableCoupons = new ObservableCollection<CouponItem>();
        public CouponItem appliedCoupon = null;

        public ObservableCollection<CouponItem> TempCouponsList = new ObservableCollection<CouponItem>();
        public double subtotal;
        public double discount;
        public double delivery_fee_db = 0;
        public double delivery_fee = 5.0;
        public double service_fee = 1.5;
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

                        //addressToValidate = validatedAddress;
                        addr.addressSelectedFillEntries(validatedAddress, AddressEntry, newUserUnitNumber, newUserCity, newUserState, newUserZipcode);
                        var client = new AddressValidation();
                        client.SetPinOnMap(map, user.getUserLatitude(), user.getUserLongitude(), validatedAddress.Street);
                    }
                }
                else
                {
                    purchase.setPurchaseAddress(user.getUserAddress());
                    purchase.setPurchaseCity(user.getUserCity());
                    purchase.setPurchaseState(user.getUserState());
                    purchase.setPurchaseZipcode(user.getUserZipcode());
                    cardHolderName.Text = user.getUserFirstName() + " " + user.getUserLastName();
                    DeliveryAddress1.Text = purchase.getPurchaseAddress() + ",";
                    DeliveryAddress2.Text = purchase.getPurchaseCity() + ", " + purchase.getPurchaseState() + ", " + purchase.getPurchaseZipcode();
                    guestAddressInfoView.HeightRequest = 0;
                    guestPaymentsView.HeightRequest = 0;
                    addressFrame.Margin = new Thickness(0, 0, 0, 0);
                    guestDeliveryAddressLabel.IsVisible = false;
                    guestMap.IsVisible = false;
                }

                cartItems.Clear();
                var showGiftCardHeader = false;
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

                    if (order[key].item_name.Contains("GiftCard"))
                    {
                        showGiftCardHeader = true;
                    }
                    orderCopy.Add(key, order[key]);
                }

                if (showGiftCardHeader)
                {
                    giftCardHeader.IsVisible = true;
                    confirmationEmail.Text = user.getUserEmail();
                }
                else
                {
                    giftCardHeader.IsVisible = false;
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
            try
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
                    if (data.result.Count != 0)
                    {
                        Debug.WriteLine("USER DELIVERY INSTRUCTIONS: " + data.result[0].delivery_instructions);
                        if (data.result[0].delivery_instructions != "")
                        {
                            result = data.result[0].delivery_instructions;
                        }
                    }
                }
                return result;
            }catch(Exception errorGetDeliveryInstruction)
            {
                var client = new Diagnostic();
                client.parseException(errorGetDeliveryInstruction.ToString(), user);
                return "";
            }
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
                    Debug.WriteLine("URL: " + "https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/get_Fee_Tax/" + zone + "," + day);
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

                        Debug.WriteLine("Delivery Fee: " + delivery_fee);
                        Debug.WriteLine("Service Fee: " + service_fee);

                        ServiceFee.Text = "$" + service_fee.ToString("N2");
                        DeliveryFee.Text = "$" + delivery_fee.ToString("N2");

                    }

                    //Debug.WriteLine("Delivery Fee: " + delivery_fee);
                    //Debug.WriteLine("Service Fee: " + service_fee);

                    //ServiceFee.Text = "$" + service_fee.ToString("N2");
                    //DeliveryFee.Text = "$" + delivery_fee.ToString("N2");
                    // GetAvailiableCoupons();
                    SetTips("$2");
                    //updateTotals(0, 0);
                }
            }
            catch (Exception errorGetFees)
            {
                var client = new Diagnostic();
                client.parseException(errorGetFees.ToString(), user);
            }
        }

        public async void GetAvailiableCoupons(Models.User user)
        {
            try
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
                        if (c.coupon_id != "SFReferral" && c.coupon_id != "SFGiftCard")
                        {
                            // IF THRESHOLD IS NULL SET IT TO ZERO, OTHERWISE INITIAL VALUE STAYS THE SAME
                            if (c.threshold == null) { c.threshold = 0.0; }
                            double discount = 0;
                            //double newTotal = 0;

                            var coupon = new CouponItem();
                            //Debug.WriteLine("COUPON IDS: " + c.coupon_uid);
                            coupon.couponId = c.coupon_uid;
                            // INITIALLY, THE IMAGE OF EVERY COUPON IS GRAY. (PLATFORM DEPENDENT)
                            //if (Device.RuntimePlatform == Device.Android)
                            //{
                            //    coupon.image = "CouponIconGray.png";
                            //}
                            //else
                            //{
                            //    coupon.image = "nonEligibleCoupon.png";
                            //}

                            coupon.image = "nonEligibleCoupon.png";

                            // SET TITLE LABEL OF COUPON
                            if (c.coupon_title != null)
                            {
                                coupon.title = (string)c.coupon_title;
                            }
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
                            couponsList.Add(coupon);
                        }
                    }



                    //couponsList.Clear();
                    var activeCoupons = CouponItem.GetActiveCoupons(couponsList);
                    var nonactiveCoupons = CouponItem.GetNonActiveCoupons(couponsList, initialSubTotal);
                    CouponItem.SortActiveCoupons(activeCoupons);
                    CouponItem.SortNonActiveCoupons(nonactiveCoupons);
                    availableCoupons = CouponItem.MergeActiveNonActiveCouponLists(activeCoupons, nonactiveCoupons);
                    if (availableCoupons.Count > 0)
                    {
                        if (availableCoupons[0].status == "ACTIVE")
                        {
                            availableCoupons[0].image = "appliedCoupon.png";
                            availableCoupons[0].isCouponEligible = "Applied";
                            availableCoupons[0].textColor = Color.White;
                            Debug.WriteLine("Discount: " + availableCoupons[0].discount);
                            Debug.WriteLine("Shipping: " + availableCoupons[0].shipping);
                            updateTotals(availableCoupons[0].discount, availableCoupons[0].shipping);
                            appliedCoupon = availableCoupons[0];
                        }
                        else
                        {
                            updateTotals(0, 0);
                        }
                    }
                    coupon_list.ItemsSource = availableCoupons;

                }
            }catch(Exception errorGetAvailableCoupons)
            {
                var client = new Diagnostic();
                client.parseException(errorGetAvailableCoupons.ToString(), user);
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
                        Debug.WriteLine("Discount: " + source[0].discount);
                        Debug.WriteLine("Shipping: " + source[0].shipping);
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
            subtotal = subtotal - ambassadorDiscount;
            this.discount = discount;
            Discount.Text = "$" + this.discount.ToString("N2");
            
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

            if(total == 0)
            {
                if (user.getUserType() == "CUSTOMER")
                {
                    //HIDE 1ST AND 2ND CHECKOUT BUTTONS AND ONLY SHOW ONE BUTTON.
                    customerPaymentsView.IsVisible = false;
                    freeCheckoutView.IsVisible = true;
                }
            }
            else
            {
                if (user.getUserType() == "CUSTOMER")
                {
                    //UNHIDE 1ST AND 2ND CHECKOUT BUTTONS AND ONLY SHOW ONE BUTTON.
                    customerPaymentsView.IsVisible = true; ;
                    freeCheckoutView.IsVisible = false; ;
                }

            }
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
            try
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
                purchase.setPurchaseDeliveryInstructions(deliveryInstructions.Text == null || deliveryInstructions.Text == "" ? "" : deliveryInstructions.Text);

            }catch(Exception errorFinalizePurchase)
            {
                var client = new Diagnostic();
                client.parseException(errorFinalizePurchase.ToString(), user);
            }
        }



        // ====================================================================
        public void increase_qty(object sender, EventArgs e)
        {
            //Label l = (Label)sender;
            //TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            var button = (ImageButton)sender;
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
                if (appliedCoupon != null)
                {
                    updateTotals(appliedCoupon.discount, appliedCoupon.shipping);
                }
                else
                {
                    updateTotals(0, 0);
                }

                GetNewDefaltCoupon();
            }
        }

        public void decrease_qty(object sender, EventArgs e)
        {
            //Label l = (Label)sender;
            //TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            var button = (ImageButton)sender;
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
                if (appliedCoupon != null)
                {
                    updateTotals(appliedCoupon.discount, appliedCoupon.shipping);
                }
                else
                {
                    updateTotals(0, 0);
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
                if (c.coupon_id != "SFReferral" && c.coupon_id != "SFGiftCard")
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
                    //if (Device.RuntimePlatform == Device.Android)
                    //{
                    //    coupon.image = "CouponIconGray.png";
                    //}
                    //else
                    //{
                    //    coupon.image = "nonEligibleCoupon.png";
                    //}

                    coupon.image = "nonEligibleCoupon.png";

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
                }

                var activeCoupons = CouponItem.GetActiveCoupons(couponsList);
                var nonactiveCoupons = CouponItem.GetNonActiveCoupons(couponsList, initialSubTotal);
                CouponItem.SortActiveCoupons(activeCoupons);
                CouponItem.SortNonActiveCoupons(nonactiveCoupons);
                availableCoupons = ApplyBestAvailableCoupon(CouponItem.MergeActiveNonActiveCouponLists(activeCoupons, nonactiveCoupons));
                coupon_list.ItemsSource = availableCoupons;
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

                    if(total == 0)
                    {
                        freeCheckoutView.IsVisible = true;
                    }
                    else
                    {
                        guestCheckoutView.IsVisible = true;
                    }

                }
                else
                {
                    button.BorderColor = Color.FromHex("#FF8500");
                    guestCheckoutView.IsVisible = false;
                    guestRequiredInfoView.IsVisible = false;
                    freeCheckoutView.IsVisible = false;
                }
            }
            else
            {
                if (messageList != null)
                {
                    if (messageList.ContainsKey("701-000004"))
                    {
                        await DisplayAlert(messageList["701-000004"].title, messageList["701-000004"].message, messageList["701-000004"].responses);
                    }
                    else
                    {
                        await DisplayAlert("Please verify your address!", "It looks like we were not able to validate your address. Make sure that your delivery address shows in the map", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Please verify your address!", "It looks like we were not able to validate your address. Make sure that your delivery address shows in the map", "OK");
                }
            }
        }

        async void GuestCheckoutWithStripe(System.Object sender, System.EventArgs e)
        {
            if (areTermsAccepted.IsChecked)
            {
                var client1 = new SignUp();

                if (client1.GuestCheckAllRequiredEntries(firstName, lastName, emailAddress, phoneNumber))
                {

                    purchase.setPurchaseFirstName(firstName.Text);
                    purchase.setPurchaseLastName(lastName.Text);
                    purchase.setPurchaseEmail(emailAddress.Text);
                    purchase.setPurchasePhoneNumber(phoneNumber.Text);
                    guestCardHolderName.Text = firstName.Text + " " + lastName.Text;
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
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000005"))
                        {
                            await DisplayAlert(messageList["701-000005"].title, messageList["701-000005"].message, messageList["701-000005"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please accept the terms and conditions", "OK");
            }
        }

        async void GuestCompletePaymentWithPayPal(System.Object sender, System.EventArgs e)
        {
            if (areTermsAccepted.IsChecked)
            {
                var client1 = new SignUp();

                if (client1.GuestCheckAllRequiredEntries(firstName, lastName, emailAddress, phoneNumber))
                {
                    purchase.setPurchaseFirstName(firstName.Text);
                    purchase.setPurchaseLastName(lastName.Text);
                    purchase.setPurchaseEmail(emailAddress.Text);
                    purchase.setPurchasePhoneNumber(phoneNumber.Text);
                    var coupond = purchase.getPurchaseCoupoID();
                    purchase.setPurchaseCoupoID(coupond + couponsUIDs);
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
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000006"))
                        {
                            await DisplayAlert(messageList["701-000006"].title, messageList["701-000006"].message, messageList["701-000006"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please accept the terms and conditions", "OK");
            }
        }

        async void GuestCompletePaymentWithStripe(System.Object sender, System.EventArgs e)
        {
            try
            {
                var client1 = new SignUp();
                if (client1.GuestCheckAllStripeRequiredEntries(guestCardHolderName, guestCardHolderNumber, guestCardCVV, guestCardExpMonth, guestCardExpYear, guestCardZipcode))
                {
                    var button = (Button)sender;

                    if (button.BackgroundColor == Color.FromHex("#FF8500"))
                    {
                        button.BackgroundColor = Color.FromHex("#2B6D74");
                        purchase.setPurchasePaymentType("STRIPE");
                        UserDialogs.Instance.ShowLoading("Your payment is processing...");
                        var paymentClient = new Payments();
                        string mode = await paymentClient.getMode(purchase.getPurchaseDeliveryInstructions(), "STRIPE");
                        if (mode == "LIVE" || mode == "TEST")
                        {
                            paymentClient = new Payments(mode);
                            var client = new SignIn();
                            var isEmailUnused = await client.ValidateExistingAccountFromEmail(purchase.getPurchaseEmail());
                            if (isEmailUnused == null)
                            {
                                var userID = await SignUp.SignUpNewUser(SignUp.GetUserFrom(purchase));
                                if (userID != "")
                                {
                                    user.setUserID(userID);
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
                                        var coupond = purchase.getPurchaseCoupoID();
                                        purchase.setPurchaseCoupoID(coupond + couponsUIDs);
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

                                        if (messageList != null)
                                        {
                                            if (messageList.ContainsKey("701-000007"))
                                            {
                                                await DisplayAlert(messageList["701-000007"].title, messageList["701-000007"].message, messageList["701-000007"].responses);
                                            }
                                            else
                                            {
                                                await DisplayAlert("Oop", "It seems that your card is invalid. Try again", "OK");
                                            }
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oop", "It seems that your card is invalid. Try again", "OK");
                                        }
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

                                        if (messageList != null)
                                        {
                                            if (messageList.ContainsKey("701-000008"))
                                            {
                                                await DisplayAlert(messageList["701-000008"].title, messageList["701-000008"].message, messageList["701-000008"].responses);
                                            }
                                            else
                                            {
                                                await DisplayAlert("Great!", "It looks like you already have an account. Please log in to find out if you have any additional discounts", "OK");
                                            }
                                        }
                                        else
                                        {
                                            await DisplayAlert("Great!", "It looks like you already have an account. Please log in to find out if you have any additional discounts", "OK");
                                        }

                                        await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94, "1"), true);
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
                                            var coupond = purchase.getPurchaseCoupoID();
                                            purchase.setPurchaseCoupoID(coupond + couponsUIDs);
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
                                            if (messageList != null)
                                            {
                                                if (messageList.ContainsKey("701-000009"))
                                                {
                                                    await DisplayAlert(messageList["701-000009"].title, messageList["701-000009"].message, messageList["701-000009"].responses);
                                                }
                                                else
                                                {
                                                    await DisplayAlert("Oops", "It seems that your card is invalid. Try again", "OK");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("Oops", "It seems that your card is invalid. Try again", "OK");
                                            }
                                        }
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
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000010"))
                        {
                            await DisplayAlert(messageList["701-000010"].title, messageList["701-000010"].message, messageList["701-000010"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                    }
                }
            }catch(Exception errorGuestCompletePaymentWithStripe)
            {
                var client = new Diagnostic();
                client.parseException(errorGuestCompletePaymentWithStripe.ToString(), user);
            }
        }

        async void CheckoutViaStripe(System.Object sender, System.EventArgs e)
        {
            if (customerAreTermAccepted.IsChecked)
            {
                var button = (Button)sender;

                if (button.BackgroundColor == Color.FromHex("#FF8500"))
                {
                    button.BackgroundColor = Color.FromHex("#2B6D74");
                    
                    customerStripeInformationView.IsVisible = true;

                    var y = scrollView.ScrollY + 120;
                    y = y + 60;
                    await scrollView.ScrollToAsync(0, y, true);
                    button.BorderColor = Color.FromHex("#2F787F");
                    guestStripeView.IsVisible = true;
  
                }
                else
                {
                    button.BackgroundColor = Color.FromHex("#FF8500");
                    //customerStripeInformationView.HeightRequest = 0;
                    customerStripeInformationView.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please accept the terms and conditions", "OK");
            }
        }

        async void CompletePaymentWithStripe(System.Object sender, System.EventArgs e)
        {
            try
            {
                var client1 = new SignUp();

                if (client1.GuestCheckAllStripeRequiredEntries(cardHolderName, cardHolderNumber, cardCVV, cardExpMonth, cardExpYear, cardZip))
                {
                    var button = (Button)sender;

                    if (button.BackgroundColor == Color.FromHex("#FF8500"))
                    {
                        UserDialogs.Instance.ShowLoading("Your payment is processing...");
                        button.BackgroundColor = Color.FromHex("#2B6D74");
                        FinalizePurchase(purchase, selectedDeliveryDate);
                        purchase.setPurchasePaymentType("STRIPE");
                        var paymentClient = new Payments();
                        string mode = await paymentClient.getMode(purchase.getPurchaseDeliveryInstructions(), "STRIPE");
                        if (mode == "LIVE" || mode == "TEST")
                        {
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
                                var coupond = purchase.getPurchaseCoupoID();
                                purchase.setPurchaseCoupoID(coupond + couponsUIDs);
                                purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                                purchase.setPurchaseChargeID(paymentClient.getTransactionID());
                                _ = paymentClient.SendPurchaseToDatabase(purchase);
                                order.Clear();
                                await WriteFavorites(GetFavoritesList(), purchase.getPurchaseCustomerUID());
                                UserDialogs.Instance.HideLoading();
                                Application.Current.MainPage = new HistoryPage();
                            }
                            else
                            {
                                if (messageList != null)
                                {
                                    if (messageList.ContainsKey("701-000011"))
                                    {
                                        UserDialogs.Instance.HideLoading();
                                        await DisplayAlert(messageList["701-000011"].title, messageList["701-000011"].message, messageList["701-000011"].responses);
                                    }
                                    else
                                    {
                                        UserDialogs.Instance.HideLoading();
                                        await DisplayAlert("Oops", "Payment was not sucessful", "OK");
                                    }
                                }
                                else
                                {
                                    UserDialogs.Instance.HideLoading();
                                    await DisplayAlert("Oops", "Payment was not sucessful", "OK");
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
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000006"))
                        {
                            await DisplayAlert(messageList["701-000006"].title, messageList["701-000006"].message, messageList["701-000006"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                    }
                }
            }catch(Exception errorCompletePaymentWithStripe)
            {
                var client = new Diagnostic();
                client.parseException(errorCompletePaymentWithStripe.ToString(), user);
            }

        }

        async void CheckoutViaPayPal(System.Object sender, System.EventArgs e)
        {
            //TERMS AND CONDITIONS: http://localhost:3000/terms-and-conditions
            if (customerAreTermAccepted.IsChecked)
            {
                FinalizePurchase(purchase, selectedDeliveryDate);
                purchase.setPurchasePaymentType("PAYPAL");
                var coupond = purchase.getPurchaseCoupoID();
                purchase.setPurchaseCoupoID(coupond + couponsUIDs);
                await Application.Current.MainPage.Navigation.PushModalAsync(new PayPalPage(), true);
            }
            else
            {
                await DisplayAlert("Oops", "Please accept the terms and conditions", "OK");
            }
        }

        public static async Task<bool> WriteFavorites(List<string> favorites, string userID)
        {
            try
            {
                var taskResponse = false;
                if (userID != null && userID != "")
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
            }catch(Exception errorWriteFavorites)
            {
                var client = new Diagnostic();
                client.parseException(errorWriteFavorites.ToString(), user);
                return false;
            }
        }

        Models.Address addr = new Models.Address();

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
            addressToValidate.isValidated = false;
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
                            addressToValidate.isValidated = true;
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
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000012"))
                                {
                                    await DisplayAlert(messageList["701-000012"].title, messageList["701-000012"].message, messageList["701-000012"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "You address is outside our delivery areas", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Oops", "You address is outside our delivery areas", "OK");
                            }
                            
                            return;
                        }
                    }
                    else
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000013"))
                            {
                                await DisplayAlert(messageList["701-000013"].title, messageList["701-000013"].message, messageList["701-000013"].responses);
                            }
                            else
                            {
                                await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                        }
                        
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
                                        addressToValidate.isValidated = true;
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
                                        if (messageList != null)
                                        {
                                            if (messageList.ContainsKey("701-000014"))
                                            {
                                                await DisplayAlert(messageList["701-000014"].title, messageList["701-000014"].message, messageList["701-000014"].responses);
                                            }
                                            else
                                            {
                                                await DisplayAlert("Oops", "You address is outside our delivery areas", "OK");
                                            }
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oops", "You address is outside our delivery areas", "OK");
                                        }
                                        
                                        return;
                                    }
                                }
                                else
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000015"))
                                        {
                                            await DisplayAlert(messageList["701-000015"].title, messageList["701-000015"].message, messageList["701-000015"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                                    }
                                    
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
                if (messageList != null)
                {
                    if (messageList.ContainsKey("701-000016"))
                    {
                        await DisplayAlert(messageList["701-000016"].title, messageList["701-000016"].message, messageList["701-000016"].responses);
                    }
                    else
                    {
                        await DisplayAlert("Oops", "This address was not confirm by USPS. Try a different address", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "This address was not confirm by USPS. Try a different address", "OK");
                }
                
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
                            
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000017"))
                                {
                                    await DisplayAlert(messageList["701-000017"].title, messageList["701-000017"].message, messageList["701-000017"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Great!", "We are able to deliver to your location! Proceed to payments", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Great!", "We are able to deliver to your location! Proceed to payments", "OK");
                            }
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
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000018"))
                                {
                                    await DisplayAlert(messageList["701-000018"].title, messageList["701-000018"].message, messageList["701-000018"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Great!", "We are able to deliver to your location! However, your new entered address is outside the initial given address.", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Great!", "We are able to deliver to your location! However, your new entered address is outside the initial given address.", "OK");
                            }
                            
                        }
                        else if (isAddressInZones != "OUTSIDE ZONE RANGE" && isAddressInZones != "")
                        {
                            // User is outside zones
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000019"))
                                {
                                    await DisplayAlert(messageList["701-000019"].title, messageList["701-000019"].message, messageList["701-000019"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Sorry", "Unfortunately, we can't deliver to this location.", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Sorry", "Unfortunately, we can't deliver to this location.", "OK");
                            }
                            
                        }
                    }
                    else
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000020"))
                            {
                                await DisplayAlert(messageList["701-000020"].title, messageList["701-000020"].message, messageList["701-000020"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Is your address missing a unit number?", "Please check your address and add unit number if missing", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Is your address missing a unit number?", "Please check your address and add unit number if missing", "OK");
                        }
                    }
                }
            }
        }

        void ShowLogInModal(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94, "1"), true);
        }

        void ShowSignUpModal(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new SignUpPage(94), true);
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

        }

        async void UpdateAddressForCustomer(System.Object sender, System.EventArgs e)
        {
            if(guestAddressInfoView.HeightRequest != 0)
            {
                var label = (Label)sender;
                var reconizer = (TapGestureRecognizer)label.GestureRecognizers[0];

                label.Text = "Change delivery address";
                guestAddressInfoView.HeightRequest = 0;
                guestDeliveryAddressLabel.IsVisible = false;
                guestMap.IsVisible = false;

                if (addressToValidate != null)
                {
                    var client = new SignIn();
                    var profile = await client.ValidateExistingAccountFromEmail(user.getUserEmail());

                    if (profile != null)
                    {
                        if (addressToValidate.isValidated)
                        {
                            if (profile.result.Count != 0)
                            {
                                profile.result[0].customer_address = addressToValidate.Street;
                                profile.result[0].customer_unit = addressToValidate.Unit;
                                profile.result[0].customer_city = addressToValidate.City;
                                profile.result[0].customer_state = addressToValidate.State;
                                profile.result[0].customer_zip = addressToValidate.ZipCode;

                                var updateStatus = await client.UpdateProfile(profile);
                                if (updateStatus)
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000021"))
                                        {
                                            await DisplayAlert(messageList["701-000021"].title, messageList["701-000021"].message, messageList["701-000021"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Great!", "We have updated your address successfully", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Great!", "We have updated your address successfully", "OK");
                                    }
                                    

                                    user.setUserAddress(addressToValidate.Street);
                                    user.setUserUnit(addressToValidate.Unit == null ? "" : addressToValidate.Unit);
                                    user.setUserCity(addressToValidate.City);
                                    user.setUserState(addressToValidate.State);
                                    user.setUserZipcode(addressToValidate.ZipCode);

                                    purchase = new Purchase(user);
                                }
                                else
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000022"))
                                        {
                                            await DisplayAlert(messageList["701-000022"].title, messageList["701-000022"].message, messageList["701-000022"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oops", "We were not able to update your address successfully", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "We were not able to update your address successfully", "OK");
                                    }
                                   
                                }
                                // make the update

                            }
                        }
                        else
                        {
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000023"))
                                {
                                    await DisplayAlert(messageList["701-000023"].title, messageList["701-000023"].message, messageList["701-000023"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "The address you entered was not validated. Please try again", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Oops", "The address you entered was not validated. Please try again", "OK");
                            }
                            
                        }
                    }
                    else
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000024"))
                            {
                                await DisplayAlert(messageList["701-000024"].title, messageList["701-000024"].message, messageList["701-000024"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "We faced some issues updating your address. Please try again", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "We faced some issues updating your address. Please try again", "OK");
                        }
                        
                    }
                }

            }
            else
            {
                guestAddressInfoView.HeightRequest = 140;
                var label = (Label)sender;
                label.Text = "Save delivery address";

                
            }

           
        }

        async void VerifyAmbassadorCode(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(ambassadorCode.Text))
                {
                    string ambassadorStatus = "";
                    if (user.getUserType() == "CUSTOMER")
                    {
                        var client = new Ambassador();
                        Debug.WriteLine("INPUTS -> CODE: {0}, INFO: {1}. IS GUEST: {2}", ambassadorCode.Text, user.getUserEmail(), "False");
                        ambassadorStatus = await client.ValidateAmbassadorCode(ambassadorCode.Text, user.getUserEmail(), "False");
                        Debug.WriteLine("OUTPUT FROM VALIDATING AMBASSADOR CODE: " + ambassadorStatus);
                    }
                    else if (user.getUserType() == "GUEST")
                    {
                        var client = new Ambassador();
                        var info = purchase.getPurchaseAddress() + ", " + purchase.getPurchaseCity() + ", " + purchase.getPurchaseState() + " " + purchase.getPurchaseZipcode();
                        Debug.WriteLine("INPUTS -> CODE: {0}, INFO: {1}. IS GUEST: {2}", ambassadorCode.Text, info, "True");

                        ambassadorStatus = await client.ValidateAmbassadorCode(ambassadorCode.Text, info, "True");
                        Debug.WriteLine("OUTPUT FROM VALIDATING AMBASSADOR CODE: " + ambassadorStatus);
                    }

                    if (ambassadorStatus != "")
                    {
                        if (ambassadorStatus.Contains("200"))
                        {
                            var ambassadorResponse = JsonConvert.DeserializeObject<AmbassadorResponseA>(ambassadorStatus);
                            if (ambassadorResponse.code == 200)
                            {
                                //await DisplayAlert("Great!", "We have verified your code! We are just checking if you meet the threshold to apply for an additional discount.", "OK");
                                if (GetSubTotal() >= ambassadorResponse.sub.threshold)
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000025"))
                                        {
                                            await DisplayAlert(messageList["701-000025"].title, messageList["701-000025"].message, messageList["701-000025"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Great!", "Your purchase meets the threshold to apply an additional discount", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Great!", "Your purchase meets the threshold to apply an additional discount", "OK");
                                    }

                                    if (total <= ambassadorResponse.sub.discount_amount)
                                    {
                                        discountFromAmbassador.Text = "$" + total.ToString("N2");
                                        ambassadorDiscount = total;
                                        purchase.setAmbassadorCode(ambassadorDiscount.ToString("N2"));
                                    }
                                    else
                                    {
                                        discountFromAmbassador.Text = "$" + ambassadorResponse.sub.discount_amount.ToString("N2");
                                        ambassadorDiscount = ambassadorResponse.sub.discount_amount;
                                        purchase.setAmbassadorCode(ambassadorDiscount.ToString("N2"));
                                    }

                                    couponsUIDs = "";

                                    foreach (string uid in ambassadorResponse.uids)
                                    {
                                        couponsUIDs += "," + uid;
                                    }

                                    if (appliedCoupon != null)
                                    {
                                        updateTotals(appliedCoupon.discount, appliedCoupon.shipping);
                                    }
                                    else
                                    {
                                        updateTotals(0, 0);
                                    }

                                }
                                else
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000026"))
                                        {
                                            await DisplayAlert(messageList["701-000026"].title, messageList["701-000026"].message + (GetSubTotal() - ambassadorResponse.sub.threshold), messageList["701-000026"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oops!", "Your purchase is under the threshold to have an additional discount. You can save this code for another purchase or increase your subtotal by $" + (GetSubTotal() - ambassadorResponse.sub.threshold), "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops!", "Your purchase is under the threshold to have an additional discount. You can save this code for another purchase or increase your subtotal by $" + (GetSubTotal() - ambassadorResponse.sub.threshold), "OK");
                                    }
                                    
                                }
                            }
                        }
                        else
                        {
                            var ambassadorResponse = JsonConvert.DeserializeObject<AmbassadorResponseB>(ambassadorStatus);
                            await DisplayAlert("Oops", ambassadorResponse.message, "OK");
                        }
                    }
                }
                else
                {
                    couponsUIDs = "";
                    ambassadorDiscount = 0.0;
                    discountFromAmbassador.Text = "$0.00";
                    purchase.setAmbassadorCode("0.00");

                    if (appliedCoupon != null)
                    {
                        updateTotals(appliedCoupon.discount, appliedCoupon.shipping);
                    }
                    else
                    {
                        updateTotals(0, 0);
                    }
                }
            }catch(Exception errorVerifyAmbassadorCode)
            {
                var client = new Diagnostic();
                client.parseException(errorVerifyAmbassadorCode.ToString(), user);
            }
        }

        async void PurchaseBalanceIsZero(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (user.getUserType() == "GUEST")
                {
                    var client1 = new SignUp();

                    if (client1.GuestCheckAllRequiredEntries(firstName, lastName, emailAddress, phoneNumber))
                    {
                        var button = (Button)sender;

                        if (button.BackgroundColor == Color.FromHex("#FF8500"))
                        {
                            button.BackgroundColor = Color.FromHex("#2B6D74");
                            FinalizePurchase(purchase, selectedDeliveryDate);
                            purchase.setPurchaseFirstName(firstName.Text);
                            purchase.setPurchaseLastName(lastName.Text);
                            purchase.setPurchaseEmail(emailAddress.Text);
                            purchase.setPurchasePhoneNumber(phoneNumber.Text);
                            purchase.setPurchasePaymentType("SFGiftCard");
                            UserDialogs.Instance.ShowLoading("Your payment is processing...");

                            var client = new SignIn();
                            var isEmailUnused = await client.ValidateExistingAccountFromEmail(purchase.getPurchaseEmail());
                            if (isEmailUnused == null)
                            {
                                var userID = await SignUp.SignUpNewUser(SignUp.GetUserFrom(purchase));
                                if (userID != "")
                                {
                                    purchase.setPurchaseCustomerUID(userID);

                                    await WriteFavorites(GetFavoritesList(), userID);

                                    var coupond = purchase.getPurchaseCoupoID();
                                    purchase.setPurchaseCoupoID(coupond + couponsUIDs);
                                    purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                                    purchase.setPurchaseChargeID("");
                                    purchase.printPurchase();
                                    paymentClient = new Payments("SFTEST");
                                    _ = paymentClient.SendPurchaseToDatabase(purchase);
                                    order.Clear();
                                    UserDialogs.Instance.HideLoading();
                                    Application.Current.MainPage = new ConfirmationPage();

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
                                        if (messageList != null)
                                        {
                                            if (messageList.ContainsKey("701-000027"))
                                            {
                                                await DisplayAlert(messageList["701-000027"].title, messageList["701-000027"].message, messageList["701-000027"].responses);
                                            }
                                            else
                                            {
                                                await DisplayAlert("Great!", "It looks like you already have an account. Please log in to find out if you have any additional discounts", "OK");
                                            }
                                        }
                                        else
                                        {
                                            await DisplayAlert("Great!", "It looks like you already have an account. Please log in to find out if you have any additional discounts", "OK");
                                        }
                                        
                                        await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94, "1"), true);
                                    }
                                    else if (role == "GUEST")
                                    {
                                        // we don't sign up but get user id
                                        user.setUserID(isEmailUnused.result[0].customer_uid);
                                        purchase.setPurchaseCustomerUID(user.getUserID());
                                        user.setUserFromProfile(isEmailUnused);
                                        //var paymentIsSuccessful = paymentClient.PayViaStripe(
                                        //    purchase.getPurchaseEmail(),
                                        //    guestCardHolderName.Text,
                                        //    guestCardHolderNumber.Text,
                                        //    guestCardCVV.Text,
                                        //    guestCardExpMonth.Text,
                                        //    guestCardExpYear.Text,
                                        //    purchase.getPurchaseAmountDue()
                                        //    );

                                        await WriteFavorites(GetFavoritesList(), user.getUserID());

                                        var coupond = purchase.getPurchaseCoupoID();
                                        purchase.setPurchaseCoupoID(coupond + couponsUIDs);
                                        purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                                        purchase.setPurchaseChargeID("");
                                        paymentClient = new Payments("SFTEST");
                                        _ = paymentClient.SendPurchaseToDatabase(purchase);
                                        order.Clear();
                                        UserDialogs.Instance.HideLoading();
                                        Application.Current.MainPage = new ConfirmationPage();


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
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000028"))
                            {
                                await DisplayAlert(messageList["701-000028"].title, messageList["701-000028"].message, messageList["701-000028"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "You seem to forgot to fill in all entries. Please fill in all entries to continue", "OK");
                        }
                        
                    }
                }
                else
                {
                    FinalizePurchase(purchase, selectedDeliveryDate);
                    purchase.setPurchasePaymentType("SFGiftCard");
                    paymentClient = new Payments("SFTEST");
                    var coupond = purchase.getPurchaseCoupoID();
                    purchase.setPurchaseCoupoID(coupond + couponsUIDs);
                    purchase.setPurchaseBusinessUID(SignUp.GetDeviceInformation() + SignUp.GetAppVersion());
                    purchase.setPurchaseChargeID("");
                    _ = paymentClient.SendPurchaseToDatabase(purchase);
                    order.Clear();
                    await WriteFavorites(GetFavoritesList(), purchase.getPurchaseCustomerUID());
                    Application.Current.MainPage = new HistoryPage();
                }
            }catch(Exception errorPurchaseBalanceIsZero)
            {
                var client = new Diagnostic();
                client.parseException(errorPurchaseBalanceIsZero.ToString(), user);
            }
        }

        async void AmbassadorShowInformation(System.Object sender, System.EventArgs e)
        {
            if(user.getUserType() == "GUEST")
            {
                //await DisplayAlert("Love Serving Fresh?", "Become an Ambassador\n\nGive 20, Get 20\n\nRefer a friend and both you and your friend get $10 off on your next two orders.", "Login", "Sign Up");
                string action = await DisplayActionSheet("Love Serving Fresh?\n\nBecome an Ambassador\n\nGive 20, Get 20\n\nRefer a friend and both you and your friend get $10 off on your next two orders.", "Cancel", null, "Login", "Sign Up");
                if(action == "Login")
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94, "1"), true);
                }else if (action == "Sign Up")
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new SignUpPage(94), true);
                }

            }
            else if(user.getUserType() == "CUSTOMER")
            {
                
                string action = await DisplayPromptAsync("Love Serving Fresh?", "\n\nBecome an Ambassador\n\nGive 20, Get 20\n\nRefer a friend and both you and your friend get $10 off on your next two orders.", "OK","Cancel",null,-1,Keyboard.Email,null);
                if (!String.IsNullOrEmpty(action))
                {
                    // Make input an ambassador...

                    var client = new Ambassador();
                    var createAmbassadorStatus = await client.CreateAmbassadorFromCode(action.ToLower());
                    if (createAmbassadorStatus)
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000029"))
                            {
                                await DisplayAlert(messageList["701-000029"].title, messageList["701-000029"].message + action + "with your friends and get $10 on your next two orders", messageList["701-000029"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Congratulations!", "You are now an ambassador! Share this email: " + action + "with your friends and get $10 on your next two orders", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Congratulations!", "You are now an ambassador! Share this email: " + action + "with your friends and get $10 on your next two orders", "OK");
                        }
                       
                    }
                    else
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000030"))
                            {
                                await DisplayAlert(messageList["701-000030"].title, messageList["701-000030"].message, messageList["701-000030"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "Something went wrong. Please try again", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "Something went wrong. Please try again", "OK");
                        }
                       
                    }
                }
            }
        }

        void ShowTermsAndConditions(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new TermsAndConditionsPage(), false);
        }
    }
}
