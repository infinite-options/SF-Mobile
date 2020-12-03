using System;
using System.Collections.Generic;

namespace ServingFresh.Models
{
    //public class ServingFreshBusiness
    //{
    //    public string message { get; set; }
    //    public int code { get; set; }
    //    public IList<Business> result { get; set; }
    //}

    //public class Business
    //{
    //    public string business_uid { get; set; }
    //    public DateTime? business_created_at { get; set; }
    //    public string business_association { get; set; }
    //    public string business_name { get; set; }
    //    public string business_type { get; set; }
    //    public string business_desc { get; set; }
    //    public string business_contact_first_name { get; set; }
    //    public string business_contact_last_name { get; set; }
    //    public string business_phone_num { get; set; }
    //    public string business_phone_num2 { get; set; }
    //    public string business_email { get; set; }
    //    public string business_hours { get; set; }
    //    public string business_accepting_hours { get; set; }
    //    public string business_delivery_hours { get; set; }
    //    public string business_address { get; set; }
    //    public string business_unit { get; set; }
    //    public string business_city { get; set; }
    //    public string business_state { get; set; }
    //    public string business_zip { get; set; }
    //    public object business_longitude { get; set; }
    //    public object business_latitude { get; set; }
    //    public object business_EIN { get; set; }
    //    public object business_WAUBI { get; set; }
    //    public object business_license { get; set; }
    //    public object business_USDOT { get; set; }
    //    public object notification_approval { get; set; }
    //    public object notification_device_id { get; set; }
    //    public int? can_cancel { get; set; }
    //    public int? delivery { get; set; }
    //    public int? reusable { get; set; }
    //    public string business_image { get; set; }
    //    public string business_password { get; set; }
    //    public string item_type { get; set; }
    //    public string itm_business_uid { get; set; }
    //}

    // Old implementation of categorical endpoint
    // ==========================================
    //public class Business
    //{
    //    public string business_uid { get; set; }
    //    public DateTime business_created_at { get; set; }
    //    public string business_name { get; set; }
    //    public string business_type { get; set; }
    //    public string business_desc { get; set; }
    //    public string business_association { get; set; }
    //    public string business_contact_first_name { get; set; }
    //    public string business_contact_last_name { get; set; }
    //    public string business_phone_num { get; set; }
    //    public string business_phone_num2 { get; set; }
    //    public string business_email { get; set; }
    //    public string business_hours { get; set; }
    //    public string business_accepting_hours { get; set; }
    //    public string business_delivery_hours { get; set; }
    //    public string business_address { get; set; }
    //    public string business_unit { get; set; }
    //    public string business_city { get; set; }
    //    public string business_state { get; set; }
    //    public string business_zip { get; set; }
    //    public string business_longitude { get; set; }
    //    public string business_latitude { get; set; }
    //    public string business_EIN { get; set; }
    //    public string business_WAUBI { get; set; }
    //    public string business_license { get; set; }
    //    public string business_USDOT { get; set; }
    //    public string bus_notification_approval { get; set; }
    //    public int? can_cancel { get; set; }
    //    public int? delivery { get; set; }
    //    public int? reusable { get; set; }
    //    public string business_image { get; set; }
    //    public string business_password { get; set; }
    //    public string bus_guid_device_id_notification { get; set; }
    //    public double platform_fee { get; set; }
    //    public double transaction_fee { get; set; }
    //    public double revenue_sharing { get; set; }
    //    public double profit_sharing { get; set; }
    //    public string itm_business_uid { get; set; }
    //    public string item_type { get; set; }
    //}

    //public class ServingFreshBusiness
    //{
    //    public string message { get; set; }
    //    public int code { get; set; }
    //    public IList<Business> result { get; set; }
    //    public string sql { get; set; }
    //}
    // ==========================================

    public class Business
    {
        public string zone { get; set; }
        public int z_id { get; set; }
        public string z_biz_id { get; set; }
        public string business_name { get; set; }
        public string z_delivery_day { get; set; }
        public string z_delivery_time { get; set; }
        public string business_type { get; set; }
        public string business_image { get; set; }
        public IDictionary<string,IList<string>> delivery_days { get; set; }
    }

    public class ServingFreshBusiness
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<Business> result { get; set; }
        public string sql { get; set; }
    }
}
