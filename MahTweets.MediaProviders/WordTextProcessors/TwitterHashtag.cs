using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class TwitterHashtag : IWordTransfomProvider
    {
        private const string RgxPattern = @"[\x22\'\“\:\;\/\.\,\-_rc\(\/\=\+]*#(\w+)[\x22\'\,\”\:\;\/\.\-_\(\/\=\+]*";
        private readonly Regex _matcher;
        private readonly ITextProcessorEngine _textProcessorEngine;

        public TwitterHashtag()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _matcher = new Regex(RgxPattern, RegexOptions.IgnoreCase);
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "TwitterHashtag"; }
        }

        public int Priority
        {
            get { return 4; }
        }

        public InlineLink Match(string word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            MatchCollection mc = _matcher.Matches(word);
            if (mc.Count <= 0)
                return null;
            var cm = new ContextMenu();
            var miIgnore = new MenuItem {Header = "Globally ignore " + mc[0].Value + " hashtag", Tag = mc[0].Value};
            cm.Items.Add(miIgnore);
            var il = new InlineLink
                         {
                             Text = mc[0].Value,
                             FontSize = _textProcessorEngine.FsDefault,
                             FontFamily = _textProcessorEngine.FfDefault,
                             Foreground = lBrush,
                             ToolTip = mc[0].Value,
                             Tag = mc[0].Value,
                             HoverColour = _textProcessorEngine.BrHover,
                             NormalColour = _textProcessorEngine.BrBase,
                             ContextMenu = cm
                         };
            il.ToolTip = "Search for " + word + " in your current tweet stream";
            il.MouseLeftButtonDown +=
                (s, e) => PluginEventHandler.FireEvent("searchHashtag", oStatusUpdate, mc[0].Value);
            miIgnore.Click += (s, e) => _textProcessorEngine.GlobalExcludeSettings.Add(word);
            return il;
        }

        #endregion
    }
}