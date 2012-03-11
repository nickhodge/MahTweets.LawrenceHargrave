using System.ComponentModel.Composition;
using MahTweets.Core.Settings;
using MahTweets2.Library;
using MahTweets2.Library.MEF;

namespace MahTweets2.TwitterPlugin.Logic
{
    public class TwitterUserControl : SettingsUserControl<Twitter>
    {
        [Import(typeof(IPluginSettingsProvider), AllowRecomposition = true)]
        public IPluginSettingsProvider PluginSettings { get; set; }

        public TwitterUserControl()
        {
            InitializeComponent();
            CompositionManager.ComposeParts(this);
        }

        public override void Load(Twitter t)
        {
            // TODO: get values from plugin, if required
            // bind to data context

        }

        public override void Save(Twitter t)
        {
            PluginSettings.Set(t, TwitterHelper.DIRECTMSG_SETTING, t.DirectMessageFrequency);
            PluginSettings.Set(t, TwitterHelper.MENTION_SETTING, t.MentionFrequency);
            PluginSettings.Set(t, TwitterHelper.TIMELINE_SETTING, t.TimelineFrequency);
            PluginSettings.Set(t, TwitterHelper.LISTS_SETTING, t.ListsFrequency);
            PluginSettings.Set(t, TwitterHelper.TWEETSPERCALL_SETTING, t.TweetsPerCall);
            PluginSettings.Set(t, TwitterHelper.GEOTAG_SETTING, t.GeoTag);
            PluginSettings.Save();

            t.OnSettingsUpdated();
        }

    }

    /// <summary>
    /// Interaction logic for TwitterSettings.xaml
    /// </summary>
    public partial class TwitterSettings : TwitterUserControl
    {
        public TwitterSettings()
        {
            InitializeComponent();
        }
    }
}
