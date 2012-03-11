using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahTweets.Core;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Location;
using MahTweets.TwitterPlugin.Logic;
using MahTweets.TwitterPlugin.UI;

namespace MahTweets.TwitterPlugin.Views
{
    /// <summary>
    /// Interaction logic for DirectMessageView.xaml
    /// </summary>
    public partial class DirectMessageView
    {
        public DirectMessageView()
        {
            InitializeComponent();
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<PasteInlineReply>().Subscribe(DoInlinePastedText);
            Loaded += TweetView_Loaded;
        }

        public DirectMessageTweet Tweet
        {
            get { return DataContext as DirectMessageTweet; }
        }

        public void DoInlinePastedText(PasteInlineReplyPayload pastedReply)
        {
            if (pastedReply.id == Tweet.ID)
            {
                DoInlineReply(pastedReply.Text);
            }
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
                    displaytext = Tweet.Hashtags.Aggregate(displaytext + " ",
                                                           (existing,
                                                            tag) => existing + " " + tag.Tag);
                }
            }

            suvContainer.ToggleCompositionBoxContainer(displaytext);
        }

        private void TweetView_Loaded(object sender, RoutedEventArgs e)
        {
            suvContainer.CompositionBox.SendMessage = new SendMessageCommand(ItemViewSendMessage);
            suvContainer.SendMessage = new SendMessageCommand(ItemViewSendMessage);
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

        private void ItemViewSendMessage(object obj, IInputElement element)
        {
            string message = obj.ToString();
            Reply(message);
        }

        private void Reply(string text)
        {
            object[] parameter = new string[1];
            parameter[0] = text;

            PluginEventHandler.FireEvent("reply", Tweet, parameter);

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
                Twitter src = Tweet.Parents.OfType<Twitter>().First();
                Process.Start(GlobalPosition.GetLocationUrl(Tweet.Location, src.AppSettings.MapEngine));
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void suvContainer_ContactNameClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            PluginEventHandler.FireEvent("profile", Tweet, "http://www.twitter.com/" + Tweet.Contact.Name);
        }

        private void ReplyAllClick(object sender, RoutedEventArgs e)
        {
            string contents;
            string x = UIHelper.GetTweetPrefix(Tweet.Text, out contents);

            //Make sure the existing user isn't being replied to.
            string toRemove = "";
            foreach (IMicroblog parent in Tweet.Parents)
            {
                toRemove += "@" + parent.Credentials.AccountName + " ";
            }

            string toReplyTo = x.Replace(toRemove, "");
            if (!toReplyTo.Contains("@" + Tweet.Contact.Name))
                toReplyTo = "@" + Tweet.Contact.Name + " " + toReplyTo;

            if (Tweet.Hashtags.Count > 0)
            {
                if (Tweet.Hashtags.Count > 0)
                {
                    toReplyTo = Tweet.Hashtags.Aggregate(toReplyTo + " ",
                                                         (existing,
                                                          tag) => existing + " " + tag.Tag);
                }
            }

            suvContainer.ToggleCompositionBoxContainer(toReplyTo);
        }

        private void FollowClick(object sender, RoutedEventArgs e)
        {
            PluginEventHandler.FireEvent("follow", Tweet, null);
        }

        private void UnfollowClick(object sender, RoutedEventArgs e)
        {
            PluginEventHandler.FireEvent("unfollow", Tweet, null);
        }

        private void BlockClick(object sender, RoutedEventArgs e)
        {
            PluginEventHandler.FireEvent("block", Tweet, null);
        }

        private void SpamClick(object sender, RoutedEventArgs e)
        {
            PluginEventHandler.FireEvent("spam", Tweet, null);
        }

        private void ProfileClick(object sender, RoutedEventArgs e)
        {
            PluginEventHandler.FireEvent("linkclicked", Tweet, "http://www.twitter.com/" + Tweet.Contact.Name);
        }

        private void Run_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PluginEventHandler.FireEvent("profileByName", Tweet, Tweet.Recipient.Name);
        }
    }
}