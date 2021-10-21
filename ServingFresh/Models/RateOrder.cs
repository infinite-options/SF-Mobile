using System;
using System.Collections.Generic;

namespace ServingFresh.Models
{
    public class RateOrder
    {
        public string purchase_uid { get; set; }
        public string rating { get; set; }
        public string comment { get; set; }

        public RateOrder()
        {
            purchase_uid = "";
            rating = "";
            comment = "";
        }
    }
}
