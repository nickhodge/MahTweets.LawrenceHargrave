using System.Collections.Generic;
using System.ComponentModel.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public interface IStyle : IPlugin
    {
        IList<StyleVariant> ThemeSizes { get; set; }
        StyleVariant SelectedVariant { get; set; }
    }

    [InheritedExport]
    public interface ITheme : IPlugin
    {
        IShell Window { get; }
        IStyle Style { get; }
        string Description { get; }
    }
}