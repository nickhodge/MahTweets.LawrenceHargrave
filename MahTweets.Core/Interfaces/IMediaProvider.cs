using System;
using System.ComponentModel.Composition;

namespace MahTweets.Core.Interfaces
{
    [InheritedExport]
    public interface IMediaProvider
    {
        String Name { get; }
        bool Match(String Url);
        MediaObject Transform(String Url);
    }
}


