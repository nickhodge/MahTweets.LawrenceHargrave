using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.TweetProcessors.ParagraphTextProcessors
{
    public class HighlightMarker : IParagraphTransfomProvider
    {
        private readonly List<string> _highlightwords;
        private readonly ITextProcessorEngine _textProcessorEngine;

        public HighlightMarker()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _highlightwords = new List<string> {"html5", "nick hodge"};
        }

        #region IParagraphTransfomProvider Members

        public string Name
        {
            get { return "highlightmarker"; }
        }

        public int Priority
        {
            get { return 2; }
        }

        public Inline Match(String paragraph, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            foreach (string words in _highlightwords)
            {
                if (!paragraph.Contains(words)) continue;
                var sp = new string[1];
                sp[0] = words;
                string[] s = paragraph.Split(sp, 30, StringSplitOptions.None);
                    // 140 div 5 is about 30; 140 characters, 5 character word size average
                foreach (string s1 in s)
                {
                }
            }
            return null;
        }

        #endregion
    }
}