using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.Core.Interfaces.Settings
{
    [InheritedExport]
    public interface IApplicationSettingsProvider
    {
        bool UseLocation { get; set; }

        double Latitude { get; set; }

        double Longitude { get; set; }

        bool DisableProfileColumns { get; set; }

        int MapEngine { get; set; }

        bool AutoExpandUrls { get; set; }

        MediaHandling MediaHandling { get; set; }

        IUrlShortener DefaultShortener { get; }

        double WindowWidth { get; set; }

        double WindowXPos { get; set; }

        double WindowHeight { get; set; }

        double WindowYPos { get; set; }

        bool IsCPURendering { get; set; }

        IList<IMediaProvider> MediaProviders { get; }

        ObservableCollection<SavedSearch> SavedSearches { get; }

        bool DisableMediaProviders { get; set; }

        bool PauseStreams { get; set; }

        /// <summary>
        /// Unique ID for this installation/user. 
        /// </summary>
        string InstallationId { get; }

        ObservableCollection<string> SelectedAccounts { get; }

        bool AutoUrlShorten { get; set; }

        double StyleFontSize { get; set; }
        void SetDefaultShortener(IUrlShortener shortner);

        void Save();

        void Reset();

        #region MahTweets Scripting

        bool ScriptingEnabled { get; set; }

        #endregion
    }
}