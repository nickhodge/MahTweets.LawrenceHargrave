using System.Runtime.Serialization;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters
{
    [DataContract]
    public class UpdateTypeFilter : Filter
    {
        public UpdateTypeFilter(FilterBehaviour behaviour, IMicroblog blog, UpdateType updateType)
        {
            IsIncluded = behaviour;
            Microblog = blog;
            UpdateType = updateType;

            MicroblogAccountName = blog.Credentials.AccountName;
            MicroblogName = blog.Protocol;
            UpdateTypeName = UpdateType.GetType().AssemblyQualifiedName;

            if (UpdateType.SaveType)
                UpdateTypeParameter = UpdateType.Type;
        }

        [DataMember]
        public string MicroblogAccountName { get; set; }

        [DataMember]
        public string MicroblogName { get; set; }

        [IgnoreDataMember]
        public IMicroblog Microblog { get; private set; }

        [DataMember]
        public string UpdateTypeName { get; set; }

        [DataMember]
        public string UpdateTypeParameter { get; set; }

        [IgnoreDataMember]
        public UpdateType UpdateType { get; private set; }

        public override bool IsMatch(IStatusUpdate update)
        {
            if (IsIncluded == FilterBehaviour.NoBehaviour) return false;
            return update.Parents.Contains(Microblog) && update.Types.Contains(UpdateType);
        }

        public void SetUpdateType(UpdateType type)
        {
            UpdateType = type;
        }

        public void SetMicroblog(IMicroblog findMicroblog)
        {
            Microblog = findMicroblog;
        }
    }
}