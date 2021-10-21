using System;
namespace ServingFresh.Models
{
    public class UpdatedProfile
    {
        public string customer_first_name { get; set; }
        public string customer_last_name { get; set; }
        public string customer_phone_num { get; set; }
        public string customer_email { get; set; }
        public string customer_address { get; set; }
        public string customer_unit { get; set; }
        public string customer_city { get; set; }
        public string customer_state { get; set; }
        public string customer_zip { get; set; }
        public string customer_lat { get; set; }
        public string customer_long { get; set; }
        public string customer_uid { get; set; }
        public string guid { get; set; }

        public UpdatedProfile()
        {
        }

        public UpdatedProfile GetUpdatedProfile(UserProfile profile)
        {
            var newProfile = new UpdatedProfile()
            {
                customer_first_name = profile.result[0].customer_first_name,
                customer_last_name = profile.result[0].customer_last_name,
                customer_phone_num = profile.result[0].customer_phone_num,
                customer_email = profile.result[0].customer_email,
                customer_address = profile.result[0].customer_address,
                customer_unit = profile.result[0].customer_unit,
                customer_city = profile.result[0].customer_city,
                customer_state = profile.result[0].customer_state,
                customer_zip = profile.result[0].customer_zip,
                customer_lat = profile.result[0].customer_lat,
                customer_long = profile.result[0].customer_long,
                customer_uid = profile.result[0].customer_uid,
                guid = "",
            };
            return newProfile;
        }
    }


}
