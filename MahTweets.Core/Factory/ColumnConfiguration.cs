using System.Collections.Generic;
using System.Runtime.Serialization;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Factory
{
    [DataContract]
    public class ColumnConfiguration : IContainerConfiguration
    {
        public ColumnConfiguration()
        {
            Mapping = new Dictionary<string, string>();
        }

        #region IContainerConfiguration Members

        [DataMember]
        public string ContainerType { get; set; }

        [DataMember]
        public string Uuid { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public IDictionary<string, string> Mapping { get; set; }

        #endregion
    }

    public class ColumnConfigurationComparer : IEqualityComparer<ColumnConfiguration>
    {
        #region IEqualityComparer<ColumnConfiguration> Members

        public bool Equals(ColumnConfiguration c1, ColumnConfiguration c2)
        {
            if (c1.Title.ToLower() == c2.Title.ToLower()) return true;
            if (c1.Uuid == c2.Uuid) return true;
            return false;
        }

        public int GetHashCode(ColumnConfiguration c1)
        {
            return c1.Uuid.GetHashCode();
        }

        #endregion
    }
}