using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class Stylista : IWordTransfomProvider
    {
        //TODO this needs to be cleaned up

        private const string RgxPatternBold = @"\*(?<found>.+)\*";
        private const string RgxPatternItalic = @"\x22(?<found>.+)\x22";
        private const string RgxPatternHTML = @"\<(?<found>.+)\>";
        private const string RgxPatternUnderline = @"_(?<found>.+)_";
        private readonly ITextProcessorEngine _textProcessorEngine;

        public Stylista()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "Stylista"; }
        }

        public int Priority
        {
            get { return 300; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            Match mcb = Regex.Match(word, RgxPatternBold);
            Match mci = Regex.Match(word, RgxPatternItalic);
            Match mch = Regex.Match(word, RgxPatternHTML);
            Match mcu = Regex.Match(word, RgxPatternUnderline);

            if (mcb.Success)
            {
                var il = new InlineLink
                             {
                                 Text = mcb.Groups["found"].Value,
                                 ToolTip = word,
                                 FontFamily = _textProcessorEngine.FfDefault,
                                 FontSize = _textProcessorEngine.FsDefault,
                                 Foreground = _textProcessorEngine.BrText,
                                 HoverColour = _textProcessorEngine.BrText,
                                 NormalColour = _textProcessorEngine.BrText,
                                 FontWeight = FontWeights.Bold
                             };
                return il;
            }

            if (mci.Success)
            {
                var il = new InlineLink
                             {
                                 Text = "\u201C" + mci.Groups["found"].Value + "\u201D",
                                 ToolTip = word,
                                 FontFamily = _textProcessorEngine.FfDefault,
                                 FontSize = _textProcessorEngine.FsDefault,
                                 Foreground = _textProcessorEngine.BrText,
                                 HoverColour = _textProcessorEngine.BrText,
                                 NormalColour = _textProcessorEngine.BrText,
                                 FontStyle = FontStyles.Italic
                             };
                return il;
            }

            if (mch.Success)
            {
                var il = new InlineLink
                             {
                                 Text = "<" + mch.Groups["found"].Value + ">",
                                 ToolTip = word,
                                 FontFamily = _textProcessorEngine.FfDefault,
                                 FontSize = _textProcessorEngine.FsDefault,
                                 Foreground = _textProcessorEngine.BrText,
                                 HoverColour = _textProcessorEngine.BrText,
                                 NormalColour = _textProcessorEngine.BrText,
                                 FontStyle = FontStyles.Italic
                             };
                return il;
            }

            if (mcu.Success)
            {
                var il = new InlineLink
                             {
                                 Text = mcu.Groups["found"].Value,
                                 ToolTip = word,
                                 FontFamily = _textProcessorEngine.FfDefault,
                                 FontSize = _textProcessorEngine.FsDefault,
                                 Foreground = _textProcessorEngine.BrText,
                                 HoverColour = _textProcessorEngine.BrText,
                                 NormalColour = _textProcessorEngine.BrText,
                                 TextDecorations = TextDecorations.Underline
                             };
                return il;
            }
            return null;
        }

        #endregion
    }
}