using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public interface IStatusHandler : IPlugin
    {
        bool Enabled { get; set; }
        void HandleUpdates(IEnumerable<IStatusUpdate> StatusUpdates);
        void HandleUpdate(IStatusUpdate StatusUpdate);
    }
}