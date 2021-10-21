using System;
namespace ServingFresh.Models
{
    public class StripePaymentIntent
    {
        public string currency { get; set; }
        public string customer_uid { get; set; }
        public string business_code { get; set; }
        public PaymentSummary payment_summary { get; set; }
    }

    public class PaymentSummary
    {
        public string total { get; set; }
    }
}
