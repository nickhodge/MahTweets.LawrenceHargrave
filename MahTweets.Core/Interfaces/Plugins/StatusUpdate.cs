using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Interfaces.Plugins
{
    [DataContract]
    public class StatusUpdate : Notify, IStatusUpdate
    {
        private ObservableCollection<IStatusUpdateAttachment> _attachments;
        private string _colorArgb;
        private IContact _contact;
        private string _id;
        private bool _isRead;
        private GeoLocation _location;
        private ObservableCollection<IMicroblog> _parents;
        private string _text;
        private DateTime _time;
        private IList<UpdateType> _types;

        public StatusUpdate()
        {
            Types = new List<UpdateType>();
            Parents = new ObservableCollection<IMicroblog>();
            Attachments = new ObservableCollection<IStatusUpdateAttachment>();
        }

        #region IStatusUpdate Members

        [DataMember]
        public string ID
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged(() => ID);
            }
        }

        [DataMember(Name = "Contact")]
        public IContact Contact
        {
            get { return _contact; }
            set
            {
                _contact = value;
                RaisePropertyChanged(() => Contact);
            }
        }

        [DataMember(Name = "Text")]
        public virtual string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged(() => Text);
            }
        }

        [DataMember]
        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                _isRead = value;
                RaisePropertyChanged(() => IsRead);
            }
        }

        [DataMember]
        public DateTime Time
        {
            get { return _time; }
            set
            {
                _time = value;
                RaisePropertyChanged(() => Time);
            }
        }


        [DataMember]
        public GeoLocation Location
        {
            get { return _location; }
            set
            {
                _location = value;
                RaisePropertyChanged(() => Location);
            }
        }

        [DataMember]
        public string ColorARGB
        {
            get { return _colorArgb; }
            set
            {
                _colorArgb = value;
                RaisePropertyChanged(() => ColorARGB);
            }
        }

        public IMicroblog Microblog { get; set; }

        public ObservableCollection<IStatusUpdateAttachment> Attachments
        {
            get { return _attachments; }
            set
            {
                _attachments = value;
                RaisePropertyChanged(() => Attachments);
            }
        }

        [DataMember]
        public IList<UpdateType> Types
        {
            get { return _types; }
            set
            {
                _types = value;
                RaisePropertyChanged(() => Types);
            }
        }

        public ObservableCollection<IMicroblog> Parents
        {
            get { return _parents; }
            set
            {
                _parents = value;
                RaisePropertyChanged(() => Parents);
            }
        }

        public virtual bool Filter(string ignore)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return true;

            if (Contact.Name.Matches(ignore, true))
                return false;

            return !Text.ToUpper().Contains(ignore.ToUpper());
        }

        public bool HasAttachment(string url)
        {
            if (Attachments == null) return false;
            if (Attachments.Count == 0) return false;
            return Attachments.Any(statusUpdateAttachment => url.ToLower() == statusUpdateAttachment.MappedToUrl());
        }

        public void RemoveAttachment(string url)
        {
            if (HasAttachment(url))
            {
                foreach (IStatusUpdateAttachment statusUpdateAttachment in Attachments)
                {
                    if (url.ToLower() == statusUpdateAttachment.MappedToUrl().ToLower())
                        Attachments.Remove(statusUpdateAttachment);
                }
            }
        }

        #endregion

        public void AddParent(IMicroblog parent)
        {
            if (!Parents.Any(microblog => microblog.Owner.Name.Matches(parent.Owner.Name)))
                Parents.Add(parent);
        }

        protected string DisplayFriendlyTime(string friendlyDistance)
        {
            TimeSpan timeSince = (DateTime.Now - Time);
            if (timeSince < new TimeSpan(0, 0, 15))
                return "Just now" + friendlyDistance;

            if (timeSince < new TimeSpan(0, 1, 0))
                return "Less than a minute ago" + friendlyDistance;

            if (timeSince < new TimeSpan(1, 0, 0))
            {
                if (timeSince > new TimeSpan(0, 1, 59))
                    return timeSince.Minutes + " minutes ago" + friendlyDistance;

                return timeSince.Minutes + " minute ago" + friendlyDistance;
            }

            if (timeSince < new TimeSpan(23, 0, 0))
            {
                if (timeSince > new TimeSpan(1, 59, 59))
                    return timeSince.Hours + " hours ago" + friendlyDistance;
                return timeSince.Hours + " hour ago" + friendlyDistance;
            }
            return friendlyDistance == null
                       ? string.Format("{0} {1} {2}", Time.ToShortTimeString(),
                                       Time.DayOfWeek.ToString().Substring(0, 3), WithDayOfMonthSuffix(Time.Day))
                       : string.Format("{0} {1} {2} {3}", Time.ToShortTimeString(),
                                       Time.DayOfWeek.ToString().Substring(0, 3), WithDayOfMonthSuffix(Time.Day),
                                       friendlyDistance);
        }

        public string DisplayFriendlyTime()
        {
            TimeSpan timeSince = (DateTime.Now - Time);
            if (timeSince > new TimeSpan(24, 0, 0))
            {
                if (timeSince < new TimeSpan(48, 0, 0))
                {
                    return "Yesterday, " + Time.ToShortTimeString();
                }

                return Time.ToString(CultureInfo.InvariantCulture);
            }
            if (timeSince < new TimeSpan(0, 1, 0))
            {
                return "Less than a minute ago";
            }

            if (timeSince < new TimeSpan(1, 0, 0))
            {
                return timeSince > new TimeSpan(0, 1, 59)
                           ? timeSince.Minutes + " minutes ago"
                           : timeSince.Minutes + " minute ago";
            }

            if (timeSince < new TimeSpan(23, 0, 0))
            {
                return timeSince > new TimeSpan(1, 59, 59)
                           ? timeSince.Hours + " hours ago"
                           : timeSince.Hours + " hour ago";
            }
            return Time.ToShortTimeString();
        }

        private static string WithDayOfMonthSuffix(int dayOfMonth)
        {
            if (dayOfMonth == 1 || dayOfMonth == 21 || dayOfMonth == 31)
                return string.Format("{0}st", dayOfMonth);
            if (dayOfMonth == 2 || dayOfMonth == 22)
                return string.Format("{0}nd", dayOfMonth);
            if (dayOfMonth == 3 || dayOfMonth == 23)
                return string.Format("{0}rd", dayOfMonth);
            return string.Format("{0}th", dayOfMonth);
        }
    }
}