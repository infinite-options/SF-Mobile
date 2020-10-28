using System;
namespace ServingFresh.LogIn.Classes
{
    public class SignUpResponse
    {
        public string message { get; set; }
        public int code { get; set; }
        public string sql { get; set; }
        public Result result { get; set; }
    }

    public class Result
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string customer_uid { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}
