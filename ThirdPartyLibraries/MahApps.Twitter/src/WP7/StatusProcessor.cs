using MahApps.Twitter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MahApps.Twitter
{
    public static class StatusProcessor
    {
        public static ITwitterResponse Process(string content)
        {
            string saneText = content.Trim();
            ITwitterResponse deserialisedResponse = null;

            if (saneText.StartsWith("{\"direct_message\""))
            {
                var obj = JsonConvert.DeserializeObject<DirectMessageContainer>(saneText);
                deserialisedResponse = obj.DirectMessage;
            }
            else if (saneText.StartsWith("{\"target\":"))
            {
                deserialisedResponse = JsonConvert.DeserializeObject<StreamEvent>(saneText);
            }
            else if (saneText.StartsWith("{\"delete\":"))
            {
                /*{"delete":{"status":{"user_id_str":"44504925","id_str":"66791879353708544","id":66791879353708544,"user_id":44504925}}}*/
                JObject o = JObject.Parse(saneText);
                var id = (long) o["delete"]["status"]["id"];
                var userid = (long) o["delete"]["status"]["user_id"];

                deserialisedResponse = new Delete {Id = id, UserId = userid};
            }
            else if (saneText.StartsWith("{\"scrub_geo\":"))
            {
                // "{\"scrub_geo\":{"user_id":14090452,"user_id_str":"14090452","up_to_status_id":23260136625,"up_to_status_id_str":"23260136625"}}"
                // TODO: parse and convert to new object
            }
            else if (saneText.StartsWith("{\"limit\":"))
            {
                // {"limit":{"track":1234}}
                // TODO: parse and convert to new object
            }
                //else if (saneText.Contains("\"retweeted_status\":{"))
                //{
                //    deserialisedResponse = JsonConvert.DeserializeObject<Tweet>(saneText);
                //}
            else
            {
                deserialisedResponse = JsonConvert.DeserializeObject<Tweet>(saneText);
            }

            return deserialisedResponse;
        }

        public static T Process<T>(string content) where T : ITwitterResponse
        {
            return JsonConvert.DeserializeObject<T>(content.Trim());
        }
    }
}