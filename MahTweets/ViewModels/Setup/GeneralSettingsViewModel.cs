using System.Collections.Generic;
using System.Linq;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.ViewModels;

namespace MahTweets.ViewModels.Setup
{
    public class GeneralSettingsViewModel : BaseViewModel
    {
        private readonly IApplicationSettingsProvider _applicationSettingsProvider;
        private readonly IEnumerable<IUrlShortener> _urlShorteners;

        public GeneralSettingsViewModel(
            IApplicationSettingsProvider applicationSettingsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            _applicationSettingsProvider = applicationSettingsProvider;
            _urlShorteners = CompositionManager.GetAll<IUrlShortener>();
        }

        public IEnumerable<IUrlShortener> UrlShorteners
        {
            get { return _urlShorteners; }
        }

        public IUrlShortener SelectedShortener
        {
            get
            {
                if (_applicationSettingsProvider.DefaultShortener == null)
                    _applicationSettingsProvider.SetDefaultShortener(
                        UrlShorteners.Where(t => t.Protocol == "is.gd").FirstOrDefault());

                return _applicationSettingsProvider.DefaultShortener;
            }
            set
            {
                _applicationSettingsProvider.SetDefaultShortener(value);
                RaisePropertyChanged(() => SelectedShortener);
            }
        }

        public bool AutoShorten
        {
            get { return _applicationSettingsProvider.AutoUrlShorten; }
            set
            {
                _applicationSettingsProvider.AutoUrlShorten = value;
                if (value)
                    _applicationSettingsProvider.SetDefaultShortener(SelectedShortener);

                RaisePropertyChanged(() => AutoShorten);
            }
        }

        public bool EnableScripting
        {
            get { return _applicationSettingsProvider.ScriptingEnabled; }
            set
            {
                _applicationSettingsProvider.ScriptingEnabled = value;
                RaisePropertyChanged(() => EnableScripting);
            }
        }

        public bool ExpandShortenedUrls
        {
            get { return _applicationSettingsProvider.AutoExpandUrls; }
            set
            {
                _applicationSettingsProvider.AutoExpandUrls = value;
                RaisePropertyChanged(() => ExpandShortenedUrls);
            }
        }

        public bool DisableMediaProviders
        {
            get { return !_applicationSettingsProvider.DisableMediaProviders; }
            set
            {
                _applicationSettingsProvider.DisableMediaProviders = !value;
                RaisePropertyChanged(() => DisableMediaProviders);
            }
        }

        public bool PauseStreams
        {
            get { return _applicationSettingsProvider.PauseStreams; }
            set
            {
                _applicationSettingsProvider.PauseStreams = value;
                RaisePropertyChanged(() => PauseStreams);
            }
        }
    }
}