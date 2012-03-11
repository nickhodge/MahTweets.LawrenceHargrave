using MahTweets.Core.Interfaces.Settings;

namespace MahTweets.Core.Interfaces.Plugins
{
    public interface IHaveSettings
    {
        void LoadDefaultSettings();

        void LoadSettings(IPluginSettingsProvider provider);

        void OnSettingsUpdated();
    }
}