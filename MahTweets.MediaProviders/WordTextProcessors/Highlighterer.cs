using System;
using System.Collections.Generic;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Highlighterer : IWordTransfomProvider
    {
        private readonly Dictionary<string, string> _backgroundmap;
        private readonly ITextProcessorEngine _textProcessorEngine;

        public Highlighterer()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _backgroundmap = new Dictionary<string, string>
                                 {
                                     {@"mahtweets", "HighlightBackgroundColour"},
                                     {@"html5", "HighlightBackgroundColour"},
                                 }; // mapping characters in the word to a single Font Family
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "Highlighterer"; }
        }

        public int Priority
        {
            get { return 100; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            foreach (var k in _backgroundmap)
            {
                if (!word.ToLower().Contains(k.Key.ToLower())) continue;
                var il = new InlineLink
                             {
                                 FontFamily = _textProcessorEngine.FfDefault,
                                 FontSize = _textProcessorEngine.FsDefault,
                                 Foreground = _textProcessorEngine.BrText,
                                 Text = word
                             };
                il.Background = (Brush) il.FindResource(k.Value);
                return il;
            }
            return null;
        }

        #endregion
    }
}