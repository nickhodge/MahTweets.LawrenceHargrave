using System.ComponentModel.Composition;

namespace MahTweets.Core.Interfaces.Settings
{
    [InheritedExport]
    public interface ISettingsProvider
    {
        IApplicationSettingsProvider Application { get; }

        IAccountSettingsProvider Account { get; }
    }
}