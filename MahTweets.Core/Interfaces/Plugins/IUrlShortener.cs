using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public interface IUrlShortener : IPlugin
    {
        Task<string> Shorten(String Url);

        // bool HasSettings { get; }
    }
}