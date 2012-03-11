using System;
using System.Globalization;
using MahTweets.Core;

namespace MahTweets.TwitterPlugin.Logic
{
    public class TwitterContact : Contact
    {
        private bool _followed;
        private long? _followers;
        private long? _following;
        private string _url;

        public string Url
        {
            get { return _url; }
            set
            {
                if (_url == value) return;

                _url = value;
                RaisePropertyChanged(() => Url);
            }
        }

        public bool Followed
        {
            get { return _followed; }
            set
            {
                if (_followed == value) return;

                _followed = value;
                RaisePropertyChanged(() => Followed);
            }
        }

        public long? Followers
        {
            get { return _followers; }
            set
            {
                _followers = value;
                RaisePropertyChanged(() => Followers);
            }
        }

        public long? Following
        {
            get { return _following; }
            set
            {
                _following = value;
                RaisePropertyChanged(() => Following);
            }
        }

        public string Time { get; set; } // TODO remove me. something is binding to this somewhere in ProfileViewStream

        public string PrettyPrintFullDetails
        {
            get
            {
                return
                    string.Format(
                        "Name: {5}" + Environment.NewLine + "Followers: {0}" + Environment.NewLine + "Following: {1}" +
                        Environment.NewLine + "IsProtected: {2}" + Environment.NewLine + "Url: {3}" +
                        Environment.NewLine + "Bio: {4}", Followers, Following,
                        IsProtected.ToString(CultureInfo.InvariantCulture), Url, Bio, FullName);
            }
        }
    }
}