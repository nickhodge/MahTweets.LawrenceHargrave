using System;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.TweetProcessors.ParagraphTextProcessors
{
    public class MultipersonTweet : IParagraphTransfomProvider
    {
        /*
         * Seen this on a couple accounts: using caret then text to indicate the name of a person tweeting from a 'group' account eg:@MSAU
         */

        private const string RgxPattern = @"\^(?<found>\w+)$";
        private readonly ITextProcessorEngine _textProcessorEngine;

        public MultipersonTweet()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IParagraphTransfomProvider Members

        public string Name
        {
            get { return "MultipersonTweet"; }
        }

        public int Priority
        {
            get { return 400; }
        }

        public Inline Match(String paragraph, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            Match mpa = Regex.Match(paragraph, RgxPattern);

            if (mpa.Success)
            {
            }
            return null;
        }

        #endregion
    }
}