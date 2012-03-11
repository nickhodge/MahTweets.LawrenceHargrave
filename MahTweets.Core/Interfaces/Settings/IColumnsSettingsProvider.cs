using System.Collections.Generic;
using System.ComponentModel.Composition;
using MahTweets.Core.Factory;
using MahTweets.Core.Filters;

namespace MahTweets.Core.Interfaces.Settings
{
    [InheritedExport]
    public interface IColumnsSettingsProvider
    {
        IList<StreamModel> Filters { get; }

        IList<ColumnConfiguration> Columns { get; }

        void Save();

        void Reset();
    }
}