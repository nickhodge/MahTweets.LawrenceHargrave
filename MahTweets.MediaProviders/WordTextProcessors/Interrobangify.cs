using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Interrobangify : IWordTransfomProvider
    {
        private readonly Dictionary<string, string> _symbolmap = new Dictionary<string, string>
                                                                     {
                                                                         {@"interrobang", "\u203D"},
                                                                         {@"--", "\u2014"},
                                                                         {@"(c)", "\u00A9"},
                                                                         {@"(r)", "\u00AE"},
                                                                         {@"+-", "\u00B1"},
                                                                         {@"-+", "\u00B1"},
                                                                         {@"1/3", "\u2153"},
                                                                         {@"2/3", "\u2154"},
                                                                         {@"1/8", "\u215B"},
                                                                         {@"3/8", "\u215C"},
                                                                         {@"5/8", "\u215D"},
                                                                         {@"7/8", "\u215E"}
                                                                     };
                                                    // mapping characters in the word to a single Font Family

        private readonly ITextProcessorEngine _textProcessorEngine;


        public Interrobangify()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "Interrobangify"; }
        }

        public int Priority
        {
            get { return 1; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            if (word == null) return null;
            bool result = _symbolmap.AsParallel().Any(p => p.Key == word.ToLower());
            if (!result) return null;

            var il = new InlineLink
                         {
                             FontFamily = _textProcessorEngine.FfDefault,
                             FontSize = _textProcessorEngine.FsDefault,
                             Foreground = _textProcessorEngine.BrText,
                             HoverColour = _textProcessorEngine.BrText,
                             NormalColour = _textProcessorEngine.BrText,
                             Text = _symbolmap[word.ToLower()],
                             ToolTip = word
                         };
            return il;
        }

        #endregion
    }
}