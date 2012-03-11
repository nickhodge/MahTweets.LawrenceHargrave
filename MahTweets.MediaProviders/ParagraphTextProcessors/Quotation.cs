using System;
using System.Windows.Documents;
using System.Windows.Media;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.TweetProcessors.ParagraphTextProcessors
{
    public class Quotation : IParagraphTransfomProvider
    {
        // private const string RgxPattern = @"\x22(.+)\x22";
        //private readonly Regex _matcher;
        //private readonly ITextProcessorEngine _textProcessorEngine;

        #region IParagraphTransfomProvider Members

        public string Name
        {
            get { return "quotation"; }
        }

        public int Priority
        {
            get { return 1; }
        }

        public Inline Match(String paragraph, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            return null;
        }

        #endregion
    }
}