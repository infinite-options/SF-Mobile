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
        public AlertMessage()
        {

        }

        public async Task<Dictionary<string, MessageResult>> GetMessageList()
        {
            Dictionary<string, MessageResult> result = null;
            var client = new HttpClient();
            var endpointCall = await client.GetAsync(Constant.AlertMessage);
            if (endpointCall.IsSuccessStatusCode)
            {
                var content = await endpointCall.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Message>(content);

                result = new Dictionary<string, MessageResult>();

                foreach (MessageResult message in data.result)
                {
                    if (!result.ContainsKey(message.alert_uid))
                    {
                        result.Add(message.alert_uid, message);
                    }
                }
            }
            return result;
        }
    }
}
