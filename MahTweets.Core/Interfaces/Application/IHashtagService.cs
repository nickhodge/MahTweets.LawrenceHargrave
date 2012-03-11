using System.Collections.ObjectModel;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IHashtagService
    {
        ObservableCollection<Hashtag> Hashtags { get; }
        Hashtag GetOrCreateHashtag(string Tag, IMicroblog microblog);
    }
}