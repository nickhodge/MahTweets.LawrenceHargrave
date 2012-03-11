using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Symbolify : IWordTransfomProvider
    {
        private static readonly Dictionary<string, string> _segoeuisymbolify = new Dictionary<string, string>
                                                                                   {
                                                                                       {@"<3", "\u2764"},
                                                                                       {@"[heart]", "\u2764"},
                                                                                       {@"#1", "\u278A"},
                                                                                       {@"#2", "\u278B"},
                                                                                       {@"#3", "\u278C"},
                                                                                       {@"#4", "\u278D"},
                                                                                       {@"#5", "\u278E"},
                                                                                       {@"#6", "\u278F"},
                                                                                       {@"#7", "\u2790"},
                                                                                       {@"#8", "\u2791"},
                                                                                       {@"#9", "\u2792"},
                                                                                       {@"#10", "\u2793"},
                                                                                       {@"1.", "\u278A"},
                                                                                       {@"2.", "\u278B"},
                                                                                       {@"3.", "\u278C"},
                                                                                       {@"4.", "\u278D"},
                                                                                       {@"5.", "\u278E"},
                                                                                       {@"6.", "\u278F"},
                                                                                       {@"7.", "\u2790"},
                                                                                       {@"8.", "\u2791"},
                                                                                       {@"9.", "\u2792"},
                                                                                       {@"10.", "\u2793"},
                                                                                       {@"1)", "\u278A"},
                                                                                       {@"2)", "\u278B"},
                                                                                       {@"3)", "\u278C"},
                                                                                       {@"4)", "\u278D"},
                                                                                       {@"5)", "\u278E"},
                                                                                       {@"6)", "\u278F"},
                                                                                       {@"7)", "\u2790"},
                                                                                       {@"8)", "\u2791"},
                                                                                       {@"9)", "\u2792"},
                                                                                       {@"10)", "\u2793"},
                                                                                       {@"->", "\u25B7"},
                                                                                       {@"-->", "\u25B7"},
                                                                                       {@"--->", "\u25B7"},
                                                                                       {@"[]", "\u25A1"},
                                                                                       {@"<-", "\u25C1"},
                                                                                       {@"<--", "\u25C1"},
                                                                                       {@"<---", "\u25C1"},
                                                                                       {@"<<", "\u226A"},
                                                                                       {@">>", "\u226B"},
                                                                                       {@"<<<", "\u22D8"},
                                                                                       {@">>>", "\u22D9"},
                                                                                       {@"=>", "\u21D2"},
                                                                                       {@"<=", "\u21D0"},
                                                                                       {@"==>", "\u21D2"},
                                                                                       {@"<==", "\u21D0"},
                                                                                       {@"1/2", "\u00BD"},
                                                                                       {@"1/4", "\u00BC"},
                                                                                       {@"3/4", "\u00BE"},
                                                                                       {@"rt", "\u267C"},
                                                                                       {@"rt:", "\u267C"},
                                                                                       {@"infinity", "\u221E"},
                                                                                       {@"degc", "\u2103"},
                                                                                       {@"(tm)", "\u2122"},
                                                                                       {@"[plink]", "\u2042"},
                                                                                       {
                                                                                           @"[redacted]",
                                                                                           "\u2588\u2588\u2588\u2588\u2588\u2588"
                                                                                           },
                                                                                   };
                                                           // mapping characters in the word to a single glyph in the Segoe UI Symbol font family

        private readonly ITextProcessorEngine _textProcessorEngine;


        public Symbolify()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "symbolify"; }
        }

        public int Priority
        {
            get { return 1; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            if (word == null) return null;
            bool result = _segoeuisymbolify.AsParallel().Any(p => p.Key == word.ToLower());
            if (!result) return null;

            var il = new InlineLink
                         {
                             FontFamily = _textProcessorEngine.FfUisymbol,
                             FontSize = _textProcessorEngine.FsDefault*1.25,
                             Foreground = _textProcessorEngine.BrText,
                             HoverColour = _textProcessorEngine.BrText,
                             NormalColour = _textProcessorEngine.BrText,
                             Text = _segoeuisymbolify[word.ToLower()],
                             ToolTip = word
                         };
            return il;
        }

        #endregion
    }
}