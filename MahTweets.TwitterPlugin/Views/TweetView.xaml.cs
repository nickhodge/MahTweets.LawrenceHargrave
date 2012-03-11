using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MahTweets.Core;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.TwitterPlugin.Logic;
using MahTweets.TwitterPlugin.UI;
using MahTweets.UI.Controls;
using MahTweets.UI.Controls.Mapping;
using SelfUpdate = MahTweets.TwitterPlugin.Logic.SelfUpdate;

namespace MahTweets.TwitterPlugin.Views
{
    public partial class TweetView
    {
        private IPluginRepository _pluginRepository;

        public TweetView()
        {
            InitializeComponent();
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<PasteInlineReply>().Subscribe(DoInlinePastedText);
            Loaded += TweetViewLoaded;
        }

        public Tweet Tweet
        {
            get { return DataContext as Tweet; }
        }

        private void TweetViewLoaded(object sender, RoutedEventArgs e)
        {
            suvContainer.CompositionBox.SendMessage = new SendMessageCommand(ItemViewSendMessage);
            suvContainer.SendMessage = new SendMessageCommand(ItemViewSendMessage);

            _pluginRepository = CompositionManager.Get<IPluginRepository>();
            IEnumerable<Twitter> twitterAccounts = _pluginRepository.Microblogs.OfType<Twitter>();

            cbAccounts.ItemsSource = twitterAccounts;
            if (Tweet != null)
                cbAccounts.SelectedItem = Tweet.Types.Any()
                                              ? Tweet.Types.OrderByDescending(u => u.Order).First().Parent
                                              : twitterAccounts.First();

            if (cbAccounts.SelectedItem == null)
                cbAccounts.SelectedItem = twitterAccounts.First();
        }


        public void DoInlinePastedText(PasteInlineReplyPayload pastedReply)
        {
            if (pastedReply.id == Tweet.ID)
            {
                DoInlineReply(pastedReply.Text);
            }
        }


        private void BtnDirectMessageClick(object sender, RoutedEventArgs e)
        {
            suvContainer.ToggleCompositionBoxContainer(TwitterHelper.DIRECTMESSAGE_PREFIX + Tweet.Contact.Name + " ");
        }

        private void BtnRetweetClick(object sender, RoutedEventArgs e)
        {
            suvContainer.ToggleCompositionBoxContainer(TwitterHelper.RETWEET_PREFIX + Tweet.Contact.Name + " " +
                                                       Tweet.OriginalText);
        }

        private void BtnReplyClick(object sender, RoutedEventArgs e)
        {
            DoInlineReply(null);
        }

        private void DoInlineReply(string incomingdisplaytext)
        {
            string displaytext;
            if (Tweet.Types.HasType<DirectMessageUpdate>())
            {
                displaytext = TwitterHelper.DIRECTMESSAGE_PREFIX + Tweet.Contact.Name + " " + incomingdisplaytext;
            }
            else
            {
                displaytext = TwitterHelper.MENTION_PREFIX + Tweet.Contact.Name + " " + incomingdisplaytext;

                // Detect hashtags, and include. 
                if (Tweet.Hashtags.Count > 0)
                {
                    displaytext = Tweet.Hashtags.Aggregate(displaytext,
                                                           (existing,
                                                            tag) => existing + tag.Tag + " ");
                }
            }
            suvContainer.ToggleCompositionBoxContainer(displaytext);
        }

        private void ItemViewSendMessage(object obj, IInputElement element)
        {
            string message = obj.ToString();
            Reply(message);
        }

        private void Reply(string text)
        {
            var parameter = new string[1];
            parameter[0] = text;

            // bugfix: sometimes no account selected. may go kaboom!
            Twitter account = null;

            // get selected account
            if (cbAccounts.SelectedItem != null)
                account = cbAccounts.SelectedItem as Twitter;

            // if not defined, just use first account
            if (account == null)
            {
                IEnumerable<Twitter> twitterAccounts = _pluginRepository.Microblogs.OfType<Twitter>();
                account = twitterAccounts.FirstOrDefault();
            }

            // sanity check
            if (account != null)
                account.HandleEvent("reply", Tweet, parameter);

            suvContainer.ToggleCompositionBoxContainer(null);
        }

        private void BtnFavouriteClick(object sender, RoutedEventArgs e)
        {
            PluginEventHandler.FireEvent("favourite", Tweet, null);
            e.Handled = true;
        }

        private void BtnMapClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var map = new CloseableMap(Tweet.Location.Latitude, Tweet.Location.Longitude, Tweet.Contact);
                map.CloseCommand = new DelegateCommand(() => spBelowTextBox.Children.Remove(map));
                map.Width = double.NaN;
                map.Height = 200;
                spBelowTextBox.Children.Add(map);
                // TODO reinstitute click-to highlight map in browser
                //var src = Tweet.Parents.OfType<Twitter>().First();
                //Process.Start(GlobalPosition.GetLocationUrl(Tweet.Location, src.AppSettings.MapEngine));
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void ContainerContactClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Tweet.Parents.OfType<Twitter>().First().HandleEvent("profile", Tweet,
                                                                "http://www.twitter.com/" + Tweet.Contact.Name);
        }

        private void ReplyAllClick(object sender, RoutedEventArgs e)
        {
            string contents;
            string toReplyTo = UIHelper.GetTweetPrefix(Tweet.Text, out contents).ToLower();

            toReplyTo = Tweet.Parents.Aggregate(toReplyTo,
                                                (current, parent) =>
                                                current.Replace("@" + parent.Credentials.AccountName.ToLower(), ""));

            if (!toReplyTo.Contains("@" + Tweet.Contact.Name))
                toReplyTo = "@" + Tweet.Contact.Name + " " + toReplyTo.Trim();

            if (Tweet.Hashtags.Count > 0)
            {
                if (Tweet.Hashtags.Count > 0)
                {
                    toReplyTo = Tweet.Hashtags.Aggregate(toReplyTo.Trim(),
                                                         (existing,
                                                          tag) => existing + " " + tag.Tag);
                }
            }

            suvContainer.ToggleCompositionBoxContainer(toReplyTo);
        }

        private void FollowClick(object sender, RoutedEventArgs e)
        {
            if (Tweet.Parents.Count > 1)
            {
                var eventAggregator = CompositionManager.Get<IEventAggregator>();
                eventAggregator.Show(new MultiParentChoiceDialog(Tweet.Parents,
                                                                 microblog => microblog.HandleEvent("follow",
                                                                                                    Tweet,
                                                                                                    null), "Follow",
                                                                 Tweet.Contact.Name));
            }
            else
            {
                Tweet.Parents.First().HandleEvent("follow", Tweet, null);
            }
        }

        private void UnfollowClick(object sender, RoutedEventArgs e)
        {
            if (Tweet.Parents.Count > 1)
            {
                var eventAggregator = CompositionManager.Get<IEventAggregator>();
                eventAggregator.Show(new MultiParentChoiceDialog(Tweet.Parents,
                                                                 microblog => microblog.HandleEvent("unfollow",
                                                                                                    Tweet,
                                                                                                    null), "Unfollow",
                                                                 Tweet.Contact.Name));
            }
            else
            {
                Tweet.Parents.First().HandleEvent("unfollow", Tweet, null);
            }
        }

        private void BlockClick(object sender, RoutedEventArgs e)
        {
            if (Tweet.Parents.Count > 1)
            {
                var eventAggregator = CompositionManager.Get<IEventAggregator>();
                eventAggregator.Show(new MultiParentChoiceDialog(Tweet.Parents,
                                                                 microblog => microblog.HandleEvent("block",
                                                                                                    Tweet,
                                                                                                    null), "Block",
                                                                 Tweet.Contact.Name));
            }
            else
            {
                Tweet.Parents.First().HandleEvent("block", Tweet, null);
            }
        }

        private void SpamClick(object sender, RoutedEventArgs e)
        {
            if (Tweet.Parents.Count > 1)
            {
                var eventAggregator = CompositionManager.Get<IEventAggregator>();
                eventAggregator.Show(new MultiParentChoiceDialog(Tweet.Parents,
                                                                 microblog => microblog.HandleEvent("spam",
                                                                                                    Tweet,
                                                                                                    null),
                                                                 "Block and report for spam", Tweet.Contact.Name));
            }
            else
            {
                Tweet.Parents.First().HandleEvent("spam", Tweet, null);
            }
        }

        private void ProfileClick(object sender, RoutedEventArgs e)
        {
            PluginEventHandler.FireEvent("linkclicked", Tweet, "http://www.twitter.com/" + Tweet.Contact.Name);
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            IEnumerable<UpdateType> isSelf = Tweet.Types.Where(t => t is SelfUpdate);
            if (isSelf.Any())
            {
                IMicroblog parent =
                    Tweet.Parents.FirstOrDefault(p => p.Owner.Name.ToLower() == Tweet.Contact.Name.ToLower());
                if (parent != null)
                {
                    ((Twitter) parent).DeleteTweet(Tweet);
                    Visibility = Visibility.Collapsed;
                }
            }
        }
    }

    internal static class UIHelper
    {
        public static string GetTweetPrefix(string text, out string contents)
        {
            string prefix = string.Empty;
            if (text.StartsWith(TwitterHelper.MENTION_PREFIX))
            {
                // check each word and remove multiple '@'s if they're found
                string[] allWords = text.Split(new[] {' '});

                // we have a mention 
                //prefix = text.Substring(0, text.IndexOf(" ", TwitterHelper.MENTION_PREFIX.Length + 1));
                prefix = string.Join(" ", allWords.Where(t => t.StartsWith("@") && t.Length > 1).ToArray());
            }
            else if (text.StartsWith(TwitterHelper.DIRECTMESSAGE_PREFIX))
            {
                // we have a DM - get the prefix and the name
                prefix = text.Substring(0,
                                        text.IndexOf(" ", TwitterHelper.DIRECTMESSAGE_PREFIX.Length + 1,
                                                     StringComparison.Ordinal));
            }
            else if (text.StartsWith(TwitterHelper.RETWEET_PREFIX))
            {
                // we have a RT - get the prefix and the name
                prefix = text.Substring(0,
                                        text.IndexOf(" ", TwitterHelper.RETWEET_PREFIX.Length + 1,
                                                     StringComparison.Ordinal));
            }

            if (prefix != string.Empty)
            {
                prefix = string.Concat(prefix, " ");
                contents = text.Replace(prefix, string.Empty);
            }
            else
            {
                contents = text;
            }

            return prefix;
        }
    }
}