using System.Runtime.Serialization;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters
{
    [DataContract]
    public class TextFilter : Filter
    {
        public TextFilter(FilterBehaviour behaviour, string text)
        {
            IsIncluded = behaviour;
            Text = text;
        }

        [DataMember]
        public string Text { get; private set; }

        public override bool IsMatch(IStatusUpdate update)
        {
            if (IsIncluded == FilterBehaviour.NoBehaviour) return false;

            return update.Text.ToLowerInvariant().Contains(Text.ToLower());
        }

        public bool IsMatch(string text)
        {
            if (IsIncluded == FilterBehaviour.NoBehaviour) return false;

            return text.ToLowerInvariant().Contains(Text.ToLower());
        }
    }
}