using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Media;
using MahTweets.TweetProcessors.AdditionalSmarts;
using MahTweets.TweetProcessors.ParagraphTextProcessors;
using MahTweets.TweetProcessors.WordTextProcessors;

namespace MahTweets.TweetProcessors
{
    public class TextProcessorEngine : ITextProcessorEngine
    {
        private bool _setup;

        #region ITextProcessorEngine Members

        public IEnumerable<IWordTransfomProvider> WordProcessors { get; private set; }
        public IEnumerable<IParagraphTransfomProvider> SentenceProcessors { get; private set; }
        public IEnumerable<IMediaProvider> MediaProcessors { get; private set; }
        public IUrlExpandService UrlExpanders { get; private set; }
        public IApplicationSettingsProvider ApplicationSettings { get; private set; }
        public IEnumerable<IAdditonalTextSmarts> AdditonalTextSmarts { get; private set; }

        public Brush BrHover { get; private set; }
        public Brush BrBase { get; private set; }
        public Brush BrText { get; private set; }

        public Brush BrNormal
        {
            get { return BrBase; }
        }

        public Brush BrLink
        {
            get { return BrBase; }
        }

        public Brush BrTransparent { get; private set; }
        public Brush BrForeground { get; private set; }
        public double FsDefault { get; private set; }
        public FontFamily FfWebsymbol { get; private set; }
        public FontFamily FfUisymbol { get; private set; }
        public FontFamily FfEmoticon { get; private set; }
        public FontFamily FfDefault { get; private set; }

        public void SetupElementCaches(FrameworkElement uie)
        {
            if (_setup) return;

            FfDefault = (FontFamily) uie.FindResource("DefaultFont"); //get & cache
            FfEmoticon = (FontFamily) uie.FindResource("Emoticon"); //get & cache
            FfUisymbol = (FontFamily) uie.FindResource("DefaultSymbol"); //get & cache
            FfWebsymbol = (FontFamily) uie.FindResource("WebSymbols"); //get & cache

            FsDefault = (double) uie.FindResource("DefaultFontSize"); //get & cache

            BrForeground = (Brush) uie.FindResource("BaseColour"); //get & cache
            BrHover = (Brush) uie.FindResource("HoverColour"); //get & cache
            BrBase = (Brush) uie.FindResource("BaseColour"); //get & cache
            BrText = (Brush) uie.FindResource("TextColour"); //get & cache
            BrTransparent = (Brush) uie.FindResource("TransparentColour"); //get & cache

            WordProcessors = CompositionManager.GetAll<IWordTransfomProvider>().OrderBy(tp => tp.Priority);
                //get & cache
            SentenceProcessors = CompositionManager.GetAll<IParagraphTransfomProvider>().OrderBy(tp => tp.Priority);
                //get & cache
            MediaProcessors = CompositionManager.GetAll<IMediaProvider>(); //get & cache
            UrlExpanders = CompositionManager.Get<IUrlExpandService>(); //get & cache
            ApplicationSettings = CompositionManager.Get<IApplicationSettingsProvider>(); //get & cache
            AdditonalTextSmarts = CompositionManager.GetAll<IAdditonalTextSmarts>(); //get & cache

            _setup = true;
        }

        public string ShortenVisualInlineUrl(string expanded)
        {
            var r = new Regex(@"([\w\-_]+\.[\w\-_]+[\w\-\.,@?^=%&amp;\(\):~\+#]*)");
            if (expanded == null) return null;
            Match match = r.Match(expanded);
            return match.Groups[1].Value;
        }

        public InlineLink WebSymbolHelper(string character, string tooltip)
        {
            return new InlineLink
                       {
                           FontFamily = FfWebsymbol,
                           FontSize = FsDefault,
                           Foreground = BrText,
                           HoverColour = BrText,
                           NormalColour = BrText,
                           Text = character,
                           ToolTip = tooltip,
                       };
        }

        public async void LinkClick(object sender, MouseButtonEventArgs e)
        {
            var link = sender as InlineLink;
            if (link == null) return;

            string url = link.Url.ToString();
            if (String.IsNullOrWhiteSpace(url)) return;

            await StartProcess(url);
        }

        #endregion

        private static Task<bool> StartProcess(string url)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                tcs.SetResult(true);
                Process.Start(url);
            }
            catch (Exception ex)
            {
                tcs.SetResult(false);
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            return tcs.Task;
        }
    }
}