using System;
namespace ServingFresh.Models
{
    public class PurchasedItem
    {
        public string img { get; set; }
        public int qty { get; set; }
        public string name { get; set; }
        public string unit { get; set; }
        public double price { get; set; }
        public string item_uid { get; set; }
        public string itm_business_uid { get; set; }
        
        //public string description { get; set; }
    }
}
