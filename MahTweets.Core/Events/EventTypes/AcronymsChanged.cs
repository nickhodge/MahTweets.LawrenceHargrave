using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Events.EventTypes
{
    public class AcronymsChanged : CompositePresentationEvent<IStatusHandler>
    {
    }
}