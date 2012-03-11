using System.ComponentModel.Composition;
using System.Windows.Controls;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Settings
{
    [InheritedExport]
    public class SettingsUserControl : UserControl
    {
        public virtual void Load(IPlugin t)
        {
        }

        public virtual void Save(IPlugin t)
        {
        }
    }
}