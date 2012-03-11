using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MahTweets.Core;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.TwitterPlugin.UI;

// ReSharper disable CheckNamespace

namespace MahTweets.TwitterPlugin.Logic
// ReSharper restore CheckNamespace
{
    internal static class TwitterHelper
    {
        public const string RETWEET_PREFIX = "RT @";
        public const string MENTION_PREFIX = "@";
        public const string DIRECTMESSAGE_PREFIX = "d ";
        public const int TWEET_LENGTH = 140;

        public const string TIMELINE_SETTING = "TimelineFrequency";
        public const string DIRECTMSG_SETTING = "DirectMessageFrequency";
        public const string MENTION_SETTING = "MentionFrequency";
        public const string RETWEET_SETTING = "RetweetFrequency";
        public const string LISTS_SETTING = "ListsFrequency";
        public const string TWEETSPERCALL_SETTING = "TweetsPerCall";
        public const string GEOTAG_SETTING = "GeoTag";
        public const string USERSTREAM_SETTING = "UserStream";

        public const string Ellipsis = "\u2026";
                            // unicode for ellipsis or ... (three dots as a single character, not comment to be continued silly)

        private const string HttpUrlRgxPattern =
            @"(?<url>(http|ftp|https):\/\/[\w\-_]+\.[\w\-_]+[\w\-\.,@!?^=%&amp;\(\):/~\+#]*[\w\-\@!?^=%&amp;/~\+\(\)#])?[\x22\.\”]?";

        public static readonly Func<Tweet, IMicroblog, Tweet>
            ConvertToMention = (t, acct) =>
                                   {
                                       t.Types.AddUpdate<MentionUpdate>(acct);
                                       return t;
                                   };

        public static readonly Func<Tweet, IMicroblog, Tweet>
            ConvertToDM = (t, acct) =>
                              {
                                  t.Types.AddUpdate<DirectMessageUpdate>(acct);
                                  return t;
                              };

        public static readonly Func<Tweet, IMicroblog, Tweet>
            ConvertToSentDM = (t, acct) =>
                                  {
                                      t.Types.AddUpdate<SelfMessageUpdate>(acct);
                                      return t;
                                  };

        public static readonly Func<Tweet, Twitter, Tweet>
            ConvertToTimeline = (t, acct) =>
                                    {
                                        if (t.RetweetBy != null)
                                            t.Types.AddUpdate<RetweetUpdate>(acct);
                                        else if (t.Text.ToUpper().Contains("@" + acct.Credentials.AccountName.ToUpper()))
                                        {
                                            t.Types.AddUpdate<MentionUpdate>(acct);
                                        }
                                        else if (t.Contact.Name.ToUpper() == acct.Credentials.AccountName.ToUpper())
                                        {
                                            t.Types.AddUpdate<SelfUpdate>(acct);
                                        }
                                        else
                                        {
                                            t.Types.AddUpdate<NormalUpdate>(acct);
                                        }
                                        return t;
                                    };

        public static readonly Func<Tweet, Twitter, ListUpdate, Tweet>
            ConvertToList = (t, acct, list) =>
                                {
                                    //t.TwitterList = list.Name;
                                    t.Types.Add(list);
                                    if (t.Text.ToUpper().Contains("@" + acct.Credentials.AccountName.ToUpper()))
                                    {
                                        t.Types.AddUpdate<MentionUpdate>(acct);
                                    }
                                    else if (t.Contact.Name.ToUpper() == acct.Credentials.AccountName.ToUpper())
                                    {
                                        t.Types.AddUpdate<SelfUpdate>(acct);
                                    }
                                    return t;
                                };

        public static readonly Func<Tweet, Twitter, Tweet>
            ConvertToRetweet = (t, acct) =>
                                   {
                                       t.Types.Add(new RetweetUpdate());
                                       if (t.Text.ToUpper().Contains("@" + acct.Credentials.AccountName.ToUpper()))
                                       {
                                           t.Types.AddUpdate<MentionUpdate>(acct);
                                       }
                                       else if (t.Contact.Name.ToUpper() == acct.Credentials.AccountName.ToUpper())
                                       {
                                           t.Types.AddUpdate<SelfUpdate>(acct);
                                       }
                                       return t;
                                   };


        public static Tweet CreateTweet(MahApps.Twitter.Models.Tweet s, TwitterContact contact, IMicroblog source)
        {
            if (s.RetweetedStatus != null)
                return CreateRetweet(s, contact);


            if (contact.ImageUrl != new Uri(s.User.ProfileImageUrl))
                contact.SetContactImage(new Uri(s.User.ProfileImageUrl), s.Created);
            contact.Bio = s.User.Description;
            contact.Following = s.User.FriendsCount;
            contact.Followers = s.User.FollowersCount;
            contact.FullName = s.User.Name;

            var t = new Tweet
                        {
                            ID = s.Id.ToString(),
                            Contact = contact,
                            Text = WebUtility.HtmlDecode(s.Text),
                            Time = s.Created.ToLocalTime(),
                            SourceUri = s.Source.GetSourceURL(),
                            Source = s.Source.GetSourceText(),
                            Favourite = s.Favourited,
                            Microblog = source,
                        };

            if (s.Coordinates != null && s.Coordinates.Lat != null && s.Coordinates.Long != null)
                t.Location = new GeoLocation((double) s.Coordinates.Lat, (double) s.Coordinates.Long);

            if (s.InReplyToStatusId > 0)
            {
                t.InReplyToID = s.InReplyToStatusId.ToString();
                t.InReplyTo = new Contact {Name = s.InReplyToScreenName};
            }

            t.AddParent(source);

            return t;
        }

        public static Tweet CreateRetweet(MahApps.Twitter.Models.Tweet s, TwitterContact contact)
        {
            contact.UpdateContactWithTwitterUser(s.User, s.Created);

            var t = new Tweet
                        {
                            ID = s.Id.ToString(),
                            Contact = contact,
                            Text = WebUtility.HtmlDecode(s.Text),
                            Time = s.Created.ToLocalTime(),
                            SourceUri = s.Source.GetSourceURL(),
                            Source = s.Source.GetSourceText(),
                            Favourite = s.Favourited,
                        };

            if (s.Coordinates != null && s.Coordinates.Lat != null && s.Coordinates.Long != null)
                t.Location = new GeoLocation((double) s.Coordinates.Lat, (double) s.Coordinates.Long);

            if (s.InReplyToStatusId > 0)
            {
                t.InReplyToID = s.InReplyToStatusId.ToString();
                t.InReplyTo = new Contact {Name = s.InReplyToScreenName};
            }

            return t;
        }

        private static string MatchURLandShorten(String word)
        {
            Match mpa = Regex.Match(word, HttpUrlRgxPattern);
            if (mpa.Success)
            {
// TODO check for pre-existing shortened URL.                
// TODO insert shortening code here
            }
            return word;
        }


        public static IList<string> SplitIntoMessages(string text)
        {
            var outputTweets = new List<string>();

            // don't need to split, return single item in list
            if (text.Length <= TWEET_LENGTH)
            {
                outputTweets.Add(text.Trim());
                return outputTweets;
            }

            // if we have a Twitter-specific message, extract it so that 
            // we can add it to multiple messages
            string contents;
            string prefix = GetTweetPrefix(text, out contents);

            string[] array = contents.Split(' ');

            int maximumLength = TWEET_LENGTH - 2 - prefix.Length;

            var str = new StringBuilder(prefix);

            foreach (string t1 in array.Select(t => MatchURLandShorten(t)))
            {
                if ((str.Length + 1 + t1.Length) <= maximumLength)
                {
                    str.Append(" " + t1);
                }
                else
                {
                    outputTweets.Add(str.ToString().Trim());
                    str = new StringBuilder(prefix + t1);
                }
            }
            // add final tweet
            outputTweets.Add(prefix + str.ToString().Trim());
            return outputTweets;
        }

        public static string GetTweetPrefix(string text, out string contents)
        {
            string prefix = string.Empty;
            if (text.StartsWith(MENTION_PREFIX))
            {
                // check each word and remove multiple '@'s if they're found
                string[] allWords = text.Split(' ');

                var mentionNames = new List<string>();

                foreach (string t in allWords)
                {
                    if (t.StartsWith("@") && t.Length > 1)
                        mentionNames.Add(t);
                    else
                        break;
                }

                // we have a mention 
                //prefix = text.Substring(0, text.IndexOf(" ", TwitterHelper.MENTION_PREFIX.Length + 1));
                prefix = string.Join(" ", mentionNames.ToArray());
            }
            else if (text.StartsWith(DIRECTMESSAGE_PREFIX))
            {
                // we have a DM - get the prefix and the name
                prefix = text.Substring(0, text.IndexOf(" ", DIRECTMESSAGE_PREFIX.Length + 1, StringComparison.Ordinal));
            }
            else if (text.StartsWith(RETWEET_PREFIX))
            {
                // we have a RT - get the prefix and the name
                prefix = text.Substring(0, text.IndexOf(" ", RETWEET_PREFIX.Length + 1, StringComparison.Ordinal));
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