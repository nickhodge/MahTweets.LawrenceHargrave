using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IFilter
    {
        bool Include { get; }
        bool? Filter(IStatusUpdate update);
    }
}