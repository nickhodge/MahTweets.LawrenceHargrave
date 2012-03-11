using System;
using System.Windows;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;

namespace MahTweets.Core.Extensions
{
    public static class EventExtensions
    {
        public static void Publish<T, TArgs>(this IEventAggregator eventAggregator, TArgs payload)
            where T : CompositePresentationEvent<TArgs>
        {
            var foundEvent = eventAggregator.GetEvent<T>();
            if (foundEvent == null) return;

            foundEvent.Publish(payload);
        }

        public static void Subscribe<T, TArgs>(this IEventAggregator eventAggregator, Action<TArgs> handler)
            where T : CompositePresentationEvent<TArgs>
        {
            var foundEvent = eventAggregator.GetEvent<T>();
            if (foundEvent == null) return;

            foundEvent.Subscribe(handler);
        }

        public static void PublishMessage(this IEventAggregator eventAggregator, string message)
        {
            eventAggregator.GetEvent<PluginMessage>().Publish(Message.Of(message));
        }

        public static void Show(this IEventAggregator eventAggregator, UIElement element)
        {
            eventAggregator.GetEvent<ShowDialog>().Publish(element);
        }
    }
}