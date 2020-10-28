using System;
using System.Collections.Generic;

namespace ServingFresh.Models
{
    public class DeliveriesModel
    {
        public string delivery_date { get; set; }
        public string delivery_shortname { get; set; }
        public string delivery_dayofweek { get; set; }
        public List<FarmsModel> farms { get; set; }
    }

    public class FarmsModel
    {
        public string uid { get; set; }
        public string name { get; set; }
    }
}
