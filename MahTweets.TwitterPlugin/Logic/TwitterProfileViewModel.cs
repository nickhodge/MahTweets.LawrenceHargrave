using System;
using System.Collections.Generic;
using System.Linq;
using MahApps.Twitter;
using MahApps.Twitter.Models;
using MahTweets.Core.Collections;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.ViewModels;
using MahTweets.TwitterPlugin.Views;

namespace MahTweets.TwitterPlugin.Logic
{
    public class TwitterProfileViewModel : ProfileViewModel
    {
        private readonly TwitterClient _twitterClient;

        public TwitterProfileViewModel()
        {
            ContactUpdates = new ThreadSafeObservableCollection<IStatusUpdate>();
            ProfileDetails = new ProfileView();
        }

        public TwitterProfileViewModel(IMicroblog source, IContact contact, TwitterClient twitterClient)
            : this()
        {
            _twitterClient = twitterClient;

            Source = source;
            Contact = contact;
        }

        public TwitterContact TwitterContact
        {
            get { return Contact as TwitterContact; }
        }

        public override void Start()
        {
            _twitterClient.Statuses.BeginUserTimeline(Contact.Name, (req, res, obj) =>
                                                                        {
                                                                            try
                                                                            {
                                                                                var tweets = obj as List<Tweet>;
                                                                                if (tweets == null) return;
                                                                                if (TwitterContact == null) return;

                                                                                TwitterContact.SetContactImage(
                                                                                    new Uri(
                                                                                        "https://api.twitter.com/1/users/profile_image/" +
                                                                                        Contact.Name + "?size=bigger"),
                                                                                    null);
                                                                                foreach (
                                                                                    UI.Tweet t in
                                                                                        tweets.Select(
                                                                                            u =>
                                                                                            TwitterHelper.CreateTweet(
                                                                                                u, TwitterContact,
                                                                                                Source)))
                                                                                {
                                                                                    t.AddParent(Source);
                                                                                    t.Types.AddUpdate<NormalUpdate>(
                                                                                        Source);
                                                                                    ContactUpdates.Add(t);
                                                                                }
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                CompositionManager.Get
                                                                                    <IExceptionReporter>().
                                                                                    ReportHandledException(ex);
                                                                            }
                                                                        });
        }
    }
}