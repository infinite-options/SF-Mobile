using System;
using System.Collections.Generic;

namespace ServingFresh.Models
{
    public class Coupon
    {
        public string coupon_uid { get; set; }
        public string coupon_id { get; set; }
        public string valid { get; set; }
        public object threshold { get; set; }
        public double discount_percent { get; set; }
        public double discount_amount { get; set; }
        public double discount_shipping { get; set; }
        public string expire_date { get; set; }
        public int limits { get; set; }
        public string notes { get; set; }
        public int num_used { get; set; }
        public string recurring { get; set; }
        public string email_id { get; set; }
        public string cup_business_uid { get; set; }
    }

    public class CouponResponse
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<Coupon> result { get; set; }
        public string sql { get; set; }
    }
}
