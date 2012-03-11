namespace MahTweets.Core.Interfaces.Plugins
{
    public interface IPluginSettings
    {
        string SettingsId { get; }
        void SaveSettings();

        void LoadDefaultSettings();

        void LoadSettings();

        void OnSettingsUpdated();
    }
}