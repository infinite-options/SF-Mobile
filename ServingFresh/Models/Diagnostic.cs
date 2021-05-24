using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ServingFresh.Config;
namespace ServingFresh.Models
{
    public class tryCatch
    {
        public string customer_uid { get; set; }
        public string caught_output { get; set; }
        public string functions { get; set; }
        public string files { get; set; }
        public string line_number { get; set; }
        public string types { get; set; }
    }

    public class Diagnostic
    {
        public Diagnostic()
        {

        }

        public void parseException(string ex, User user)
        {
            tryCatch sendExc = new tryCatch();
            
            Debug.WriteLine("full exception: " + ex.ToString());
            string exFile = ex.ToString().Substring(ex.ToString().LastIndexOf("/") + 1);
            exFile = exFile.Substring(0, exFile.IndexOf(":"));
            string exFunc = ex.ToString().Substring(ex.ToString().IndexOf("at") + 3);
            exFunc = exFunc.Substring(0, exFunc.IndexOf(")") + 1);
            Debug.WriteLine("exception type: " + ex.ToString().Substring(0, ex.ToString().IndexOf("at") - 1).Trim());
            Debug.WriteLine("exception function: " + exFunc);
            Debug.WriteLine("exception file: " + exFile);
            Debug.WriteLine("exception line: " + ex.ToString().Substring(ex.ToString().LastIndexOf(":") + 1));


            //if ((string)Application.Current.Properties["platform"] != "GUEST")
            //    sendExc.customer_uid = (string)Application.Current.Properties["platform"];
            //else sendExc.customer_uid = "GUEST";
            sendExc.customer_uid = user.getUserPlatform();
            sendExc.caught_output = ex;
            sendExc.types = ex.ToString().Substring(0, ex.ToString().IndexOf("at") - 1).Trim();
            sendExc.functions = exFunc;
            sendExc.files = exFile;
            sendExc.line_number = ex.ToString().Substring(ex.ToString().LastIndexOf(":") + 1);


            var exceptionJSONString = JsonConvert.SerializeObject(sendExc);
            var content2 = new StringContent(exceptionJSONString, Encoding.UTF8, "application/json");
            Console.WriteLine("Content: " + content2);
            var client = new HttpClient();
            var response = client.PostAsync(Constant.ErroDiagnostic, content2);
            // HttpResponseMessage response = await client.SendAsync(request);
            Console.WriteLine("RESPONSE TO TRY CATCH   " + response.Result);
            Console.WriteLine("TRY CATCH JSON OBJECT BEING SENT: " + exceptionJSONString);
        }
    }
}
