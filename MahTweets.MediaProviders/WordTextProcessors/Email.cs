using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Email : IWordTransfomProvider
    {
        private const string RgxPattern =
            @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";

        private readonly Regex _matcher;
        private readonly ITextProcessorEngine _textProcessorEngine;

        public Email()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _matcher = new Regex(RgxPattern, RegexOptions.IgnoreCase);
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "email"; }
        }

        public int Priority
        {
            get { return 2; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            MatchCollection mc = _matcher.Matches(word);
            if (mc.Count <= 0)
                return null;
            Uri ilurl;
            if (!Uri.TryCreate("mailto:" + mc[0].Value, UriKind.Absolute, out ilurl)) return null;
            var il = new InlineLink
                         {
                             Url = ilurl,
                             Text = mc[0].Value,
                             Foreground = lBrush,
                             ToolTip = "Send email to: " + mc[0].Value,
                             Tag = mc[0].Value,
                             HoverColour = _textProcessorEngine.BrHover,
                             NormalColour = _textProcessorEngine.BrBase
                         };
            il.MouseLeftButtonDown += _textProcessorEngine.LinkClick;
            return il;
        }

        #endregion
    }
}