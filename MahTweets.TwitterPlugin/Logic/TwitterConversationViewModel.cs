using System.Collections.Generic;
using System.Linq;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.ViewModels;
using MahTweets.TwitterPlugin.UI;

namespace MahTweets.TwitterPlugin.Logic
{
    public class TwitterConversationViewModel : ConversationViewModel
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly string _latestId;
        private readonly IStatusUpdateRepository _repository;

        public TwitterConversationViewModel()
        {
            _repository = CompositionManager.Get<IStatusUpdateRepository>();
            _contactsRepository = CompositionManager.Get<IContactsRepository>();
        }

        public TwitterConversationViewModel(IMicroblog source, string title, string id)
            : this()
        {
            Source = source;
            Title = title;
            _latestId = id;
        }

        public override void Start()
        {
            FetchTweet(_latestId);
        }

        private void FetchTweet(string latestId)
        {
            var firstTweet = _repository.GetById<Tweet>(latestId);
            IEnumerable<IStatusUpdate> moarTweets =
                ((IStatusUpdateService) _repository).OutgoingUpdates.Where(
                    t => t is Tweet && ((Tweet) t).InReplyToID == latestId);
            if (moarTweets.Any())
            {
                moarTweets.ForEach(t =>
                                       {
                                           if (!Updates.Contains(t))
                                               Updates.Add(t);
                                       });
            }
            if (firstTweet != null)
            {
                Updates.Add(firstTweet);
                LockAddContact(firstTweet.Contact);

                if (!string.IsNullOrWhiteSpace(firstTweet.InReplyToID) && firstTweet.InReplyToID != "0")
                {
                    FetchTweet(firstTweet.InReplyToID);
                }
            }
            else
            {
                var twitter = ((Twitter) Source);
                twitter._twitterClient.Statuses.BeginGetTweet(latestId, (res, req, obj) =>
                                                                            {
                                                                                if (obj != null &&
                                                                                    obj is MahApps.Twitter.Models.Tweet)
                                                                                {
                                                                                    var tweet =
                                                                                        (MahApps.Twitter.Models.Tweet)
                                                                                        obj;
                                                                                    if (tweet.User == null)
                                                                                    {
                                                                                        return;
                                                                                    }

                                                                                    TwitterContact foundContact =
                                                                                        Contacts.OfType<TwitterContact>()
                                                                                            .FirstOrDefault(
                                                                                                x =>
                                                                                                x.Name ==
                                                                                                tweet.User.ScreenName);
                                                                                    if (foundContact == null)
                                                                                    {
                                                                                        foundContact =
                                                                                            _contactsRepository.
                                                                                                GetOrCreate
                                                                                                <TwitterContact>(
                                                                                                    tweet.User.
                                                                                                        ScreenName,
                                                                                                    twitter.Source);
                                                                                        foundContact.
                                                                                            UpdateContactWithTwitterUser
                                                                                            (tweet.User, tweet.Created);

                                                                                        LockAddContact(foundContact);
                                                                                    }

                                                                                    Tweet t =
                                                                                        TwitterHelper.CreateTweet(
                                                                                            tweet, foundContact, Source);
                                                                                    t.Types.AddUpdate<NormalUpdate>(
                                                                                        Source);
                                                                                    if (!Updates.Contains(t))
                                                                                        Updates.Add(t);

                                                                                    if (
                                                                                        !string.IsNullOrWhiteSpace(
                                                                                            t.InReplyToID) &&
                                                                                        t.InReplyToID != "0")
                                                                                    {
                                                                                        FetchTweet(t.InReplyToID);
                                                                                    }
                                                                                }
                                                                            });
            }
        }
    }
}