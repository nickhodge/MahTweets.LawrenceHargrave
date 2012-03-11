using System;
using System.Collections.Generic;
using System.Linq;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;

namespace MahTweets.Services
{
    public class PluginLoader : IPluginLoader
    {
        private readonly Func<IEnumerable<IMicroblogSource>> _composeMicroblogs;
        private readonly Func<IEnumerable<ISearchProvider>> _composeSearchProviders;
        private readonly Func<IEnumerable<IStatusHandler>> _composeStatusHandler;
        private readonly Func<IEnumerable<IUrlShortener>> _composeUrlShorteners;
        private readonly IPluginSettingsProvider _pluginSettingsProvider;

        public PluginLoader(Func<IEnumerable<ISearchProvider>> composeSearchProviders,
                            Func<IEnumerable<IMicroblogSource>> composeMicroblogs,
                            Func<IPluginSettingsProvider> composePluginSettings,
                            Func<IEnumerable<IUrlShortener>> composeUrlShorteners,
                            Func<IEnumerable<IStatusHandler>> composeStatusHandler)
        {
            _pluginSettingsProvider = composePluginSettings();

            _composeMicroblogs = composeMicroblogs;
            _composeSearchProviders = composeSearchProviders;
            _composeUrlShorteners = composeUrlShorteners;
            _composeStatusHandler = composeStatusHandler;
        }

        private IEnumerable<ISearchProvider> AvailableSearchProviders { get; set; }

        private IEnumerable<IMicroblogSource> AvailableMicroblogs { get; set; }

        private IEnumerable<IUrlShortener> AvailableShorteners { get; set; }

        private IEnumerable<IStatusHandler> AvailableStatusHandlers { get; set; }

        #region IPluginLoader Members

        public bool TryFind(Credential blogCredential, out IMicroblog blog)
        {
            return TryFind(blogCredential, null, out blog);
        }

        public bool TryFind(Credential credential, Action<IMicroblog> onCreate, out IMicroblog blog)
        {
            if (AvailableMicroblogs == null)
                AvailableMicroblogs = _composeMicroblogs();

            blog = null;

            IMicroblogSource provider = AvailableMicroblogs.Any()
                                            ? AvailableMicroblogs.SingleOrDefault(
                                                b => b.Protocol.Matches(credential.Protocol))
                                            : null;
            if (provider == null)
                return false;

            IPlugin newBlog = provider.Create();

            if (newBlog == null)
                return false;

            blog = (IMicroblog) newBlog;

            blog.Credentials = credential;

            if (onCreate != null) onCreate(blog);

            if (blog is IHaveSettings)
            {
                var foundSettings = (blog as IHaveSettings);

                if (_pluginSettingsProvider.HasSettingsFor(blog))
                    foundSettings.LoadSettings(_pluginSettingsProvider);
                else
                    foundSettings.LoadDefaultSettings();

                foundSettings.OnSettingsUpdated();
            }

            return true;
        }

        public void TryFind(IEnumerable<string> searchProviders, out IList<ISearchProvider> providers)
        {
            if (AvailableSearchProviders == null)
                AvailableSearchProviders = _composeSearchProviders();

            providers = new List<ISearchProvider>();

            foreach (string provider in searchProviders)
            {
                string providerName = provider;
                ISearchProvider found = AvailableSearchProviders.SingleOrDefault(s => s.Protocol.Matches(providerName));
                var seacher = CompositionManager.Get<ISearchProvider>(found.GetType());
                ISearchProvider newInstance = seacher;
                if (newInstance != null)
                    providers.Add(newInstance);
            }
        }


        public bool TryFind(Credential credential, out IUrlShortener shortener)
        {
            if (AvailableShorteners == null)
                AvailableShorteners = _composeUrlShorteners();

            shortener = null;

            IUrlShortener newShortener = AvailableShorteners.Any()
                                             ? AvailableShorteners.SingleOrDefault(
                                                 b => b.Protocol.Matches(credential.Protocol))
                                             : null;

            if (newShortener == null)
                return false;

            shortener = CompositionManager.Get<IUrlShortener>(newShortener.GetType());

            if (shortener == null)
                return false;

            shortener.Credentials = credential;

            return true;
        }

        public bool TryFind(Credential credential, out IStatusHandler statusHandler)
        {
            if (AvailableStatusHandlers == null)
                AvailableStatusHandlers = _composeStatusHandler();

            statusHandler = null;

            IStatusHandler newStatusHandler = AvailableStatusHandlers.Any()
                                                  ? AvailableStatusHandlers.SingleOrDefault(
                                                      b => b.Protocol.Matches(credential.Protocol))
                                                  : null;

            if (newStatusHandler == null)
                return false;

            statusHandler = CompositionManager.Get<IStatusHandler>(newStatusHandler.GetType());

            if (statusHandler == null)
                return false;

            statusHandler.Credentials = credential;

            return true;
        }

        #endregion
    }
}