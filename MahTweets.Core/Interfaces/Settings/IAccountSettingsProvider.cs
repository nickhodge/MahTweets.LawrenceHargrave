using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MahTweets.Core.Interfaces.Settings
{
    [InheritedExport]
    public interface IAccountSettingsProvider
    {
        IList<string> ActiveStatusHandlers { get; }

        IList<Credential> UrlShortenerCredentials { get; }

        IList<Credential> MicroblogCredentials { get; }

        IList<Credential> StatusHandlerCredentials { get; }

        void Save();

        void Reset();
    }
}