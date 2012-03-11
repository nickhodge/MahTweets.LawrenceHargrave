using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using MahTweets.Core.Collections;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;

namespace MahTweets.Services
{
    public class PluginRepository : IPluginRepository
    {
        private readonly IEventAggregator _eventAggregator;

        private readonly ThreadSafeObservableCollection<IMicroblog> _microblogs =
            new ThreadSafeObservableCollection<IMicroblog>();

        private readonly IPluginSettingsProvider _pluginSettingsProvider;

        private readonly Timer _refreshTimer;

        private readonly ThreadSafeObservableCollection<IStatusHandler> _statusHandlers =
            new ThreadSafeObservableCollection<IStatusHandler>();

        private readonly ThreadSafeObservableCollection<IUrlShortener> _urlshorteners =
            new ThreadSafeObservableCollection<IUrlShortener>();

        public PluginRepository(IEventAggregator eventAggregator,
                                IPluginSettingsProvider pluginSettingsProvider)
        {
            _eventAggregator = eventAggregator;
            _pluginSettingsProvider = pluginSettingsProvider;
            _eventAggregator.Subscribe<MicroblogAdded, IMicroblog>(HandleMicroblogAdded);
            _eventAggregator.Subscribe<MicroblogRemoved, IMicroblog>(HandleMicroblogRemoved);
            _eventAggregator.Subscribe<UrlShortenerAdded, IUrlShortener>(HandleUrlShortenerAdded);
            _eventAggregator.Subscribe<StatusHandlerAdded, IStatusHandler>(HandleStatusHandlerAdded);
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }

            _refreshTimer = new Timer();
            // _refreshTimer.Elapsed += (o, args) => _taskService.Enqueue(() => Refresh(false));
            _refreshTimer.Elapsed += (o, args) => Task.Run(() => Refresh(false));
            _refreshTimer.Interval = 1000*60;
            _refreshTimer.Start();
        }

        #region IPluginRepository Members

        public T GetMicroblogByAccount<T>(string accountName) where T : IMicroblog
        {
            return _microblogs.OfType<T>().SingleOrDefault(f => f.Credentials.AccountName.Matches(accountName));
        }

        public IMicroblog GetMicroblogByAccount(string protocol, string accountName)
        {
            return _microblogs.SingleOrDefault(m => protocol.Matches(m.Protocol)
                                                    && accountName.Matches(m.Credentials.AccountName));
        }

        public ObservableCollection<IMicroblog> Microblogs
        {
            get { return _microblogs; }
        }

        public ObservableCollection<IUrlShortener> UrlShorteners
        {
            get { return _urlshorteners; }
        }

        public ObservableCollection<IStatusHandler> StatusHandlers
        {
            get { return _statusHandlers; }
        }

        public void Clear()
        {
            _microblogs.Clear();
        }

        public void Refresh(bool force)
        {
            Task.Run(() => _microblogs.ForEach(a => a.Refresh(force)));
        }

        public void Close()
        {
            Task.Run(() => _microblogs.ForEach(a => a.Disconnect()));
        }

        #endregion

        private void HandleUrlShortenerAdded(IUrlShortener shortener)
        {
            _urlshorteners.Add(shortener);
        }

        private void HandleStatusHandlerAdded(IStatusHandler statusHandler)
        {
            _statusHandlers.Add(statusHandler);
        }

        private void HandleMicroblogAdded(IMicroblog microblog)
        {
            if (_microblogs.Any(m => m.Id.Matches(microblog.Id)))
                return;

            if (microblog is IHaveSettings)
            {
                var foundSettings = (microblog as IHaveSettings);

                if (_pluginSettingsProvider.HasSettingsFor(microblog))
                    foundSettings.LoadSettings(_pluginSettingsProvider);
                else
                    foundSettings.LoadDefaultSettings();

                foundSettings.OnSettingsUpdated();
            }

            Task.Run(() =>
                           {
                               microblog.Connect();
                               microblog.ListenForEvents();
                               microblog.Refresh(true);
                           });

            _microblogs.Add(microblog);
        }

        private void HandleMicroblogRemoved(IMicroblog microblog)
        {
            microblog.Disconnect();
            if (_microblogs.Contains(microblog))
            {
                _microblogs.Remove(microblog);
            }
        }
    }
}