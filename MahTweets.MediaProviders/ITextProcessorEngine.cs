using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Media;
using MahTweets.TweetProcessors.AdditionalSmarts;
using MahTweets.TweetProcessors.ParagraphTextProcessors;
using MahTweets.TweetProcessors.WordTextProcessors;

namespace MahTweets.TweetProcessors
{
    public interface ITextProcessorEngine
    {
        IUrlExpandService UrlExpanders { get; }
        IApplicationSettingsProvider ApplicationSettings { get; }

        IEnumerable<IWordTransfomProvider> WordProcessors { get; }
        IEnumerable<IParagraphTransfomProvider> SentenceProcessors { get; }
        IEnumerable<IMediaProvider> MediaProcessors { get; }
        IEnumerable<IAdditonalTextSmarts> AdditonalTextSmarts { get; }
        IGlobalExcludeSettings GlobalExcludeSettings { get; }

        Brush BrHover { get; }
        Brush BrNormal { get; }
        Brush BrBase { get; }
        Brush BrLink { get; }
        Brush BrText { get; }
        Brush BrForeground { get; }
        Brush BrTransparent { get; }
        double FsDefault { get; }
        FontFamily FfWebsymbol { get; }
        FontFamily FfUisymbol { get; }
        FontFamily FfEmoticon { get; }
        FontFamily FfDefault { get; }
        string ShortenVisualInlineUrl(string expanded);
        void SetupElementCaches(FrameworkElement uie);
        void LinkClick(object sender, MouseButtonEventArgs e);
        InlineLink WebSymbolHelper(string character, string tooltip);
    }
}