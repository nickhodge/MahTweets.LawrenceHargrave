using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    [InheritedExport]
    public interface IWordTransfomProvider
    {
        string Name { get; }
        int Priority { get; }
        InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate);
    }
}