using System.Runtime.Serialization;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters
{
    [DataContract]
    public class ContactFilter : Filter
    {
        public ContactFilter(FilterBehaviour behaviour, IContact contact)
        {
            IsIncluded = behaviour;
            ContactName = contact.Name;
            AccountName = contact.Source.Protocol;
        }

        [DataMember]
        public string ContactName { get; private set; }

        [DataMember]
        public string AccountName { get; private set; }

        public override bool IsMatch(IStatusUpdate update)
        {
            if (IsIncluded == FilterBehaviour.NoBehaviour) return false;
            return IsMatch(update.Contact);
        }

        public bool IsMatch(IContact contact)
        {
            if (IsIncluded == FilterBehaviour.NoBehaviour) return false;
            return contact.Name.Matches(ContactName, true) && contact.Source.Protocol.Matches(AccountName, true);
        }
    }
}