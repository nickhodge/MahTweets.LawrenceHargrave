using System.Windows;

namespace MahTweets.Core.Events.EventTypes
{
    public class HideNotification : CompositePresentationEvent<HideNotificationPayload>
    {
    }

    public class HideNotificationPayload
    {
        public HideNotificationPayload(UIElement element)
        {
            Element = element;
        }

        public UIElement Element { get; private set; }
    }
}