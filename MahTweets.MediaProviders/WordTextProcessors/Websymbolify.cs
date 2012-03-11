using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Websymbolify : IWordTransfomProvider
    {
        private static readonly Dictionary<string, string> _websymbols = new Dictionary<string, string>
                                                                             {
                                                                                 {@"[pic]", "I"},
                                                                                 {@"[pict]", "I"},
                                                                                 {@"[picture]", "I"},
                                                                                 {@"[img]", "I"},
                                                                                 {@"[image]", "I"},
                                                                                 {@"[link]", "K"},
                                                                                 {@"[url]", "K"},
                                                                                 {@"[web]", "K"},
                                                                                 {@"[vid]", "M"},
                                                                                 {@"[video]", "M"},
                                                                                 {@"[mov]", "M"},
                                                                                 {@"[movie]", "M"},
                                                                                 {@"video:", "M"},
                                                                                 {@"[audio]", "d"},
                                                                                 {@"[sound]", "d"},
                                                                                 {@"[music]", "d"},
                                                                                 {@"audio:", "d"},
                                                                                 {@"[time]", "P"},
                                                                                 {@"[mail]", "@"},
                                                                                 {@"[email]", "@"},
                                                                                 {@"[location]", "?"},
                                                                                 {@"[loc]", "P"},
                                                                                 {@"[geo]", "P"},
                                                                                 {@"[geotag]", "P"},
                                                                             };

        private readonly ITextProcessorEngine _textProcessorEngine;

        // mapping characters in the word to a single glyph in the Websymbols font family

        public Websymbolify()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "websymbolify"; }
        }

        public int Priority
        {
            get { return 1; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            if (word == null) return null;
            bool result = _websymbols.AsParallel().Any(p => p.Key == word.ToLower());
            if (!result) return null;
            return _textProcessorEngine.WebSymbolHelper(_websymbols[word.ToLower()], word);
        }

        #endregion
    }
}