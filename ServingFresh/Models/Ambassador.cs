using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServingFresh.Config;

namespace ServingFresh.Models
{
    public class Ambassador
    {
        public Ambassador()
        {
        }

        public async Task<string> ValidateAmbassadorCode(string code, string info, string IsGuest)
        {
            string result = "";

            var client = new HttpClient();
            var ambassador = new AmbassadorPost();

            ambassador.code = code;
            ambassador.info = info;
            ambassador.IsGuest = IsGuest;

            var serializedObject = JsonConvert.SerializeObject(ambassador);
            var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var endpointCall = await client.PostAsync(Constant.AmbassadorChecker, content);

            Debug.WriteLine("JSON TO BE SEND: " + serializedObject);

            if (endpointCall.IsSuccessStatusCode)
            {
                var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                result = endpointContentString;
                //result = JsonConvert.DeserializeObject<AmbassadorResponse>(endpointContentString);
            }

            return result;
        }

        public async Task<string> CreateAmbassadorFromCode(string code)
        {
            string result = "";

            var client = new HttpClient();
            var ambassador = new CreateAmbassador();

            ambassador.code = code;

            var serializedObject = JsonConvert.SerializeObject(ambassador);
            var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var endpointCall = await client.PostAsync(Constant.CreateAmbassador, content);

            Debug.WriteLine("JSON TO BE SEND: " + serializedObject);

            if (endpointCall.IsSuccessStatusCode)
            {
                var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                if(endpointContentString.Contains("SF Ambassdaor created"))
                {
                    result = "SF Ambassdaor created";
                }
                else if (endpointContentString.Contains("Customer already an Ambassador"))
                {
                    result = "Customer already an Ambassador";
                }
            }

            return result;
        }
    }

    public class AmbassadorPost
    {
        public string code { get; set; }
        public string info { get; set; }
        public string IsGuest { get; set; }
    }

    public class CreateAmbassador
    {
        public string code { get; set; }
    }

    public class Sub
    {
        public string coupon_uid { get; set; }
        public string coupon_id { get; set; }
        public string valid { get; set; }
        public double threshold { get; set; }
        public double discount_percent { get; set; }
        public double discount_amount { get; set; }
        public double discount_shipping { get; set; }
        public string expire_date { get; set; }
        public int limits { get; set; }
        public object coupon_title { get; set; }
        public string notes { get; set; }
        public int num_used { get; set; }
        public string recurring { get; set; }
        public string email_id { get; set; }
        public string cup_business_uid { get; set; }
    }

    public class AmbassadorResponseA
    {
        public string message { get; set; }
        public int code { get; set; }
        public double discount { get; set; }
        public IList<string> uids { get; set; }
        public Sub sub { get; set; }
    }

    public class AmbassadorResponseB
    {
        public string message { get; set; }
        public int code { get; set; }
        public string discount { get; set; }
        public string uids { get; set; }
    }
}
