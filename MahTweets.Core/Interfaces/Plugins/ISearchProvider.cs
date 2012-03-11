using System.Collections.Generic;
using System.ComponentModel.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public interface ISearchProvider : IPlugin
    {
        bool IsAutomatic { get; }

        IEnumerable<string> SearchTerms { get; }

        void AddSearchTerm(string term);

        void RemoveSearchTerm(string term);

        void Start(IStatusUpdateService statusUpdateRepository);

        void Stop();

        void Refresh();
    }
}