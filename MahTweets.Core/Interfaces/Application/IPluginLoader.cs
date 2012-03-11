using System;
using System.Collections.Generic;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IPluginLoader
    {
        bool TryFind(Credential blogCredential, out IMicroblog blog);
        bool TryFind(Credential credential, Action<IMicroblog> onCreate, out IMicroblog blog);
        bool TryFind(Credential credential, out IUrlShortener shortener);
        bool TryFind(Credential credential, out IStatusHandler statusHandler);
        void TryFind(IEnumerable<string> searchProviders, out IList<ISearchProvider> providers);
    }
}