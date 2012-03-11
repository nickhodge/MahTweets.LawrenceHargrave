using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Emoticons : IWordTransfomProvider
    {
        private static ITextProcessorEngine _textProcessorEngine;

        //TODO sort these emoticons into some sensible order!
        private static readonly Dictionary<string, string> _emoticons = new Dictionary<string, string>
                                                                            {
                                                                                {@":-)", ")"},
                                                                                {@":o)", ")"},
                                                                                {@":)", ")"},
                                                                                {@"(:", ")"},
                                                                                {@"=)", ")"},
                                                                                {@"c:", ")"},
                                                                                {@"c;", ";"},
                                                                                {@":>", ")"},
                                                                                {@":->", ")"},
                                                                                {@"C:", ")"},
                                                                                {@"C;", ";"},
                                                                                {";_;", ")"},
                                                                                {@":-(", "("},
                                                                                {@":(", "("},
                                                                                {@"=(", "("},
                                                                                {@":-<", "("},
                                                                                {@":<", "("},
                                                                                {@":c", "("},
                                                                                {@":C", "("},
                                                                                {@":-\", "\u002f"},
                                                                                {@":\", "\u002f"},
                                                                                {@":-/", "G"},
                                                                                {@":/", "G"},
                                                                                {@":-o", "o"},
                                                                                {@":o", "o"},
                                                                                {@":-O", "o"},
                                                                                {@":O", "o"},
                                                                                {@";-)", ";"},
                                                                                {@";)", ";"},
                                                                                {@";->", ";"},
                                                                                {@";>", ";"},
                                                                                {@";-o", "o"},
                                                                                {@";o", "o"},
                                                                                {@";-O", "o"},
                                                                                {@";O", "o"},
                                                                                {@";-0", "o"},
                                                                                {@";0", "o"},
                                                                                {@";-(", "I"},
                                                                                {@";(", "I"},
                                                                                {@";c", "I"},
                                                                                {@";C", "I"},
                                                                                {@";-|", "J"},
                                                                                {@";|", "J"},
                                                                                {@";-D", "K"},
                                                                                {@";D", "K"},
                                                                                {@":-|", "C"},
                                                                                {@":|", "C"},
                                                                                {@":-D", "D"},
                                                                                {@":D", "D"},
                                                                                {@":-P", "E"},
                                                                                {@":P", "E"},
                                                                                {@";-P", "L"},
                                                                                {@";P", "L"},
                                                                                {@":-p", "E"},
                                                                                {@":p", "E"},
                                                                                {@";-p", "L"},
                                                                                {@";p", "L"},
                                                                                {@"=P", "E"},
                                                                                {@"O_o", "="},
                                                                                {@"O.o", "="},
                                                                                {@"0_o", "="},
                                                                                {@">:[", "\u0034"},
                                                                                {@">:(", "\u0033"},
                                                                                {@">[", "\u0034"},
                                                                                {@">:S", "?"},
                                                                                {@"o_O", "U"},
                                                                                {@"o_0", "U"},
                                                                                {@"0_O", "U"},
                                                                                {@"O_O", "g"},
                                                                                {@"0_0", "U"},
                                                                                {@">:{", "\u0031"},
                                                                                {@"]:-)", "\u0039"},
                                                                                {@"B-)", "B"},
                                                                                {@"B)", "B"},
                                                                                {@"S-p", "S"},
                                                                                {@"S-P", "S"},
                                                                                {@"XD", "\u0034"},
                                                                                {@">.<", "k"},
                                                                                {@">_<", "k"},
                                                                                {@">_>", "k"},
                                                                                {@"<_<", "k"},
                                                                                {@"^_^", "j"},
                                                                                {@":-S", "<"},
                                                                                {@":S", "<"},
                                                                                {@";-S", ">"},
                                                                                {@";S", ">"},
                                                                                {"-_-", "C"},
                                                                            };
                                                           // mapping characters in the word to a single glyph in the Emoticons font family;

        public Emoticons()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "emoticons"; }
        }

        public int Priority
        {
            get { return 10; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            if (word == null) return null;
            bool result = _emoticons.AsParallel().Any(p => p.Key == word);
            if (!result) return null;

            var il = new InlineLink
                         {
                             FontFamily = _textProcessorEngine.FfEmoticon,
                             FontSize = _textProcessorEngine.FsDefault*1.2,
                             Foreground = _textProcessorEngine.BrText,
                             HoverColour = _textProcessorEngine.BrText,
                             NormalColour = _textProcessorEngine.BrText,
                             Text = _emoticons[word],
                             ToolTip = word
                         };
            return il;
        }

        #endregion
    }
}