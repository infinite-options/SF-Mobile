﻿using System;
using System.Collections.Generic;

namespace ServingFresh.Models
{
    public class Profile
    {
        public string customer_uid { get; set; }
        public string customer_created_at { get; set; }
        public string customer_first_name { get; set; }
        public string customer_last_name { get; set; }
        public string role { get; set; }
        public string user_social_media { get; set; }
        public string referral_source { get; set; }
        public string customer_phone_num { get; set; }
        public string customer_email { get; set; }
        public string customer_address { get; set; }
        public string customer_unit { get; set; }
        public string customer_city { get; set; }
        public string customer_state { get; set; }
        public string customer_zip { get; set; }
        public string customer_lat { get; set; }
        public string customer_long { get; set; }
        public object cust_notification_approval { get; set; }
        public object SMS_freq_preference { get; set; }
        public object SMS_last_notification { get; set; }
        public object notification_group { get; set; }
        public object customer_rep { get; set; }
        public string password_salt { get; set; }
        public string password_hashed { get; set; }
        public string password_algorithm { get; set; }
        public object customer_updated_at { get; set; }
        public object email_verified { get; set; }
        public string social_id { get; set; }
        public string user_access_token { get; set; }
        public string user_refresh_token { get; set; }
        public string mobile_access_token { get; set; }
        public string mobile_refresh_token { get; set; }
        public string social_timestamp { get; set; }
        public string cust_guid_device_id_notification { get; set; }
        public object favorite_produce { get; set; }
    }

    public class UserProfile
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<Profile> result { get; set; }
        public string sql { get; set; }
    }
}
