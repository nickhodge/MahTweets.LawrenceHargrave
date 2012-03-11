using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;

namespace MahTweets.Views.Controls
{
    public partial class Messages
    {
        private readonly List<ShowNotificationPayload> NotificationQueue = new List<ShowNotificationPayload>();
        private readonly IEventAggregator _eventAggregator;

        public Messages()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _eventAggregator = CompositionManager.Get<IEventAggregator>();
            _eventAggregator.GetEvent<ShowNotification>().Subscribe(HandleShowNotification, ThreadOption.UIThread);
            _eventAggregator.GetEvent<HideNotification>().Subscribe(HandleHideNotification, ThreadOption.UIThread);
        }

        public void HandleShowNotification(ShowNotificationPayload showNotificationPayload)
        {
            if (showNotificationPayload.Element == null && !string.IsNullOrWhiteSpace(showNotificationPayload.Text))
            {
                showNotificationPayload.Element = new TextBlock
                                                      {
                                                          Text = showNotificationPayload.Text,
                                                          FontSize = (double) FindResource("SubHeadingFontSize"),
                                                          FontFamily = (FontFamily) FindResource("AltHeadingFont"),
                                                          Foreground = (Brush) FindResource("InverseTextColour"),
                                                      };
            }

            if (showNotificationPayload.Element == null)
                return;

            NotificationQueue.Add(showNotificationPayload);

            if (showNotificationPayload.Element.Parent != null)
                return;

            if (showNotificationPayload.TimeSpan != TimeSpan.Zero)
            {
                Action actRemoveNotification = async () =>
                                                         {
                                                             await
                                                                 Task.Delay(
                                                                     (int)
                                                                     showNotificationPayload.TimeSpan.TotalMilliseconds);
                                                             lock (NotificationQueue)
                                                             {
                                                                 if (NotificationQueue.Contains(showNotificationPayload))
                                                                 {
                                                                     NotificationQueue.Remove(showNotificationPayload);
                                                                 }
                                                             }
                                                             RemoveNotification(showNotificationPayload.Element);
                                                         };
                actRemoveNotification();
            }
            if (ccContent.Content == null && NotificationQueue.Any())
                ShowNotification(NotificationQueue.First().Element, NotificationQueue.First().NotificactionLevel);
        }


        private void HandleHideNotification(HideNotificationPayload payload)
        {
            if (NotificationQueue.Any())
            {
                ShowNotificationPayload found = NotificationQueue.FirstOrDefault(t => t.Element == payload.Element);
                if (found != null)
                    NotificationQueue.Remove(found);
            }

            RemoveNotification(payload.Element);
        }

        public void RemoveNotification(UIElement notification)
        {
            if (ccContent.Content == notification)
            {
                var sb = Resources["hideNotification"] as Storyboard;
                if (sb == null) return;

                sb.Completed += (s, e) =>
                                    {
                                        ccContent.Content = null;
                                        if (ccContent.Content == null && NotificationQueue.Any() &&
                                            NotificationQueue.First() != null)
                                            ShowNotification(NotificationQueue.First().Element,
                                                             NotificationQueue.First().NotificactionLevel);
                                    };

                sb.Begin();
            }
        }

        private void ShowNotification(UIElement element, NotificactionLevel notificactionLevel)
        {
            ccContent.Content = element;

            switch (notificactionLevel)
            {
                case NotificactionLevel.None:
                    rectError.Visibility = Visibility.Collapsed;
                    rectInfo.Visibility = Visibility.Collapsed;
                    rectBackground.Visibility = Visibility.Visible;
                    break;

                case NotificactionLevel.Information:
                    rectError.Visibility = Visibility.Collapsed;
                    rectInfo.Visibility = Visibility.Visible;
                    rectBackground.Visibility = Visibility.Visible;
                    break;

                case NotificactionLevel.Error:
                    rectError.Visibility = Visibility.Visible;
                    rectInfo.Visibility = Visibility.Collapsed;
                    rectBackground.Visibility = Visibility.Visible;
                    break;
            }

            var sb = Resources["showNotification"] as Storyboard;
            if (sb != null) sb.Begin();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            if (NotificationQueue.Any())
                NotificationQueue.Remove(NotificationQueue.First());

            RemoveNotification((UIElement) ccContent.Content);
        }
    }
}