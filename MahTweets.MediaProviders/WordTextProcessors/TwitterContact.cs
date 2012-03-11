using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class TwitterContact : IWordTransfomProvider
    {
        /*
         * ".@" "-@" ":@" "r@" "cc@" "rt@" "(@" "\"@" "“@" "\\@" "/@" "+@" seen in the wild
         * beginning / hopefully zero, but any of the above prefixes, @-sign, 1 or more characters, but no 'dot'
         * want to keep the initial characters, append contact after at sign
         * */

        private const string RgxPattern = @"\@(\w+)";
        private readonly Regex _matcher;
        private readonly ITextProcessorEngine _textProcessorEngine;

        public TwitterContact()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _matcher = new Regex(RgxPattern, RegexOptions.IgnoreCase);
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "TwitterContact"; }
        }

        public int Priority
        {
            get { return 3; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            MatchCollection mc = _matcher.Matches(word);
            if (mc.Count <= 0)
            {
                return null;
            }
            var il = new InlineLink
                         {
                             Text = word,
                             Foreground = lBrush,
                             TextDecorations = null,
                             ToolTip = "View " + mc[0].Value + "'s profile",
                             HoverColour = _textProcessorEngine.BrHover,
                             NormalColour = _textProcessorEngine.BrBase
                         };
            il.MouseLeftButtonDown +=
                (s, e) => PluginEventHandler.FireEvent("profileByName", oStatusUpdate, mc[0].Value);
            return il;
        }

        #endregion
    }
}