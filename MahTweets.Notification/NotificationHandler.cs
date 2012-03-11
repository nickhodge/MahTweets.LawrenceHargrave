using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media.Imaging;
using MahTweets.Core;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Notification
{
    public class NotificationHandler : Notify, IStatusHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private bool enabled;

        [ImportingConstructor]
        public NotificationHandler(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #region IStatusHandler Members

        public void HandleUpdates(IEnumerable<IStatusUpdate> StatusUpdates)
        {
            _eventAggregator.GetEvent<ShowNotification>()
                .Publish(new ShowNotificationPayload(StatusUpdates.Count() + " new updates", TimeSpan.FromSeconds(15)));
        }

        public void HandleUpdate(IStatusUpdate StatusUpdate)
        {
            _eventAggregator.GetEvent<ShowNotification>()
                .Publish(new ShowNotificationPayload("1 new update", TimeSpan.FromSeconds(15)));
        }

        public bool Enabled
        {
            get
            {
                if (Credentials == null)
                    return enabled;
                else
                    return Boolean.Parse(Credentials.Username);
            }
            set
            {
                enabled = value;

                if (Credentials == null)
                    Credentials = new Credential
                                      {
                                          Username = value.ToString(),
                                          Protocol = Protocol
                                      };

                Credentials.Username = value.ToString();

                RaisePropertyChanged(() => Enabled);
            }
        }

        public string Id
        {
            get { return ""; }
        }

        public string Name
        {
            get { return "Notifications"; }
        }

        public string Protocol
        {
            get { return "notificationsDemo"; }
        }

        public BitmapImage Icon
        {
            get { return null; }
        }

        public Credential Credentials { get; set; }

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
    }
}