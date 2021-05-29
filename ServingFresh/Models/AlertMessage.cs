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
        public string alert_message { get; set; }
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

        public async Task<IList<MessageResult>> GetMessageList()
        {
            IList<MessageResult> result = null;
            var client = new HttpClient();
            var endpointCall = await client.GetAsync(Constant.AlertMessage);
            if (endpointCall.IsSuccessStatusCode)
            {
                var content = await endpointCall.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Message>(content);
                foreach (MessageResult key in data.result)
                {
                    Debug.WriteLine(key.alert_message);
                }
                result = data.result;
            }
            return result;
        }
    }
}
