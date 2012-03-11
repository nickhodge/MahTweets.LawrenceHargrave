using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Events.EventTypes
{
    public class MicroblogAdded : CompositePresentationEvent<IMicroblog>
    {
    }

    public class PluginAdded : CompositePresentationEvent<IPlugin>
    {
    }
}