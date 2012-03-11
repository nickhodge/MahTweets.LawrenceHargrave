using MahTweets.Core.Events.EventTypes;

namespace MahTweets.Core.VersionCheck
{
    public interface ICurrentVersionCheck
    {
        void HandleCheckMahTweetsVersion(CheckMahTweetsVersionMessage currentVersion);
    }
}
