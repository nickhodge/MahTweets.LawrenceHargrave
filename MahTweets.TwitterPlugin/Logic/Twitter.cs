using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using Hammock;
using MahApps.RESTBase;
using MahApps.Twitter;
using MahApps.Twitter.Models;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Location;
using MahTweets.Core.Media.Attachments;
using MahTweets.Core.Settings;
using MahTweets.Core.Utilities;
using MahTweets.TwitterPlugin.UI;
using MahTweets.TwitterPlugin.Views;
using MAT = MahApps.Twitter;
using Tweet = MahApps.Twitter.Models.Tweet;

namespace MahTweets.TwitterPlugin.Logic
{
    [DataContract]
    [SettingsClass(typeof (TwitterSettings))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class Twitter : Notify, IMicroblog, IHaveSettings, IObservable<IStatusUpdate>
    {
        //Private properties
        // Original MahTweets 3.5 oauth codes
        //private const string ConsumerKey = "26ClesNK3zAiTi8zfBlz8w";
        //private const string ConsumerSecret = "JcG1iTyXBfJamFHyOtzPKs4XVd3FNLqnFIolmKMWwU";

        // new LawrenceHargrave oauth launch codes
        private const string ConsumerKey = "b8qsK6pFUPNZzdu5FxfxVg";
        private const string ConsumerSecret = "mYO5CysNHvFQ0pPO7y7Fwj7LY1KsLlxha794FXp7qM";


        //Arbitary values so that it works (0 didn't work, 900million is too large for DMs)
        private const Int64 _highestTweetId = 900000000;
        private const Int64 _highestDmIdSent = 100;

        public const string Ellipsis = "\u2026";
                            // unicode for ellipsis or ... (three dots as a single character, not comment to be continued silly)

        private readonly IContactsRepository _contactsRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly List<UpdateType> _listTypes = new List<UpdateType>();
        private readonly IStatusUpdateRepository _statusUpdateManager;

        private Timer _directMsgTimer;
        private bool _disconnectSignalled;
        private bool _fetchingOwner;
        public Int64 _highestDmId = 100;
        private Timer _listsTimer;
        private Timer _mentionTimer;
        private TwitterContact _owner;
        private List<UpdateType> _supportedTypes;
        private Timer _timelineTimer;
        public TwitterClient _twitterClient = new TwitterClient(ConsumerKey, ConsumerSecret, null);

        private IAsyncResult streamingStatus;

        [ImportingConstructor]
        public Twitter(IEventAggregator eventAggregator,
                       IContactsRepository contactsRepository,
                       IStatusUpdateRepository statusUpdateRepository,
                       IApplicationSettingsProvider applicationSettingsProvider,
                       ISearchProvider availableSearchProviders
            // NOTE: MEF will implode once a second search provider is added
            )
        {
            AppSettings = applicationSettingsProvider;
            _eventAggregator = eventAggregator;
            _statusUpdateManager = statusUpdateRepository;
            // _availableSearchProviders = new List<ISearchProvider> { availableSearchProviders };
            _contactsRepository = contactsRepository;
            HourlyAPILimit = 300;
        }

        public IApplicationSettingsProvider AppSettings { get; private set; }

        public int SubscribedListCount { get; private set; }
        public int MyListCount { get; private set; }
        public int HourlyAPILimit { get; private set; }

        public bool IsStreaming
        {
            get { return UserStreams; }
            set
            {
                UserStreams = value;
                RaisePropertyChanged(() => IsStreaming);
            }
        }

        public static List<Twitter> CurrentAccounts { get; set; }

        #region IHaveSettings Members

        public void LoadDefaultSettings()
        {
            GeoTag = false;
            DirectMessageFrequency = 10;
            MentionFrequency = 10;
            TimelineFrequency = 30;
            ListsFrequency = 10;
            TweetsPerCall = 100;
            UserStreams = true;
        }

        public void LoadSettings(IPluginSettingsProvider provider)
        {
            DirectMessageFrequency = provider.Get<int>(this, TwitterHelper.DIRECTMSG_SETTING);
            MentionFrequency = provider.Get<int>(this, TwitterHelper.MENTION_SETTING);
            TimelineFrequency = provider.Get<int>(this, TwitterHelper.TIMELINE_SETTING);
            ListsFrequency = provider.Get<int>(this, TwitterHelper.LISTS_SETTING);
            TweetsPerCall = provider.Get<int>(this, TwitterHelper.TWEETSPERCALL_SETTING);
            GeoTag = provider.Get<bool>(this, TwitterHelper.GEOTAG_SETTING);
            UserStreams = provider.Get<bool>(this, TwitterHelper.USERSTREAM_SETTING);
        }

        public void OnSettingsUpdated()
        {
            if (UserStreams)
            {
                ConnectStream();
            }
            else
            {
                UpdateTimer(ref _timelineTimer, TriggerTimelineUpdate, TimelineFrequency);
                UpdateTimer(ref _mentionTimer, TriggerMentionsUpdate, MentionFrequency);
                UpdateTimer(ref _directMsgTimer, TriggerDirectMessageUpdate, DirectMessageFrequency);
                UpdateTimer(ref _listsTimer, TriggerListsUpdate, ListsFrequency);
            }
        }

        #endregion

        #region IMicroblog Members

        public bool HasSettings
        {
            get { return true; }
        }

        public void ShowSettings()
        {
            var settings = new TwitterSettings();
            settings.Load(this);
            settings.btnSave.Click += (s, e) => settings.Save(this);
            _eventAggregator.Show(settings);
        }

        public string Id
        {
            get
            {
                if (Credentials == null)
                    return "Twitter - New";

                return "Twitter: " + Credentials.AccountName;
            }
        }

        public void Setup()
        {
            _twitterClient.BeginGetRequestUrl((req, res, url) => Process.Start(url));

            var view = new PinVerifierView();
            view.Closed += (s, e) => CheckResult(view);

            _eventAggregator.Show(view);
        }

        public void Connect()
        {
            if (CurrentAccounts == null)
                CurrentAccounts = new List<Twitter>();
            CurrentAccounts.Add(this);

            _twitterClient.SetOAuthToken(new Credentials
                                             {
                                                 OAuthToken = Credentials.Username,
                                                 OAuthTokenSecret = Credentials.Password
                                             });

            ConnectStream();
        }

        public void Disconnect()
        {
            _disconnectSignalled = true;

            UpdateTimer(ref _timelineTimer, TriggerTimelineUpdate, 0);
            UpdateTimer(ref _mentionTimer, TriggerMentionsUpdate, 0);
            UpdateTimer(ref _directMsgTimer, TriggerDirectMessageUpdate, 0);
            UpdateTimer(ref _listsTimer, TriggerListsUpdate, 0);
        }

        public void Refresh(bool isRequired)
        {
            if (_disconnectSignalled)
                return;

            if (isRequired)
            {
                UpdateTimeline();
                UpdateMentions();
                UpdateDirectMessages();
                UpdateLists();
            }
        }

        public void NewStatusUpdate(string text)
        {
            ConvertAndSendTweets(text, Tweet);
        }

        public void UpdateAvatar(string image)
        {
            try
            {
                //var response = StartOAuthRequest().Account()
                //    .UpdateProfileImage(image).AsXml().Request();

                //Contact self = ContactsRepository.GetOrCreate(this.Credentials.AccountName,this);
                //self.SetContactImage(new Uri(image));
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        public Credential Credentials { get; set; }

        public bool HandleEvent(string eventName, IStatusUpdate update, params object[] obj)
        {
            if (eventName == "profileByName")
            {
                var c = _contactsRepository.GetOrCreate<TwitterContact>((string) obj[0], Source);
                var profile = new TwitterProfileViewModel(this, c, _twitterClient);
                _eventAggregator.GetEvent<ShowProfile>().Publish(profile);

                return true;
            }

            if (eventName == "searchHashtag")
            {
                TwitterSearchClassic search = GetSearchProvider((string) obj[0]);
                _eventAggregator.GetEvent<ShowSearch>().Publish(search);
                return true;
            }

            if (update == null) return false;

            var tweet = (UI.Tweet) update;
            string convertedName = eventName.ToLower();

            switch (convertedName)
            {
                case "linkclicked":
                    if (obj.Length == 0)
                        break;

                    var url = obj[0] as string;
                    if (string.IsNullOrEmpty(url))
                        break;

                    _eventAggregator.GetEvent<ShowLink>().Publish(new Link(url));

                    break;

                case "reply":
                    if (obj.Length == 0)
                        break;

                    var replyId = obj[0] as string;
                    if (string.IsNullOrEmpty(replyId))
                        break;

                    if (replyId == String.Format("RT @{0} {1}", tweet.Contact.Name, tweet.Text))
                        Retweet(tweet);
                    else
                        Reply(tweet, replyId);

                    break;

                case "loadconversation":
                    LoadConversation(tweet.ID);
                    break;

                case "favourite":
                    try
                    {
                        if (tweet.Favourite)
                        {
                            _twitterClient.Favourites.BeginDestroy(tweet.ID, null);
                            tweet.Favourite = true;
                        }
                        else
                        {
                            _twitterClient.Favourites.BeginCreate(tweet.ID, null);
                            tweet.Favourite = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                    break;

                case "block":
                    _twitterClient.Block.BeginBlock(tweet.Contact.Name, null);
                    MessageBox.Show(tweet.Contact.Name + " is now blocked.");
                    break;

                case "spam":
                    _twitterClient.Block.BeginSpamBlock(tweet.Contact.Name, null);
                    MessageBox.Show(tweet.Contact.Name + " is now blocked and reported as a spammer.");
                    break;

                case "follow":
                    _twitterClient.Friendships.BeginCreate(tweet.Contact.Name, null);
                    break;

                case "drag":
                    if (obj.Length == 0)
                        break;

                    //var payload = obj[0] as DragPayload;
                    //if (payload == null)
                    //    break;

                    //if (DragRequested != null)
                    //    DragRequested(payload);
                    break;

                case "unfollow":
                    _twitterClient.Friendships.BeginDestroy(tweet.Contact.Name, null);
                    break;

                case "profile":
                    var profile = new TwitterProfileViewModel(this, tweet.Contact, _twitterClient);
                    _eventAggregator.GetEvent<ShowProfile>().Publish(profile);

                    break;

                case "sidebar_favourite":

                    break;
            }
            return true;
        }

        void IMicroblog.ListenForEvents()
        {
            PluginEventHandler.RegisterEvent(this, "retweet");
            PluginEventHandler.RegisterEvent(this, "reply");
            PluginEventHandler.RegisterEvent(this, "favourite");
            PluginEventHandler.RegisterEvent(this, "loadconversation");
            PluginEventHandler.RegisterEvent(this, "follow");
            PluginEventHandler.RegisterEvent(this, "unfollow");
            PluginEventHandler.RegisterEvent(this, "block");
            PluginEventHandler.RegisterEvent(this, "linkclicked");
            PluginEventHandler.RegisterEvent(this, "drag");
            PluginEventHandler.RegisterEvent(this, "spam");
            PluginEventHandler.RegisterEvent(this, "profile");
            PluginEventHandler.RegisterEvent(this, "profileByName");
            PluginEventHandler.RegisterEvent(this, "searchHashtag");
            PluginEventHandler.RegisterEvent(this, "sidebar_favourite");
        }

        //private List<ISearchProvider> _searchProviders;
        //public IList<ISearchProvider> SearchProviders
        //{
        //    get
        //    {
        //        if (_searchProviders == null)
        //        {
        //            _searchProviders = new List<ISearchProvider>();
        //            var search = CompositionManager.Get<TwitterSearchClassic>(typeof(TwitterSearchClassic));
        //            search.Parent = this;
        //            _searchProviders.Add(search);
        //            //_searchProviders.Add(new StreamingSearchProvider(this));
        //        }
        //        return _searchProviders;
        //    }
        //}

        public IList<UpdateType> SupportedTypes
        {
            get
            {
                if (_supportedTypes == null)
                {
                    _supportedTypes = new List<UpdateType>();
                    _supportedTypes.AddUpdate<NormalUpdate>(this);
                    _supportedTypes.AddUpdate<SelfUpdate>(this);
                    _supportedTypes.AddUpdate<RetweetUpdate>(this);
                    _supportedTypes.AddUpdate<MentionUpdate>(this);
                    _supportedTypes.AddUpdate<DirectMessageUpdate>(this);
                    _supportedTypes.AddUpdate<SelfMessageUpdate>(this);
                    if (_listTypes != null)
                    {
                        _supportedTypes.AddRange(_listTypes);
                    }
                }

                return _supportedTypes.Distinct().ToList();
            }
        }

        public IMicroblogSource Source { get; set; }

        public bool ReadOnly
        {
            get { return false; }
        }

        public string Name
        {
            get { return "Twitter"; }
        }

        public string Protocol
        {
            get { return Name.ToLower(); }
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

        public IContact Owner
        {
            get
            {
                if (_owner == null && !_fetchingOwner)
                {
                    // TODO:  This whole GET is a race condition. 
                    _owner = _contactsRepository.GetOrCreate<TwitterContact>(Credentials.AccountName, Source);
                    try
                    {
                        _fetchingOwner = true;
                        _twitterClient.Account.BeginVerifyCredentials((req, res, obj) =>
                                                                          {
                                                                              var user = obj as User;
                                                                              if (user != null)
                                                                              {
                                                                                  _owner =
                                                                                      _contactsRepository.GetOrCreate
                                                                                          <TwitterContact>(
                                                                                              user.ScreenName, Source);
                                                                                  if (_owner == null)
                                                                                  {
                                                                                      return;
                                                                                  }
                                                                                  if (_owner.ImageUrl !=
                                                                                      new Uri(user.ProfileImageUrl))
                                                                                      _owner.SetContactImage(
                                                                                          new Uri(user.ProfileImageUrl),
                                                                                          null);
                                                                                  _owner.Bio = user.Description;
                                                                                  _owner.Following = user.FriendsCount;
                                                                                  _owner.Followers = user.FollowersCount;
                                                                                  _owner.FullName = user.Name;
                                                                                  _fetchingOwner = false;
                                                                                  RaisePropertyChanged(() => Owner);
                                                                              }
                                                                          });
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                        _owner = _contactsRepository.GetOrCreate<TwitterContact>(Credentials.AccountName, Source);
                    }
                }

                return _owner;
            }
        }

        #endregion

        #region Custom Public Properties

        //private bool _isConnected;

        //public bool IsConnected
        //{
        //    get { return _isConnected; }
        //    set
        //    {
        //        if (_isConnected != value)
        //        {
        //            _isConnected = value;
        //            OnPropertyChanged("IsConnected");
        //            OnPropertyChanged("Message");
        //        }
        //    }
        //}

        //private bool _isPermitted;

        //public bool IsPermitted
        //{
        //    get { return _isPermitted; }
        //    set
        //    {
        //        if (_isPermitted != value)
        //        {
        //            _isPermitted = value;
        //            OnPropertyChanged("IsPermitted");
        //            OnPropertyChanged("Message");
        //        }
        //    }
        //}

        //public string Message
        //{
        //    get
        //    {
        //        if (!IsConnected)
        //            return "Net";
        //        if (!IsPermitted)
        //            return "API";
        //        return "OK";
        //    }
        //}

        //private int _remaining;

        //public int Remaining
        //{
        //    get { return _remaining; }
        //    set
        //    {
        //        _remaining = value;
        //        OnPropertyChanged("Remaining");
        //    }
        //}

        //private DateTime _resetTime;

        //public DateTime ResetTime
        //{
        //    get { return _resetTime; }
        //    set
        //    {
        //        if (_resetTime != value)
        //        {
        //            _resetTime = value;
        //            OnPropertyChanged("ResetTime");
        //        }
        //    }
        //}

        #endregion

        #region Plugin Configuration Settings

        public bool GeoTag { get; set; }
        public int TimelineFrequency { get; set; }
        public int DirectMessageFrequency { get; set; }
        public int MentionFrequency { get; set; }
        public int ListsFrequency { get; set; }
        public int TweetsPerCall { get; set; }
        public bool UserStreams { get; set; }

        #endregion

        #region Checking Twitter Service

        private readonly Dictionary<long, long> _listActivity = new Dictionary<long, long>();

        private void TriggerTimelineUpdate(object sender, ElapsedEventArgs args)
        {
            UpdateTimeline();
        }

        private void TriggerMentionsUpdate(object sender, ElapsedEventArgs args)
        {
            UpdateMentions();
        }

        private void TriggerDirectMessageUpdate(object sender, ElapsedEventArgs args)
        {
            UpdateDirectMessages();
        }

        private void TriggerListsUpdate(object sender, ElapsedEventArgs args)
        {
            UpdateLists();
        }

        private void UpdateTimeline()
        {
            try
            {
                _twitterClient.Statuses.BeginHomeTimeline(_highestTweetId, null, TweetsPerCall, null, false, true,
                                                          NewTweetCallback);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }


        private void UpdateMentions()
        {
            try
            {
                _twitterClient.Statuses.BeginMentions(_highestTweetId, null, TweetsPerCall, null, false, false,
                                                      NewTweetCallback);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void UpdateDirectMessages()
        {
            try
            {
                _twitterClient.DirectMessages.BeginDirectMessages(_highestDmId, null, TweetsPerCall, null, false, false,
                                                                  NewTweetCallback);
                _twitterClient.DirectMessages.BeginSentDirectMessages(_highestDmIdSent, null, TweetsPerCall, null, false,
                                                                      false, NewTweetCallback);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void UpdateLists()
        {
            try
            {
                _twitterClient.Lists.BeginGetSubscriptions(Credentials.AccountName.ToLower(), UpdateListCallback);
                _twitterClient.Lists.BeginGetUserLists(Credentials.AccountName.ToLower(), UpdateListCallback);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void UpdateListCallback(RestRequest request, RestResponse response, object Response)
        {
            var lists = Response as List<TwitterList>;
            if (lists != null)
            {
                if (!lists.Any()) return;
                try
                {
                    //_listTypes = new List<UpdateType>();

                    // Logging.Info("{0} lists recieved", lists.Count());
                    foreach (TwitterList list in lists)
                    {
                        long mostRecent = MaximumForList(list);

                        // bugfix - which counter to update here?
                        if (list.Owner.ScreenName.Matches(Credentials.AccountName))
                            MyListCount = lists.Count();
                        else
                            SubscribedListCount = lists.Count();

                        TwitterList list1 = list;
                        _twitterClient.Lists.BeginGetList(list.Owner.ScreenName, list.Id.ToString(), mostRecent, null,
                                                          TweetsPerCall, null, false,
                                                          (req, res, obj) =>
                                                              {
                                                                  if (obj is List<Tweet>)
                                                                  {
                                                                      var tweets = (List<Tweet>) obj;
                                                                      UpdateListTweetsCallback(list1, tweets);
                                                                  }
                                                              });
                    }


                    _supportedTypes = null;
                    RaisePropertyChanged(() => SupportedTypes);
                }
                catch (Exception ex)
                {
                    CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                }
            }
        }

        private void UpdateListTweetsCallback(TwitterList list, List<Tweet> tweets)
        {
            try
            {
                var type = new ListUpdate(list.FullName);
                if (!_listTypes.Any(t => t.Type.Matches(list.FullName)))
                {
                    _listTypes.Add(type);

                    _supportedTypes = null;
                    RaisePropertyChanged(() => SupportedTypes);
                }

                if (tweets == null || !tweets.Any()) return;

                // Logging.Info("{0} tweets recieved", tweets.Count());
                IEnumerable<UI.Tweet> convertToTweets = tweets.Select(CreateTweet)
                    .Select(x => TwitterHelper.ConvertToList(x, this, type));

                long? max = tweets.Select(x => x.Id).Max();
                if (max != null)
                    SetMaximumForList(list, (long) max);

                AddRangeToStatusManager(convertToTweets.ToList());
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void AddRangeToStatusManager(IEnumerable<UI.Tweet> updates)
        {
            _statusUpdateManager.Send(updates);
        }


        private long MaximumForList(TwitterList list)
        {
            return _listActivity.ContainsKey(list.Id) ? _listActivity[list.Id] : 0;
        }

        private void SetMaximumForList(TwitterList list, long maximum)
        {
            if (_listActivity.ContainsKey(list.Id))
                _listActivity[list.Id] = maximum;
            else
                _listActivity.Add(list.Id, maximum);
        }

        #endregion

        #region IObservable<IStatusUpdate> Members

        public IDisposable Subscribe(IObserver<IStatusUpdate> observer)
        {
            // TODO: as new updates are generated by the manager
            // these are subscribed to by the observers
            return null;
        }

        #endregion

        private void CheckResult(PinVerifierView view)
        {
            if (view.ModalResult == true)
            {
                _twitterClient.BeginGetAccessToken(view.PIN, (req, res, cred) =>
                                                                 {
                                                                     //TODO: check this actually returns null on invalid pin etc
                                                                     if (cred == null)
                                                                         _eventAggregator.PublishMessage(
                                                                             "Invalid authentication token received from Twitter");

                                                                     else if (
                                                                         !string.IsNullOrWhiteSpace(cred.OAuthToken) &&
                                                                         !string.IsNullOrWhiteSpace(
                                                                             cred.OAuthTokenSecret))
                                                                     {
                                                                         _twitterClient.SetOAuthToken(cred);
                                                                         _twitterClient.Account.
                                                                             BeginVerifyCredentials(
                                                                                 (ireq, ires, obj) =>
                                                                                     {
                                                                                         var user = (User) obj;
                                                                                         Credentials = new Credential
                                                                                                           {
                                                                                                               Username
                                                                                                                   =
                                                                                                                   cred
                                                                                                                   .
                                                                                                                   OAuthToken,
                                                                                                               Password
                                                                                                                   =
                                                                                                                   cred
                                                                                                                   .
                                                                                                                   OAuthTokenSecret,
                                                                                                               Protocol
                                                                                                                   =
                                                                                                                   "twitter",
                                                                                                               AccountName
                                                                                                                   =
                                                                                                                   user
                                                                                                                   .
                                                                                                                   ScreenName
                                                                                                           };

                                                                                         _eventAggregator.GetEvent
                                                                                             <MicroblogAdded>().
                                                                                             Publish(this);
                                                                                     });
                                                                     }
                                                                     else
                                                                         _eventAggregator.PublishMessage(
                                                                             "An Error occurred");
                                                                 });
            }
            else
                _eventAggregator.PublishMessage("An Error occurred");
        }

        private void TwitterStreamingStreamingReconnectAttemptEvent()
        {
            UpdateTimeline();
            UpdateMentions();
            UpdateDirectMessages();
            UpdateLists();
        }

        private void NewTweetCallback(RestRequest request, RestResponse response, object DeserialisedResponse)
        {
            if (response.Headers != null && response.Headers.HasKeys() &&
                response.Headers.AllKeys.Contains("X-RateLimit-Limit"))
            {
                try
                {
                    HourlyAPILimit = int.Parse(response.Headers["X-RateLimit-Limit"]);
                }
                catch (Exception ex)
                {
                    CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                }
            }
            if (DeserialisedResponse is List<DirectMessage>)
            {
                foreach (DirectMessage t in (List<DirectMessage>) DeserialisedResponse)
                    StreamCallback(request, response, t);
            }

            else if (DeserialisedResponse is List<Tweet>)
            {
                foreach (Tweet t in (List<Tweet>) DeserialisedResponse)
                    StreamCallback(request, response, t);
            }
        }

        private void StreamCallback(RestRequest request, RestResponse response, ITwitterResponse DeserialisedResponse)
        {
            try
            {
                if (DeserialisedResponse is DirectMessage)
                {
                    var dm = DeserialisedResponse as DirectMessage;
                    var contact = _contactsRepository.GetOrCreate<TwitterContact>(dm.Sender.ScreenName, Source);
                    contact.Bio = dm.Sender.Description;
                    contact.Url = dm.Sender.Url;
                    contact.IsProtected = dm.Sender.Protected;
                    contact.Followed = true;
                    contact.FullName = dm.Sender.Name;

                    if (contact.ImageUrl != new Uri(dm.Sender.ProfileImageUrl))
                        contact.SetContactImage(new Uri(dm.Sender.ProfileImageUrl), dm.Created);

                    var recipient = _contactsRepository.GetOrCreate<TwitterContact>(dm.Recipient.ScreenName, Source);
                    recipient.Bio = dm.Recipient.Description;

                    recipient.Followed = true;
                    recipient.FullName = dm.Recipient.Name;

                    if (recipient.ImageUrl != new Uri(dm.Recipient.ProfileImageUrl))
                        recipient.SetContactImage(new Uri(dm.Recipient.ProfileImageUrl), dm.Created);

                    var t = new DirectMessageTweet
                                {
                                    ID = dm.Id.ToString(),
                                    Contact = contact,
                                    Text = WebUtility.HtmlDecode(dm.Text),
                                    Time = dm.Created.ToLocalTime(),
                                    Recipient = recipient
                                };


                    t.AddParent(this);
                    UI.Tweet tweet = TwitterHelper.ConvertToDM(t, this);
                    Send(tweet);
                }
                else if (DeserialisedResponse is StreamEvent)
                {
                    var twitterEvent = DeserialisedResponse as StreamEvent;
                    switch (twitterEvent.Event)
                    {
                        case "follow":
                            var s = _contactsRepository.GetOrCreate<TwitterContact>(twitterEvent.SourceUser.ScreenName,
                                                                                    Source);
                            var t = _contactsRepository.GetOrCreate<TwitterContact>(twitterEvent.TargetUser.ScreenName,
                                                                                    Source);

                            if (s != Owner)
                                _eventAggregator.GetEvent<ShowNotification>().Publish(
                                    new ShowNotificationPayload(new FollowEventView(s, t, _twitterClient, this),
                                                                TimeSpan.Zero, NotificactionLevel.Information));

                            break;
                    }
                }
                else if (DeserialisedResponse is Tweet)
                {
                    var original = (Tweet) DeserialisedResponse;
                    var s = original.RetweetedStatus ?? original;

                    if (s.User == null)
                    {
                        return; // TODO: What should happen if user is empty/null? 
                    }

                    var contact = _contactsRepository.GetOrCreate<TwitterContact>(s.User.ScreenName, Source);

                    if (contact.ImageUrl != new Uri(s.User.ProfileImageUrl))
                        contact.SetContactImage(new Uri(s.User.ProfileImageUrl), s.Created);
                    contact.Bio = s.User.Description;
                    contact.Url = s.User.Url;
                    contact.IsProtected = s.User.Protected;
                    contact.FullName = s.User.Name;

                    var t = new UI.Tweet
                                {
                                    ID = s.Id.ToString(),
                                    Contact = contact,
                                    Text = WebUtility.HtmlDecode(s.Text),
                                    Time = s.Created.ToLocalTime(),
                                    SourceUri = s.Source.GetSourceURL(),
                                    Source = s.Source.GetSourceText(),
                                    Favourite = s.Favourited,
                                    Microblog = this,
                                };

                    if (original.Entities != null)
                    {
                        if (original.Entities.Urls != null)
                        {
                            foreach (Url u in original.Entities.Urls.Where(u => u.ExpandedUrl != null))
                            {
                                t.Text = t.Text.Replace(u.Link, u.ExpandedUrl);
                            }
                        }
                        if (original.Entities.Media != null)
                        {
                            foreach (Media m in original.Entities.Media)
                            {
                                // Logging.Info("Found something: " + m.MediaURL);
                                t.Attachments.Add(new ImageMinimisedAttachment(m.MediaURL));
                            }
                        }
                    }

                    if (original.RetweetedStatus != null)
                        t.RetweetBy = _contactsRepository.GetOrCreate<TwitterContact>(original.User.ScreenName, Source);

                    if (s.Geo != null)
                        t.Location = new GeoLocation(s.Geo.Coordinates[0], s.Geo.Coordinates[1]);

                    if (s.InReplyToStatusId > 0)
                    {
                        t.InReplyToID = s.InReplyToStatusId.ToString();
                        t.InReplyTo = _contactsRepository.GetOrCreate<TwitterContact>(s.InReplyToScreenName, Source);
                    }

                    t.AddParent(this);
                    TwitterHelper.ConvertToTimeline(t, this);

                    Send(t);
                }
                else
                {
                    Logging.Info("Unknown Stream Response Type: {0} : {1}", DeserialisedResponse.GetType().FullName,
                                 DeserialisedResponse.ToString());
                }
            }

            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void Send<T>(T t) where T : StatusUpdate
        {
            foreach (Twitter account in CurrentAccounts.Where(a => a != this))
            {
                if (t.Text.ToUpper().Contains("@" + account.Credentials.AccountName.ToUpper()))
                {
                    t.Types.AddUpdate<MentionUpdate>(account);
                    t.AddParent(account);
                }
            }


            _statusUpdateManager.Send(t);
        }

        public HttpWebRequest DelegatedQuery(string p, Format format)
        {
            return (HttpWebRequest) _twitterClient.DelegatedRequest(p, format);
        }

        private static void UpdateTimer(ref Timer timer, ElapsedEventHandler handler, int frequency)
        {
            if (frequency == 0)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Elapsed -= handler;
                    timer = null;
                }
            }
            else
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Elapsed -= handler;
                }

                timer = new Timer();
                timer.Elapsed += handler;
                timer.Interval = Convert.ToDouble(3600000/frequency);
                timer.Start();
            }
        }

        public void Search(string term)
        {
            TwitterSearchClassic search = GetSearchProvider(term);
            _eventAggregator.GetEvent<ShowSearch>().Publish(search);
        }

        // TODO: clean up how search providers are declared and consumed
        private TwitterSearchClassic GetSearchProvider(string term)
        {
            var pluginRepository = CompositionManager.Get<IPluginRepository>();
            var search =
                Activator.CreateInstance(typeof (TwitterSearchClassic), _contactsRepository, pluginRepository) as
                TwitterSearchClassic;
            if (search != null)
            {
                search.Parent = this;
                search.AddSearchTerm(term);
                return search;
            }
            return null;
        }

        private void Retweet(UI.Tweet t)
        {
            // Tweet t1;
            _twitterClient.Statuses.BeginRetweet(t.ID, (req, res, obj) =>
                                                           {
                                                               t.Types.AddUpdate<RetweetUpdate>(this);
                                                               t.RetweetBy = Owner;
                                                               Send(t);
                                                           });
        }

        private bool Tweet(string text, UI.Tweet reply)
        {
            try
            {
                if (reply.Types.HasType<DirectMessageUpdate>())
                {
                    string contents;
                    TwitterHelper.GetTweetPrefix(text, out contents);
                    _twitterClient.DirectMessages.BeginCreate(reply.Contact.Name, contents, (res, req, obj) =>
                                                                                                {
                                                                                                    DirectMessageTweet
                                                                                                        t1 =
                                                                                                            CreateDirectMessage
                                                                                                                ((
                                                                                                                 DirectMessage
                                                                                                                 ) obj);
                                                                                                    t1.Types.AddUpdate
                                                                                                        <
                                                                                                            DirectMessageUpdate
                                                                                                            >(this);
                                                                                                    t1.Types.AddUpdate
                                                                                                        <
                                                                                                            SelfMessageUpdate
                                                                                                            >(this);
                                                                                                    t1.Types.AddUpdate
                                                                                                        <SelfUpdate>(
                                                                                                            this);
                                                                                                    Send(t1);
                                                                                                });
                }
                else
                {
                    if (GeoTag)
                    {
                        GeoLocation loc = GlobalPosition.GetLocation();

                        if (GeoTag && loc != null)
                        {
                            _twitterClient.Statuses.BeginUpdate(text, reply.ID, loc.Latitude, loc.Longitude,
                                                                (res, req, obj) =>
                                                                    {
                                                                        if (!(obj is Tweet))
                                                                            return;

                                                                        UI.Tweet t1 = CreateTweet((Tweet) obj);
                                                                        t1.Types.AddUpdate<NormalUpdate>(this);
                                                                        t1.Types.AddUpdate<SelfUpdate>(this);
                                                                        Send(t1);
                                                                    });
                        }
                            //if geo fails..
                        else
                            _twitterClient.Statuses.BeginUpdate(text, reply.ID, (res, req, obj) =>
                                                                                    {
                                                                                        if (!(obj is Tweet))
                                                                                            return;

                                                                                        UI.Tweet t1 =
                                                                                            CreateTweet((Tweet) obj);
                                                                                        t1.Types.AddUpdate<NormalUpdate>
                                                                                            (this);
                                                                                        t1.Types.AddUpdate<SelfUpdate>(
                                                                                            this);
                                                                                        Send(t1);
                                                                                    });
                    }
                    else
                        _twitterClient.Statuses.BeginUpdate(text, reply.ID, (res, req, obj) =>
                                                                                {
                                                                                    if (!(obj is Tweet))
                                                                                        return;

                                                                                    UI.Tweet t1 =
                                                                                        CreateTweet((Tweet) obj);
                                                                                    t1.Types.AddUpdate<NormalUpdate>(
                                                                                        this);
                                                                                    t1.Types.AddUpdate<SelfUpdate>(this);
                                                                                    Send(t1);
                                                                                });
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                // Logging.Fail("Tweet (text,reply) failed with: ", ex);
                return false;
            }

            // Logging.Important("Tweet (text,reply) completed at: " + Environment.TickCount);
            return true;
        }

        private bool Tweet(string text)
        {
            try
            {
                if (text.StartsWith(TwitterHelper.DIRECTMESSAGE_PREFIX))
                {
//TODO: Restore DM replying
                    string contents;
                    string prefix = TwitterHelper.GetTweetPrefix(text, out contents);
                    string name = prefix.Substring(1).Trim();
                    _twitterClient.DirectMessages.BeginCreate(name, contents, (res, req, obj) =>
                                                                                  {
                                                                                      DirectMessageTweet t1 =
                                                                                          CreateDirectMessage(
                                                                                              (DirectMessage) obj);
                                                                                      t1.Types.AddUpdate
                                                                                          <DirectMessageUpdate>(this);
                                                                                      t1.Types.AddUpdate
                                                                                          <SelfMessageUpdate>(this);
                                                                                      t1.Types.AddUpdate<SelfUpdate>(
                                                                                          this);
                                                                                      Send(t1);
                                                                                  });
                }
                else
                {
                    GeoLocation loc = GlobalPosition.GetLocation();

                    UI.Tweet t;
                    if (GeoTag && loc != null)
                    {
                        _twitterClient.Statuses.BeginUpdate(text, null, loc.Latitude, loc.Longitude,
                                                            (res, req, obj) =>
                                                                {
                                                                    var response = obj as Tweet;
                                                                    if (response != null)
                                                                    {
                                                                        t = CreateTweet(response);
                                                                        t.Types.AddUpdate<NormalUpdate>(this);
                                                                        t.Types.AddUpdate<SelfUpdate>(this);
                                                                        Send(t);
                                                                    }
                                                                });
                    }
                    else
                        _twitterClient.Statuses.BeginUpdate(text, null, null, null,
                                                            (res, req, obj) =>
                                                                {
                                                                    var response = obj as Tweet;
                                                                    if (response != null)
                                                                    {
                                                                        t = CreateTweet(response);
                                                                        t.Types.AddUpdate<NormalUpdate>(this);
                                                                        t.Types.AddUpdate<SelfUpdate>(this);
                                                                        Send(t);
                                                                    }
                                                                });
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                // Logging.Fail("Tweet (text) failed with: ", ex);
                return false;
            }

            // Logging.Important("Tweet (text) completed at: " + Environment.TickCount);
            return true;
        }

        private UI.Tweet CreateTweet(Tweet s)
        {
            var contact = _contactsRepository.GetOrCreate<TwitterContact>(s.User.ScreenName, Source);
            return TwitterHelper.CreateTweet(s, contact, this);
        }

        private DirectMessageTweet CreateDirectMessage(DirectMessage dm)
        {
            var contact = _contactsRepository.GetOrCreate<TwitterContact>(dm.Sender.ScreenName, Source);
            contact.Bio = dm.Sender.Description;
            contact.Following = dm.Sender.FriendsCount;
            contact.Followers = dm.Sender.FollowersCount;
            contact.Followed = true;
            contact.FullName = dm.Sender.Name;

            if (contact.ImageUrl != new Uri(dm.Sender.ProfileImageUrl))
                contact.SetContactImage(new Uri(dm.Sender.ProfileImageUrl), dm.Created);

            var recipient = _contactsRepository.GetOrCreate<TwitterContact>(dm.Recipient.ScreenName, Source);
            recipient.Bio = dm.Recipient.Description;
            recipient.Following = dm.Recipient.FriendsCount;
            recipient.Followers = dm.Recipient.FollowersCount;
            recipient.Followed = true;
            recipient.FullName = dm.Recipient.Name;

            if (recipient.ImageUrl != new Uri(dm.Recipient.ProfileImageUrl))
                recipient.SetContactImage(new Uri(dm.Recipient.ProfileImageUrl), dm.Created);

            var t = new DirectMessageTweet
                        {
                            ID = dm.Id.ToString(),
                            Contact = contact,
                            Text = WebUtility.HtmlDecode(dm.Text),
                            Time = dm.Created.ToLocalTime(),
                            Recipient = recipient
                        };
            t.AddParent(this);

            return t;
        }

        private void Reply(UI.Tweet t, string text)
        {
            ConvertAndSendTweets(text, s => Tweet(s, t));
        }

        private void ConvertAndSendTweets(string text, Func<string, bool> tweetSendingAction)
        {
            if (text.Length > 9*TwitterHelper.TWEET_LENGTH) return; // too much, possibly bordering on spam

            IList<string> outputTweets = TwitterHelper.SplitIntoMessages(text);

            if (outputTweets.Count > 1)
            {
                // multiple messages - format accordingly
                for (int i = 0; i < outputTweets.Count; i++)
                {
                    if (i == 0)
                        outputTweets[i] = string.Format("{0}" + Ellipsis, outputTweets[i]);
                    else if (i == outputTweets.Count - 1)
                    {
                        string contents;
                        string prefix = TwitterHelper.GetTweetPrefix(outputTweets[i], out contents);

                        outputTweets[i] = string.Format("{0}" + Ellipsis + "{1}", prefix, contents);
                    }
                    else
                    {
                        string contents;
                        string prefix = TwitterHelper.GetTweetPrefix(outputTweets[i], out contents);

                        outputTweets[i] = string.Format("{0}" + Ellipsis + "{1}" + Ellipsis, prefix, contents);
                    }
                }
            }

            if (outputTweets.Count > 0)
            {
                foreach (string i in outputTweets)
                {
                    if (string.IsNullOrWhiteSpace(i)) continue;
                    string value = i;

                    int attempts = 1;
                    bool isSuccessful = tweetSendingAction(value);
                    while (!isSuccessful && attempts < 5)
                    {
                        attempts++;
                        Task.Delay(5000);
                        isSuccessful = tweetSendingAction(value);
                    }

                    if (isSuccessful)
                    {
                        _eventAggregator.GetEvent<ShowNotification>().Publish(
                            new ShowNotificationPayload("Tweeted!", TimeSpan.FromSeconds(5)));
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ShowNotification>().Publish(
                            new ShowNotificationPayload("Unable to tweet!", TimeSpan.Zero,
                                                        NotificactionLevel.Error));
                    }

                    Task.Delay(5000);
                }
            }
        }

        private void LoadConversation(string id)
        {
            var conversation = new TwitterConversationViewModel(this, "Conversation", id);
            _eventAggregator.GetEvent<ShowConversation>().Publish(conversation);
        }

        private void ConnectStream()
        {
            if ((streamingStatus == null || streamingStatus.IsCompleted) && UserStreams)
            {
                _twitterClient.SetOAuthToken(new Credentials
                                                 {
                                                     OAuthToken = Credentials.Username,
                                                     OAuthTokenSecret = Credentials.Password
                                                 });
                _twitterClient.StreamingReconnectAttemptEvent -= TwitterStreamingStreamingReconnectAttemptEvent;
                _twitterClient.StreamingReconnectAttemptEvent += TwitterStreamingStreamingReconnectAttemptEvent;

                UpdateTimer(ref _timelineTimer, TriggerTimelineUpdate, 0);
                UpdateTimer(ref _mentionTimer, TriggerMentionsUpdate, 0);
                UpdateTimer(ref _directMsgTimer, TriggerDirectMessageUpdate, 0);

                streamingStatus = _twitterClient.BeginStream(StreamCallback, null);
            }
        }

        public void DeleteTweet(UI.Tweet Tweet)
        {
            _twitterClient.Statuses.BeginDestroy(Tweet.ID, null);
        }
    }
}