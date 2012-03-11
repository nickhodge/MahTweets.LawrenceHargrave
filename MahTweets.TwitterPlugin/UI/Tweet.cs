using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Global;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Location;

namespace MahTweets.TwitterPlugin.UI
{
    public class Tweet : StatusUpdate, IWeakEventListener
    {
        private bool _favourite;
        private List<Hashtag> _hashtags;
        private IContact _retweetBy;
        private string _text;

        public Tweet()
        {
            GlobalPulseManager.AddListener(GlobalPulseManager.Pulsor, this);
        }

        public string OriginalText { get; set; }

        public override string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (string.IsNullOrEmpty(OriginalText))
                {
                    OriginalText = value;
                }
                RaisePropertyChanged(() => Text);
            }
        }

        public IContact RetweetBy
        {
            get { return _retweetBy; }
            set
            {
                _retweetBy = value;
                RaisePropertyChanged(() => RetweetBy);
            }
        }

        public bool Favourite
        {
            get { return _favourite; }
            set
            {
                _favourite = value;
                RaisePropertyChanged(() => Favourite);
            }
        }

        public string Source { get; set; }
        public string SourceUri { get; set; }
        public IContact InReplyTo { get; set; }
        public string InReplyToID { get; set; }

        [DataMember]
        public IList<Hashtag> Hashtags
        {
            get
            {
                if (_hashtags != null)
                    return _hashtags;

                _hashtags = new List<Hashtag>();

                if (string.IsNullOrEmpty(OriginalText))
                    return _hashtags;

                var hashtagService = CompositionManager.Get<IHashtagService>();

                MatchCollection matches = Regex.Matches(OriginalText, @"(#[\w\d]{1,})");
                if (matches.Count == 0)
                    return _hashtags;

                foreach (Match match in matches)
                {
                    IMicroblog firstParent = Parents.First();
                    IEnumerable<IMicroblog> otherParents = Parents.Skip(1);

                    string tag = match.Groups[1].Value;

                    Hashtag hashtag = hashtagService.GetOrCreateHashtag(tag, firstParent);

                    foreach (IMicroblog otherParent in otherParents)
                    {
                        hashtag.AddSource(otherParent);
                    }
                    _hashtags.Add(hashtag);
                }
                return _hashtags;
            }
        }

        public String FriendlyTime
        {
            get { return DisplayFriendlyTime(FriendlyDistance); }
        }

        #region IWeakEventListener Members

        /// <summary>
        /// Recieves WeakEvents - including Global Pulses when it's time to actually update the time.
        /// </summary>
        /// <param name="managerType"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof (GlobalPulseManager))
            {
                RaisePropertyChanged(() => FriendlyTime);
                return true;
            }
            return false;
        }

        #endregion

        public void Track(String term)
        {
            var parameter = new string[1];
            parameter[0] = term;
            PluginEventHandler.FireEvent("track", this, parameter);
        }

        public override bool Filter(string ignore)
        {
            bool baseValue = base.Filter(ignore);

            if (!baseValue)
                return false;

            if (!string.IsNullOrWhiteSpace(Source))
            {
                if (Source.ToUpper().Contains(ignore.ToUpper()))
                    return false;
            }

            return true;
        }

        #region Location/Distance Magics

        public Distance? InterDistanceInKms
        {
            get
            {
                GeoLocation iamat = GlobalPosition.GetLocation();
                if (iamat != null && Location != null)
                {
                    return Haversine.CalculateDistance(iamat, Location);
                }

                return null;
            }
        }

        /// <summary>
        /// // Return distance between tweets. if tweet & user has no location, nothing is returned.
        /// </summary>
        public string FriendlyDistance
        {
            get
            {
                Distance? dist = InterDistanceInKms;
                if (dist.HasValue)
                {
                    return dist.Value.ToString();
                }

                return string.Empty;
            }
        }

        #endregion
    }
}