using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace MahTweets.Core.Media
{
    [InheritedExport]
    public interface IMediaProvider
    {
        string Name { get; }
        bool Match(string url);
        Task<string> Transform(String url);
    }
}