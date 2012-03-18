using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Highlighterer : IWordTransfomProvider
    {
        private readonly ITextProcessorEngine _textProcessorEngine;
        private readonly IHighlighterSettings _highlightWordSettingsProvider;

        public Highlighterer()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _highlightWordSettingsProvider = CompositionManager.Get<IHighlighterSettings>(); //get & cache
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
            if (_highlightWordSettingsProvider.HighlightWords.Any(k => word.ToLower().Contains(k.ToLower())))
            {
                var il = new InlineLink
                             {
                                 FontFamily = _textProcessorEngine.FfDefault,
                                 FontSize = _textProcessorEngine.FsDefault,
                                 Foreground = _textProcessorEngine.BrText,
                                 Text = word
                             };
                il.Background = (Brush)il.FindResource("HighlightBackgroundColour");
                return il;
            }
            return null;
        }

        #endregion
    }
}