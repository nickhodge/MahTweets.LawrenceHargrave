using System;
using System.Collections.Generic;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class ComicSansify : IWordTransfomProvider
    {
        private readonly Dictionary<string, string> _fontmap;
        private readonly ITextProcessorEngine _textProcessorEngine;

        public ComicSansify()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _fontmap = new Dictionary<string, string>
                           {
                               {@"comicsans", "ComicSans"},
                           }; // mapping characters in the word to a single Font Family
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "comicsansify"; }
        }

        public int Priority
        {
            get { return 1; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            if (!_fontmap.ContainsKey(word.ToLower()))
                return null;
            var il = new InlineLink();
            il.FontFamily = (FontFamily) il.FindResource(_fontmap[word.ToLower()]);
            il.FontSize = _textProcessorEngine.FsDefault;
            il.Foreground = _textProcessorEngine.BrText;
            il.Text = word;
            il.ToolTip = "Why do people hate on Comic Sans?";
            return il;
        }

        #endregion
    }
}