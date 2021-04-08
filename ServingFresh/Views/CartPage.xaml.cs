using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using Xamarin.Forms;
using static ServingFresh.Views.ItemsPage;
using static ServingFresh.Views.SelectionPage;

namespace ServingFresh.Views
{
    public partial class CartPage : ContentPage
    {
        public static ObservableCollection<ItemObject> cartItems = new ObservableCollection<ItemObject>();

        public CartPage(IDictionary<string, ItemPurchased> order = null, ScheduleInfo deliveryInfo = null)
        {
            InitializeComponent();

            //expectedDelivery.Text = deliveryInfo.deliveryTimeStamp.ToString("dddd, MMM dd, yyyy, ")+ " between " + deliveryInfo.delivery_time;

            var client = 0;
            if (client == 0)
            {
                // guest
                customerPaymentsView.HeightRequest = 0;
                customerStripeInformationView.HeightRequest = 0;
                customerDeliveryAddressView.HeightRequest = 0;

                //foreach (string key in order.Keys)
                //{
                //    cartItems.Add(new ItemObject()
                //    {
                //        qty = order[key].item_quantity,
                //        name = order[key].item_name,
                //        priceUnit = "( $" + order[key].item_price.ToString("N2") + " / " + order[key].unit + " )",
                //        price = order[key].item_price,
                //        item_uid = order[key].item_uid,
                //        business_uid = order[key].pur_business_uid,
                //        img = order[key].img,
                //        unit = order[key].unit,
                //        description = order[key].description,
                //        business_price = order[key].business_price,
                //        taxable = order[key].taxable,
                //    });
                //    //orderCopy.Add(key, order[key]);
                //}

                //CartItems.ItemsSource = cartItems;
                //CartItems.HeightRequest = 56 * cartItems.Count;
            }
            else
            {
                // customer
                guestAddressInfoView.HeightRequest = 0;
                guestPaymentsView.HeightRequest = 0;
            }
        }

        //public async void GetAvailiableCoupons()
        //{
        //    var client = new System.Net.Http.HttpClient();
        //    var email = (string)Application.Current.Properties["user_email"];
        //    var RDSResponse = new HttpResponseMessage();
        //    if (!(bool)Application.Current.Properties["guest"])
        //    {
        //        RDSResponse = await client.GetAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/available_Coupons/" + email);
        //    }
        //    else
        //    {
        //        RDSResponse = await client.GetAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/available_Coupons/guest");
        //    }

        //    if (RDSResponse.IsSuccessStatusCode)
        //    {
        //        var result = await RDSResponse.Content.ReadAsStringAsync();
        //        couponData = JsonConvert.DeserializeObject<CouponResponse>(result);

        //        couponsList.Clear();

        //        Debug.WriteLine(result);

        //        double initialSubTotal = GetSubTotal();
        //        double initialDeliveryFee = GetDeliveryFee();
        //        double initialServiceFee = GetServiceFee();
        //        double initialTaxes = GetTaxes();
        //        double initialTotal = initialSubTotal + initialDeliveryFee + initialServiceFee + initialTaxes;


        //        foreach (Models.Coupon c in couponData.result)
        //        {
        //            // IF THRESHOLD IS NULL SET IT TO ZERO, OTHERWISE INITIAL VALUE STAYS THE SAME
        //            if (c.threshold == null) { c.threshold = 0.0; }
        //            double discount = 0;
        //            //double newTotal = 0;

        //            var coupon = new couponItem();
        //            //Debug.WriteLine("COUPON IDS: " + c.coupon_uid);
        //            coupon.couponId = c.coupon_uid;
        //            // INITIALLY, THE IMAGE OF EVERY COUPON IS GRAY. (PLATFORM DEPENDENT)
        //            if (Device.RuntimePlatform == Device.Android)
        //            {
        //                coupon.image = "CouponIconGray.png";
        //            }
        //            else
        //            {
        //                coupon.image = "CouponIcon.png";
        //            }

        //            // SET TITLE LABEL OF COUPON
        //            coupon.couponNote = c.notes;
        //            // SET THRESHOLD LABEL BASED ON THRESHOLD VALUE: 0 = NO MINIMUM PURCHASE, GREATER THAN 0 = SPEND THE AMOUNT OF THRESHOLD
        //            if ((double)c.threshold == 0)
        //            {
        //                coupon.threshold = 0;
        //                coupon.thresholdNote = "No minimum purchase";
        //            }
        //            else
        //            {
        //                coupon.threshold = (double)c.threshold;
        //                coupon.thresholdNote = "$" + coupon.threshold.ToString("N2") + " minimum purchase";
        //            }

        //            // SET EXPIRATION DATE
        //            coupon.expNote = "Expires: " + DateTime.Parse(c.expire_date).ToString("MM/dd/yyyy");
        //            coupon.index = 0;

        //            // CALCULATING DISCOUNT, SHIPPING, AND COUPON STATUS
        //            if (initialSubTotal >= (double)c.threshold)
        //            {
        //                if (initialSubTotal >= c.discount_amount)
        //                {
        //                    // All
        //                    discount = initialSubTotal - ((initialSubTotal - c.discount_amount) * (1.0 - (c.discount_percent / 100.0)));
        //                }
        //                else
        //                {
        //                    // Partly apply coupon: % discount and $ shipping
        //                    discount = initialSubTotal;
        //                }
        //                //newTotal = initialSubTotal - discount + initialServiceFee + (initialDeliveryFee - c.discount_shipping) + initialTaxes;
        //                coupon.discount = discount;
        //                coupon.shipping = c.discount_shipping;
        //                coupon.status = "ACTIVE";
        //                coupon.totalDiscount = coupon.discount + coupon.shipping;
        //            }
        //            else
        //            {
        //                coupon.discount = 0;
        //                coupon.shipping = 0;
        //                coupon.status = "NOT-ACTIVE";
        //                coupon.totalDiscount = coupon.discount + coupon.shipping;
        //            }
        //            couponsList.Add(coupon);
        //        }

        //        var activeCoupons = new List<couponItem>();
        //        var nonactiveCoupons = new List<couponItem>();

        //        foreach (couponItem a in couponsList)
        //        {
        //            if (a.status == "ACTIVE")
        //            {
        //                activeCoupons.Add(a);
        //            }
        //            else
        //            {
        //                nonactiveCoupons.Add(a);
        //            }
        //        }

        //        // TESTING
        //        couponsList.Clear();
        //        Debug.WriteLine("");
        //        Debug.Write("LIST OF ACTIVE COUPONS: ");
        //        foreach (couponItem e in activeCoupons)
        //        {
        //            Debug.Write(e.couponId + " ");

        //        }
        //        Debug.WriteLine("");
        //        Debug.WriteLine("");
        //        Debug.Write("List OF NONACTIVE COUPONS: ");
        //        foreach (couponItem e in nonactiveCoupons)
        //        {
        //            Debug.Write(e.couponId + " ");
        //        }

        //        Debug.WriteLine("");
        //        Debug.WriteLine("");

        //        // ALL COUPOUNS ARE ACTIVE
        //        if (nonactiveCoupons.Count == 0)
        //        {
        //            // PLACE TOTAL DISCOUNT VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
        //            var TotalDiscountArray = new List<double>();

        //            // TotalDiscountArray IS OUT OF ORDER FOR EXAMPLE = [5,3,6,4,5,2,1,]
        //            foreach (couponItem ActiveCoupon in activeCoupons)
        //            {
        //                TotalDiscountArray.Add(ActiveCoupon.totalDiscount);
        //            }

        //            // TotalDiscountArray IS IN ORDER = [1,2,3,4,5,6]
        //            TotalDiscountArray.Sort();

        //            Debug.WriteLine("");
        //            Debug.Write("SORTED TOTAL DISCOUNT VALUES: ");
        //            foreach (double Value in TotalDiscountArray)
        //            {
        //                Debug.Write(Value + " ");
        //            }

        //            // RECORD REPETITIONS + READING SORTED ARRAY BACKWARDS + WRITING THE RIGHT MESSAGE ON THE COUPON
        //            var Limit = TotalDiscountArray.Count;
        //            var LastIndex = Limit - 1;
        //            for (int i = 0; i < Limit; i++)
        //            {
        //                var Value = TotalDiscountArray[LastIndex];
        //                for (int j = 0; j < activeCoupons.Count; j++)
        //                {
        //                    if (Value == activeCoupons[j].totalDiscount)
        //                    {
        //                        activeCoupons[j].image = "CouponIconGreen.png";
        //                        activeCoupons[j].savingsOrSpendingNote = "You saved: $" + activeCoupons[j].totalDiscount.ToString("N2");
        //                        couponsList.Add(activeCoupons[j]);
        //                        activeCoupons.RemoveAt(j);
        //                        break;
        //                    }
        //                }
        //                LastIndex--;
        //            }
        //        }

        //        // ALL COUPONS ARE NON-ACTIVE
        //        if (activeCoupons.Count == 0)
        //        {
        //            // PLACE THRESHOLD VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
        //            var ThresholdArray = new List<double>();
        //            foreach (couponItem NonActiveCoupon in nonactiveCoupons)
        //            {
        //                ThresholdArray.Add(NonActiveCoupon.threshold);
        //            }

        //            ThresholdArray.Sort();

        //            var Limit = nonactiveCoupons.Count;
        //            var FirstIndex = 0;
        //            for (int i = 0; i < Limit; i++)
        //            {
        //                var Value = ThresholdArray[FirstIndex];
        //                for (int j = 0; j < nonactiveCoupons.Count; j++)
        //                {
        //                    if (Value == nonactiveCoupons[j].threshold)
        //                    {
        //                        nonactiveCoupons[j].savingsOrSpendingNote = "Spend $" + (nonactiveCoupons[j].threshold - initialSubTotal).ToString("N2") + " more to use";
        //                        couponsList.Add(nonactiveCoupons[j]);
        //                        nonactiveCoupons.RemoveAt(j);
        //                        break;
        //                    }
        //                }
        //                FirstIndex++;
        //            }
        //        }

        //        // COUPONS ARE ACTIVE AND NON-ACTIVE
        //        if (activeCoupons.Count != 0 && nonactiveCoupons.Count != 0)
        //        {
        //            // PLACE TOTAL DISCOUNT VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
        //            var TotalDiscountArray = new List<double>();

        //            // TotalDiscountArray IS OUT OF ORDER FOR EXAMPLE = [5,3,6,4,5,2,1,]
        //            foreach (couponItem ActiveCoupon in activeCoupons)
        //            {
        //                TotalDiscountArray.Add(ActiveCoupon.totalDiscount);
        //            }

        //            // TotalDiscountArray IS IN ORDER = [1,2,3,4,5,6]
        //            TotalDiscountArray.Sort();

        //            Debug.WriteLine("");
        //            Debug.Write("SORTED TOTAL DISCOUNT VALUES: ");
        //            foreach (double Value in TotalDiscountArray)
        //            {
        //                Debug.Write(Value + " ");
        //            }

        //            // RECORD REPETITIONS + READING SORTED ARRAY BACKWARDS + WRITING THE RIGHT MESSAGE ON THE COUPON
        //            var Limit = TotalDiscountArray.Count;
        //            var LastIndex = Limit - 1;
        //            for (int i = 0; i < Limit; i++)
        //            {
        //                var Value = TotalDiscountArray[LastIndex];
        //                for (int j = 0; j < activeCoupons.Count; j++)
        //                {
        //                    if (Value == activeCoupons[j].totalDiscount)
        //                    {
        //                        activeCoupons[j].image = "CouponIconGreen.png";
        //                        activeCoupons[j].savingsOrSpendingNote = "You saved: $" + activeCoupons[j].totalDiscount.ToString("N2");
        //                        couponsList.Add(activeCoupons[j]);
        //                        activeCoupons.RemoveAt(j);
        //                        break;
        //                    }
        //                }
        //                LastIndex--;
        //            }

        //            // PLACE THRESHOLD VALUE IN A ARRAY TO SORT FROM SMALLEST TO LARGEST
        //            var ThresholdArray = new List<double>();
        //            foreach (couponItem NonActiveCoupon in nonactiveCoupons)
        //            {
        //                ThresholdArray.Add(NonActiveCoupon.threshold);
        //            }

        //            ThresholdArray.Sort();

        //            Limit = nonactiveCoupons.Count;
        //            var FirstIndex = 0;
        //            for (int i = 0; i < Limit; i++)
        //            {
        //                var Value = ThresholdArray[FirstIndex];
        //                for (int j = 0; j < nonactiveCoupons.Count; j++)
        //                {
        //                    if (Value == nonactiveCoupons[j].threshold)
        //                    {
        //                        nonactiveCoupons[j].savingsOrSpendingNote = "Spend $" + (nonactiveCoupons[j].threshold - initialSubTotal).ToString("N2") + " more to use";
        //                        couponsList.Add(nonactiveCoupons[j]);
        //                        nonactiveCoupons.RemoveAt(j);
        //                        break;
        //                    }
        //                }
        //                FirstIndex++;
        //            }
        //        }

        //        if (couponsList.Count != 0)
        //        {
        //            if (couponsList[0].status == "ACTIVE")
        //            {
        //                couponsList[0].image = "CouponIconOrange.png";
        //                Debug.WriteLine("COUPON DISCOUNT: {0}, COUPON SHIPPING: {1}", couponsList[0].discount, couponsList[0].shipping);
        //                updateTotals(couponsList[0].discount, couponsList[0].shipping);
        //                appliedIndex = 0;
        //            }
        //        }
        //        else
        //        {
        //            updateTotals(0, 0);
        //        }

        //        for (int i = 0; i < couponsList.Count; i++)
        //        {
        //            couponsList[i].index = i;
        //        }

        //        coupon_list.ItemsSource = couponsList;
        //    }
        //}



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

        }

        void CheckoutWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if(button.BackgroundColor == Color.FromHex("#FF8500"))
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
    }
}
