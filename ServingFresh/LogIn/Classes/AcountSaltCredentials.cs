using System;
using System.Collections.Generic;

namespace ServingFresh.LogIn.Classes
{
    public class AcountSaltCredentials
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<AccountSalt> result { get; set; }
        public string sql { get; set; }
    }

    public class AccountSalt
    {
        public string password_algorithm { get; set; }
        public string password_salt { get; set; }
    }
}
