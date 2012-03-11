using System.Runtime.Serialization;
using MahTweets.Core;

namespace MahTweets.TwitterPlugin.UI
{
    [DataContract]
    public class DirectMessageTweet : Tweet
    {
        [DataMember]
        public Contact Recipient { get; set; }
    }
}