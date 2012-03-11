using MahTweets.Core.Collections;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IStatusUpdateService : IStatusUpdateRepository
    {
        ThreadSafeObservableCollection<IStatusUpdate> OutgoingUpdates { get; }

        void Reset();

        void MarkAllRead();
    }
}