using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hammock;
using MahApps.Twitter;
using MahApps.Twitter.Models;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using Tweet = MahTweets.TwitterPlugin.UI.Tweet;

namespace MahTweets.TwitterPlugin.Logic
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TwitterSearchClassic : ISearchProvider
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly IPluginRepository _pluginRepository;

        private readonly IList<string> _searchTerms;
        private IStatusUpdateService _statusUpdate;
        private TwitterClient _twitterClient;

        [ImportingConstructor]
        public TwitterSearchClassic(
            IContactsRepository contactsRepository,
            IPluginRepository pluginRepository)
        {
            _contactsRepository = contactsRepository;
            _pluginRepository = pluginRepository;

            _searchTerms = new List<string>();
        }

        public Twitter Parent { private get; set; }

        #region ISearchProvider Members

        public string Id
        {
            get { return "Twitter Search"; }
        }

        public string Name
        {
            get { return "Twitter Search"; }
        }

        public string Protocol
        {
            get { return "twittersearch"; }
        }

        public BitmapImage Icon
        {
            get { return null; }
        }

        public Credential Credentials { get; set; }

        public bool IsAutomatic
        {
            get { return false; }
        }

        public IEnumerable<string> SearchTerms
        {
            get { return _searchTerms; }
        }

        public void AddSearchTerm(string term)
        {
            // require values
            if (string.IsNullOrWhiteSpace(term))
                return;

            // prevent duplicates
            if (_searchTerms.Any(m => m.Matches(term, false)))
                return;

            _searchTerms.Add(term);
        }

        public void RemoveSearchTerm(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return;

            _searchTerms.Add(term);
        }

        public void Start(IStatusUpdateService updateRepository)
        {
            _statusUpdate = updateRepository;

            if (Parent == null)
                Parent = _pluginRepository.Microblogs.OfType<Twitter>().First();

            _twitterClient = Parent._twitterClient;
        }

        public void Stop()
        {
        }

        public void Refresh()
        {
            Task.Run(() => RefreshInternal());
        }

        public bool HasSettings
        {
            get { return false; }
        }

        public void ShowSettings()
        {
        }

        public void Setup()
        {
        }

        #endregion

        private void RefreshInternal()
        {
            _searchTerms.ForEach(SearchInternal);
        }

        private void SearchInternal(string term)
        {
            _twitterClient.Search.BeginSearch(term, HandleSearchResults);
        }

        private void HandleSearchResults(RestRequest request, RestResponse response, object obj)
        {
            var updates = new List<Tweet>();

            if (obj is List<SearchTweet>)
            {
                var results = (List<SearchTweet>) obj;

                foreach (SearchTweet s in results)
                {
                    try
                    {
                        var c = _contactsRepository.GetOrCreate<TwitterContact>(s.ContactName, Parent.Source);
                        c.SetContactImage(new Uri(s.ContactImage), DateTime.Parse(s.CreatedDate.ToString()));

                        var t = new Tweet
                                    {
                                        ID = s.Id.ToString(),
                                        Contact = c,
                                        Microblog = Parent,
                                        Text = WebUtility.HtmlDecode(s.Text),
                                        Time = DateTime.Parse(s.CreatedDate.ToString()).ToLocalTime(),
                                        SourceUri = s.Source.StripHTML().GetSourceURL(),
                                        Source = s.Source.StripHTML().GetSourceText(),
                                        Types = new List<UpdateType> {new SearchUpdate()}
                                    };

                        t.AddParent(Parent);
                        updates.Add(t);
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                }
            }

            if (updates.Any())
                _statusUpdate.Send(updates);
        }
    }
}