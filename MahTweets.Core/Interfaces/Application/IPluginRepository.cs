using System.Collections.ObjectModel;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IPluginRepository
    {
        ObservableCollection<IMicroblog> Microblogs { get; }
        ObservableCollection<IUrlShortener> UrlShorteners { get; }
        ObservableCollection<IStatusHandler> StatusHandlers { get; }
        T GetMicroblogByAccount<T>(string accountName) where T : IMicroblog;
        IMicroblog GetMicroblogByAccount(string type, string accountName);

        void Clear();
        void Refresh(bool force);
        void Close();
    }
}