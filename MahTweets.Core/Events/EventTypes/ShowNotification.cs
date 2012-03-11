using System;
using System.Windows;

namespace MahTweets.Core.Events.EventTypes
{
    public class ShowNotification : CompositePresentationEvent<ShowNotificationPayload>
    {
    }

    public enum NotificactionLevel
    {
        None,
        Information,
        Error
    }

    public class ShowNotificationPayload
    {
        public ShowNotificationPayload(FrameworkElement element, TimeSpan timeSpan,
                                       NotificactionLevel notificactionLevel = NotificactionLevel.None)
        {
            Element = element;
            TimeSpan = timeSpan;
            NotificactionLevel = notificactionLevel;
        }

        public ShowNotificationPayload(string text, TimeSpan timeSpan,
                                       NotificactionLevel notificactionLevel = NotificactionLevel.None)
        {
            Text = text;
            TimeSpan = timeSpan;
            NotificactionLevel = notificactionLevel;
        }

        public FrameworkElement Element { get; set; }

        public string Text { get; private set; }


        public TimeSpan TimeSpan { get; private set; }

        public NotificactionLevel NotificactionLevel { get; private set; }
    }
}