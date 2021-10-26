using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServingFresh.Config;

namespace ServingFresh.Models
{
    public class MessageResult
    {
        public string alert_uid { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string responses { get; set; }
    }

    public class Message
    {
        public string message { get; set; }
        public int code { get; set; }
        public IList<MessageResult> result { get; set; }
        
    }





    public class AlertMessage
    {

        // this is where attributes of the class would go

        // default constructor for AlertMessage class
        // This is a way to create an object of type AlertMessage (the class)
        public AlertMessage()
        {
            //
        }


        // These are functions within the class

        // to make the GetMessageList function available throughout the program make it public and static
        //public static async Task<Dictionary<string, MessageResult>> GetMessageList()

        public async Task<Dictionary<string, MessageResult>> GetMessageList()
        {
            // Create a dictionary called result where the key is a string and the value is a MessageResult object and initialize it to null
            Dictionary<string, MessageResult> result = null;
            try
            {
                var client = new HttpClient();  //this comes from the nuget package (dotNet)

                // call a function within the HttpClient class called GetAsync and it take one argurement
                // **** THIS IS THE ENDPOINT CALL ****   This is an example of a GET
                var endpointCall = await client.GetAsync(Constant.AlertMessage);  // returns data (http response message)
                //var endpointCall2 = client.GetAsync(Constant.AlertMessage);         // returns a task

               

                // IsSuccessStatusCode is an attribute of HttpResponseMessage which is part of the nuget package (dotNet)
                if (endpointCall.IsSuccessStatusCode)
                {
                    var content = await endpointCall.Content.ReadAsStringAsync();  //convert Content type to string
                    var data = JsonConvert.DeserializeObject<Message>(content);    //convert String into a local class called Message

                    result = new Dictionary<string, MessageResult>();       // needed to allocate the memory to the variable result

                    foreach (MessageResult message in data.result)          //data.result has the whole list
                    {
                        if (!result.ContainsKey(message.alert_uid))         //if the dictionary does not already have this id, then ...
                        {
                            result.Add(message.alert_uid, message);         //add uid and message to dictionary
                        }
                    }
                }
            }
            catch
            {

            }
            return result;
        }
    }
}
