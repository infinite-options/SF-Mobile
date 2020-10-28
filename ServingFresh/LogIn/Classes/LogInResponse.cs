using System;
using System.Collections.Generic;

namespace ServingFresh.LogIn.Classes
{
    public class LogInResponse
    {
        public string message { get; set; }
        public IList<User> result { get; set; }
    }

    public class User
    {
        public string customer_uid { get; set; }
        public string customer_last_name { get; set; }
        public string customer_first_name { get; set; }
        public string customer_email { get; set; }
        public string user_social_media { get; set; }
        public string user_access_token { get; set; }
        public string user_refresh_token { get; set; }
    }
}
