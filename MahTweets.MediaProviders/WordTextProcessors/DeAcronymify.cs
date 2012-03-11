using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class DeAcronymify : IWordTransfomProvider
    {
        private static readonly Dictionary<string, string> _wordmap = new Dictionary<string, string>
                                                                          {
                                                                              {@"j/k", "just kidding"},
                                                                              {@"jsyk", "just so you know"},
                                                                              {@"bff", "best friends foreever"},
                                                                              {@"h/t", "hat tip"},
                                                                              {@"tl;dr", "too long, didn't read"},
                                                                              {@"btw", "by the way"},
                                                                              {@"ianal", "I am not a lawyer"},
                                                                              {@"imo", "in my opinion"},
                                                                              {@"imho", "in my humble opinion"},
                                                                              {@">^..^<","KITTEH!!1!"},
                                                                              {@"bil","brother in law"},
                                                                              {@"iirc","if I recall correctly"},
                                                                              {@"afaict","as far as I can tell"},
                                                                          };
                                                           // mapping acronyms to a phrase; just the more obscure ones. keeping away from the sweary & well known ones like GTFO

        private readonly ITextProcessorEngine _textProcessorEngine;


        public DeAcronymify()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "DeAcronymify"; }
        }

        public int Priority
        {
            get { return 100; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            if (word == null) return null;
            bool result = _wordmap.AsParallel().Any(p => p.Key == word.ToLower());
            if (!result) return null;

            var il = new InlineLink
                         {
                             FontFamily = _textProcessorEngine.FfDefault,
                             FontSize = _textProcessorEngine.FsDefault,
                             Foreground = _textProcessorEngine.BrText,
                             FontStyle = FontStyles.Italic,
                             Text = _wordmap[word.ToLower()],
                             ToolTip = word
                         };
            return il;
        }

        #endregion
    }
}