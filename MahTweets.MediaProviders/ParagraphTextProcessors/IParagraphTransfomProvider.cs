using System;
using System.ComponentModel.Composition;
using System.Windows.Documents;
using System.Windows.Media;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.TweetProcessors.ParagraphTextProcessors
{
    [InheritedExport]
    public interface IParagraphTransfomProvider
    {
        string Name { get; }
        int Priority { get; }
        Inline Match(String paragraph, Brush lBrush, IStatusUpdate oStatusUpdate);
    }
}