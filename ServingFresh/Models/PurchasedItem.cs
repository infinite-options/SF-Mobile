using System;
namespace ServingFresh.Models
{
    public class PurchasedItem
    {
        public string item_uid { get; set; }
        public string qty { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public string itm_business_uid { get; set; }
    }
}
