using System.Runtime.Serialization;
using System.Windows.Media;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters
{
    [DataContract]
    public abstract class Filter
    {
        [DataMember]
        public FilterBehaviour IsIncluded { get; set; }

        [DataMember]
        public Color Color { get; set; }

        public abstract bool IsMatch(IStatusUpdate update);
    }
}