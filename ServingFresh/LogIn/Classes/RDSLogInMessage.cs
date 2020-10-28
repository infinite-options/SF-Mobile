using System;
namespace ServingFresh.LogIn.Classes
{
    public class RDSLogInMessage
    {
        public string message { get; set; }
        public int code { get; set; }
        public string result { get; set; }
        public string sql { get; set; }
    }
}
