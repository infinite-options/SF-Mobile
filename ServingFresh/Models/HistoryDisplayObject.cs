using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ServingFresh.Models
{
    public class HistoryDisplayObject : INotifyPropertyChanged
    {
        public ObservableCollection<HistoryItemObject> items { get; set; }
        public string delivery_date { get; set; }
        public int itemsHeight { get; set; }
        public string purchase_status { get; set; }
        public string purchase_id { get; set; }
        public string original_purchase_id { get; set; }
        public string purchase_date { get; set; }
        public string subtotal { get; set; }
        public string promo_applied { get; set; }
        public string delivery_fee { get; set; }
        public string service_fee { get; set; }
        public string driver_tip { get; set; }
        public string taxes { get; set; }
        public string total { get; set; }
        public string coupon_id { get; set; }
        public string ambassador_code { get; set; }
        public bool isRateOrderButtonAvailable { get; set; }
        public bool isRateIconAvailable { get; set; }
        public string ratingSourceIcon { get; set; }

        public bool updateIsRateOrderButtonAvaiable
        {
            set
            {
                isRateOrderButtonAvailable = value;
                OnPropertyChanged(nameof(isRateOrderButtonAvailable));
            }
        }

        public bool updateIsRateIconAvailable
        {
            set
            {
                isRateIconAvailable = value;
                OnPropertyChanged(nameof(isRateIconAvailable));
            }
        }

        public string updateRatingSourceIcon
        {
            set
            {
                ratingSourceIcon = value;
                OnPropertyChanged(nameof(ratingSourceIcon));
            }
        }

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
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
        public double ambassador_code { get; set; }
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
        public string description { get; set; }
        public string business_price { get; set; }

        public string namePriceUnit
        {
            get
            {
                return "(" + unit + ")";
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
}
