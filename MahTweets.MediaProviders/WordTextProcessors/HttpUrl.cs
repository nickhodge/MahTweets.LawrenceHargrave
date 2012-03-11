using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;
using MahTweets.Core.Media.Attachments;
using MahTweets.TweetProcessors.AdditionalSmarts;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class HttpUrl : IWordTransfomProvider
    {
        private const string HttpUrlRgxPattern =
            @"(?<urlscheme>(http|ftp|https):\/\/)(?<domainpath>[\w\-_]+\.[\w\-_]+[\w\-\.,@!?^=%&amp;\(\):/~\+#]*[\w\-\@!?^=%&amp;/~\+\(\)#])?[\x22\.\”]?";

        private const string BrokenTweetdeckHttpUrlRgxPattern = @"^(?<tweetdecksucks>\w+\.\w+\/\w+)\Z";

        private const string Ellipsis = "\u2026";
                             // unicode for ellipsis or ... (three dots as a single character, not comment to be continued silly)

        private readonly ITextProcessorEngine _textProcessorEngine;

        public HttpUrl()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "httpurl"; }
        }

        public int Priority
        {
            get { return 3; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            string word1 = word;
            Match tweetdeckiskillingme = Regex.Match(word, BrokenTweetdeckHttpUrlRgxPattern);
            if (tweetdeckiskillingme.Success)
                word1 = "http://" + tweetdeckiskillingme.Groups["tweetdecksucks"].Value;
                    // pop http:// onto the front of the bastardised tweetdeck shorturl mechanism
            Match mpa = Regex.Match(word1, HttpUrlRgxPattern);
            if (mpa.Success)
            {
                string nmatched = mpa.Groups["urlscheme"].Value + mpa.Groups["domainpath"].Value;
                Uri ilurl;
                if (!Uri.TryCreate(nmatched, UriKind.Absolute, out ilurl)) return null;
                Uri ciurl;
                if (
                    !Uri.TryCreate(
                        "http://" + _textProcessorEngine.ShortenVisualInlineUrl(mpa.Groups["domainpath"].Value) +
                        "/favicon.ico", UriKind.Absolute, out ciurl)) return null;
                var image = new CachedImage
                                {
                                    Url = ciurl,
                                    MaxHeight = 10,
                                    MaxWidth = 10,
                                    Margin = new Thickness(0, 0, 2, 0)
                                };
                var il = new InlineLink
                             {
                                 Url = ilurl,
                                 Text = _textProcessorEngine.ShortenVisualInlineUrl(nmatched) + Ellipsis,
                                 Foreground = lBrush,
                                 ToolTip = "Browse to: " + nmatched,
                                 Tag = false,
                                 Image = image,
                                 HoverColour = _textProcessorEngine.BrHover,
                                 NormalColour = _textProcessorEngine.BrNormal,
                             };
                il.MouseLeftButtonDown += _textProcessorEngine.LinkClick;

                AddImagePreviewIcon(oStatusUpdate, il, nmatched);

                AddAdditionalSmartText(il, nmatched);

                CheckAndConvertToLongUrl(oStatusUpdate, il, nmatched);

                return il;
            }
            return null;
        }

        #endregion

        private async void CheckAndConvertToLongUrl(IStatusUpdate oStatusUpdate, InlineLink il, string nmatched)
        {
            if (!_textProcessorEngine.ApplicationSettings.AutoExpandUrls ||
                !_textProcessorEngine.UrlExpanders.IsShortUrl(nmatched)) return;
            //TODO: cleanup the multiple passes, manual vs. web service longurl checker

            string tmatched = nmatched;
            //await CheckAndConvertToManualLongUrl(nmatched); // returns nmatched the same if its not a manual url lengthener
            //tmatched = await _textProcessorEngine.UrlExpanders.ExpandUrl(tmatched);
            //if (tmatched == null) return;

            if (_textProcessorEngine.UrlExpanders.IsShortUrl(tmatched))
                // go one more round to unshortent the url; useful when t.co shortens an existing shortened link
                tmatched = await _textProcessorEngine.UrlExpanders.ExpandUrl(tmatched);
            if (tmatched == null) tmatched = nmatched;

            //tmatched = await CheckAndConvertToManualLongUrl(nmatched); // and go around again

            il.Text = _textProcessorEngine.ShortenVisualInlineUrl(tmatched);
            Uri url;
            if (!Uri.TryCreate("http://" + il.Text + "/favicon.ico", UriKind.Absolute, out url)) return;
            var iimage = new CachedImage
                             {
                                 Url = url,
                                 MaxHeight = _textProcessorEngine.FsDefault,
                                 MaxWidth = _textProcessorEngine.FsDefault,
                                 Margin = new Thickness(0, 0, 2, 0)
                             };
            il.Text = il.Text + Ellipsis;
            il.Image = iimage;
            AddAdditionalSmartText(il, tmatched);
            AddImagePreviewIcon(oStatusUpdate, il, tmatched);
        }

        private static async Task<string> CheckAndConvertToManualLongUrl(string nmatched)
        {
            string tmatched = nmatched;
            var manual = CompositionManager.Get<IManualLongUrlRetrieverService>();
            if (manual != null)
            {
                if (manual.IsShortUrl(nmatched))
                {
                    string g = await manual.ExpandUrl(nmatched);
                    if (g == null) return nmatched;
                    tmatched = g;
                }
            }
            return tmatched;
        }

        private async void AddAdditionalSmartText(InlineLink il, string f)
        {
            foreach (
                IAdditonalTextSmarts textSmarts in
                    _textProcessorEngine.AdditonalTextSmarts.Where(textSmarts => textSmarts.CanPageTitle(f)))
            {
                string title = await textSmarts.Process(f);
                if (title == null) continue;
                var ilt = new InlineLink
                              {
                                  Text = "\u201C" + title + "\u201D ",
                                  FontFamily = _textProcessorEngine.FfDefault,
                                  FontSize = _textProcessorEngine.FsDefault,
                                  Foreground = _textProcessorEngine.BrText,
                                  Background = (Brush) il.FindResource("LighterGreyColour"),
                                  HoverColour = _textProcessorEngine.BrText,
                                  NormalColour = _textProcessorEngine.BrText,
                                  FontStyle = FontStyles.Italic,
                              };
                il.Inlines.Add(_textProcessorEngine.WebSymbolHelper(" \u005D", null));
                il.Inlines.Add(ilt);
            }
        }

        public async void AddImagePreviewIcon(IStatusUpdate oStatusUpdate, InlineLink il, string matchedurl)
        {
            if (_textProcessorEngine.ApplicationSettings.DisableMediaProviders) return;
            if ((il.Tag is bool && (bool) il.Tag)) return;
            foreach (
                IMediaProvider mediaProvider in
                    _textProcessorEngine.MediaProcessors.Where(mediaProvider => mediaProvider.Match(matchedurl)))
            {
                string source = await mediaProvider.Transform(matchedurl);

                if (source == null) continue;
                il.Tag = true;
                var ili = new ImageMinimisedAttachment(source);
                il.Inlines.Add(new ImageMinimisedAttachmentView(ili));
            }
        }
    }
}