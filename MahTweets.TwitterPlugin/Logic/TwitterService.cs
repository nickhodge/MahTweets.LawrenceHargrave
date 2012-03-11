using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media.Imaging;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Utilities;

namespace MahTweets.TwitterPlugin.Logic
{
    public class TwitterService : IMicroblogSource
    {
        private readonly IApplicationSettingsProvider _applicationSettingsProvider;
        private readonly ISearchProvider _availableSearchProviders;
        private readonly IContactsRepository _contactsRepository;
        private readonly IEventAggregator _eventAggregator;

        private readonly List<Twitter> _list = new List<Twitter>();
        private readonly IStatusUpdateRepository _statusUpdateRepository;

        [ImportingConstructor]
        public TwitterService(
            IEventAggregator eventAggregator,
            IContactsRepository contactsRepository,
            ISearchProvider availableSearchProviders,
            IApplicationSettingsProvider applicationSettingsProvider,
            IStatusUpdateRepository statusUpdateRepository
            )
        {
            _eventAggregator = eventAggregator;
            _statusUpdateRepository = statusUpdateRepository;
            _applicationSettingsProvider = applicationSettingsProvider;
            _availableSearchProviders = availableSearchProviders;
            _contactsRepository = contactsRepository;
        }

        #region IMicroblogSource Members

        public string Name
        {
            get { return "Twitter"; }
        }

        public string Protocol
        {
            get { return "twitter"; }
        }

        public BitmapImage Icon
        {
            get
            {
                return
                    ImageUtility.ConvertResourceToBitmap(
                        "pack://application:,,,/MahTweets.TwitterPlugin;component/Resources/twitter.png");
            }
        }

        public IPlugin Create()
        {
            var microblog = new Twitter(
                _eventAggregator,
                _contactsRepository,
                _statusUpdateRepository,
                _applicationSettingsProvider,
                _availableSearchProviders);
            microblog.Source = this;

            _list.Add(microblog);
            return microblog;
        }

        public void CreateAndDisplayProfile(IContact contact)
        {
            Twitter twitter = _list.First();
            var profile = new TwitterProfileViewModel(twitter, contact, twitter._twitterClient);
            _eventAggregator.GetEvent<ShowProfile>().Publish(profile);
        }

        #endregion
    }
}