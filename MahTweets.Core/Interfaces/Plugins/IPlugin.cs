using System.Windows.Media.Imaging;

namespace MahTweets.Core.Interfaces.Plugins
{
    public interface IPlugin
    {
        string Id { get; }

        string Name { get; }

        string Protocol { get; }

        BitmapImage Icon { get; }

        Credential Credentials { get; set; }

        bool HasSettings { get; }

        void ShowSettings();

        void Setup();
    }
}