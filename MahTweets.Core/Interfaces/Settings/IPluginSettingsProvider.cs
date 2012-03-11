using System.ComponentModel.Composition;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Settings
{
    [InheritedExport]
    public interface IPluginSettingsProvider
    {
        void Save();

        void Reset();

        void Set<T>(IPlugin plugin, string key, T value);

        T Get<T>(IPlugin plugin, string key);

        bool HasSettingsFor(IPlugin foundSettings);
    }
}