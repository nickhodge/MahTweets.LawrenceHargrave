using System.Windows;
using MahApps.Twitter;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.TwitterPlugin.Logic;

namespace MahTweets.TwitterPlugin.Views
{
    /// <summary>
    /// Interaction logic for FollowEventView.xaml
    /// </summary>
    public partial class FollowEventView
    {
        public FollowEventView(Contact source, Contact target, TwitterClient twitterClient, Twitter tw)
        {
            InitializeComponent();
            Source = source;
            Target = target;

            DataContext = this;

            this.twitterClient = twitterClient;
            t = tw;
            _eventAggregator = CompositionManager.Get<IEventAggregator>();
        }

        public Contact Source { get; set; }
        public Contact Target { get; set; }
        private TwitterClient twitterClient { get; set; }
        private IEventAggregator _eventAggregator { get; set; }
        private Twitter t { get; set; }

        private void btnFollow_Click(object sender, RoutedEventArgs e)
        {
            twitterClient.Friendships.BeginCreate(Source.Name, null);
            _eventAggregator.GetEvent<HideNotification>().Publish(new HideNotificationPayload(this));
        }

        private void btnBlock_Click(object sender, RoutedEventArgs e)
        {
            twitterClient.Block.BeginBlock(Source.Name, null);
            _eventAggregator.GetEvent<HideNotification>().Publish(new HideNotificationPayload(this));
        }

        private void btnViewProfile_Click(object sender, RoutedEventArgs e)
        {
            var profile = new TwitterProfileViewModel(t, Source, twitterClient);
            _eventAggregator.GetEvent<ShowProfile>().Publish(profile);
        }
    }
}