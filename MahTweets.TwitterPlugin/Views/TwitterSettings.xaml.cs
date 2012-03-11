using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.TwitterPlugin.Logic;

namespace MahTweets.TwitterPlugin.Views
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TwitterSettings
    {
        [ImportingConstructor]
        public TwitterSettings()
        {
            InitializeComponent();

            PluginSettings = CompositionManager.Get<IPluginSettingsProvider>();
        }

        private Twitter Placeholder { get; set; }

        public IPluginSettingsProvider PluginSettings { get; set; }

        public override void Load(IPlugin t)
        {
            Placeholder = t as Twitter;

            if (Placeholder == null)
                throw new ArgumentException("Plugin is not of type Twitter");

            DataContext = Placeholder;

            double value = GetTotalsForSliders();
            freeCount.Content = Placeholder.HourlyAPILimit - value;

            foreach (object item in cbTweetsPerCall.Items)
            {
                var ctrl = item as ComboBoxItem;
                if (ctrl == null) continue;

                string label = ctrl.Content.ToString();
                if (String.CompareOrdinal(label, Placeholder.TweetsPerCall.ToString()) == 0)
                {
                    cbTweetsPerCall.SelectedItem = item;
                    return;
                }
            }

            cbTweetsPerCall.SelectedIndex = 0;
        }

        public override void Save(IPlugin t)
        {
            var twitter = t as Twitter;
            if (twitter == null) return;

            var comboBoxItem = cbTweetsPerCall.SelectedItem as ComboBoxItem;
            if (comboBoxItem != null)
                twitter.TweetsPerCall = Convert.ToInt32(comboBoxItem.Content);

            if (PluginSettings != null)
            {
                PluginSettings.Set(twitter, TwitterHelper.DIRECTMSG_SETTING, twitter.DirectMessageFrequency);
                PluginSettings.Set(twitter, TwitterHelper.MENTION_SETTING, twitter.MentionFrequency);
                PluginSettings.Set(twitter, TwitterHelper.TIMELINE_SETTING, twitter.TimelineFrequency);
                PluginSettings.Set(twitter, TwitterHelper.LISTS_SETTING, twitter.ListsFrequency);
                PluginSettings.Set(twitter, TwitterHelper.TWEETSPERCALL_SETTING, twitter.TweetsPerCall);
                PluginSettings.Set(twitter, TwitterHelper.GEOTAG_SETTING, twitter.GeoTag);
                PluginSettings.Set(twitter, TwitterHelper.USERSTREAM_SETTING, twitter.UserStreams);

                PluginSettings.Save();
            }

            twitter.OnSettingsUpdated();
        }

        private void TimelineValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = GetTotalsForSliders();
            freeCount.Content = Placeholder.HourlyAPILimit - value;
        }

        private double GetTotalsForSliders()
        {
            var listsTotal =
                (int) (sliderListsTimeline.Value*(Placeholder.MyListCount + Placeholder.SubscribedListCount));

            return (int) sliderMainTimeline.Value + (int) sliderDMTimeline.Value + (int) sliderMentionTimeline.Value +
                   listsTotal;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = cbTweetsPerCall.SelectedItem as ComboBoxItem;
            if (comboBoxItem != null)
                Placeholder.TweetsPerCall = Convert.ToInt32(comboBoxItem.Content);
        }
    }
}