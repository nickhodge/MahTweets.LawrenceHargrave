using System;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;

namespace MahTweets.Core.Extensions
{
    /// <summary>
    /// String related static methods
    /// </summary>
    public static class StringExtensions
    {
        //private static Regex _rxUrlPattern = new Regex(@"((https?|ftp)://)([\w#?=:&%\.\-~@\!\$|^\*\/\|\[\]]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex RxURLPattern2 =
            new Regex(
                @"((http|ftp|https):\/\/)([\w\-_]+\.[\w\-_]+[\w\-\.,@!?^=%&amp;\(\):/~\+#]*[\w\-\@!?^=%&amp;/~\+\(\)#])?",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex RsURLPatternTD = new Regex(@"^bit.ly/((.)+)",
                                                                 RegexOptions.Compiled | RegexOptions.IgnoreCase |
                                                                 RegexOptions.Multiline);

        private static readonly Regex RxTwit = new Regex("@([A-Za-z0-9_]*)",
                                                         RegexOptions.Compiled & RegexOptions.Singleline);

        private static readonly Regex RxSourceText = new Regex(@">(.*?)<",
                                                               RegexOptions.Compiled & RegexOptions.Multiline);

        private static readonly Regex RxSourceURL = new Regex("href=\"(.*?)\"",
                                                              RegexOptions.Compiled & RegexOptions.Multiline);

        public static bool Matches(this string s, string t, bool matchCase)
        {
            return string.Compare(s, t, matchCase) == 0;
        }

        public static bool Matches(this string s, string t)
        {
            return string.CompareOrdinal(s, t) == 0;
        }

        public static MatchCollection GetHyperlinks(this string matchText)
        {
            return RxURLPattern2.Matches(matchText);
        }

        public static string StripHTML(this string text)
        {
            if (String.IsNullOrEmpty(text))
                return null;

            string filtered = Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
            filtered = filtered.Replace("&apos;", "'");
                // Stupid MSFT Bug. &apos; is missing from their entities table. 
            filtered = WebUtility.HtmlDecode(filtered);
            return filtered;
        }

        public static bool ContainsHyperlink(this string matchText)
        {
            return RxURLPattern2.IsMatch(matchText);
        }

        public static bool ContainsBastardisedTweetdeckURL(this string matchText)
        {
            return RsURLPatternTD.IsMatch(matchText);
        }

        public static string GetTwit(this string text)
        {
            return RxTwit.Match(text).Groups[1].Value;
        }

        public static string GetSourceText(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return String.Empty;
            }
            if (RxSourceText.IsMatch(source))
            {
                return RxSourceText.Match(source).Groups[1].Value;
            }
            return source;
        }

        public static string GetSourceURL(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return String.Empty;
            }
            if (RxSourceURL.IsMatch(source))
            {
                return RxSourceURL.Match(source).Groups[1].Value;
            }
            return "http://twitter.com";
        }

        public static string EscapeXml(this string s)
        {
            string inputText = SecurityElement.IsValidText(s) ? s : SecurityElement.Escape(s);
            return string.Format("<![CDATA[{0}]]>", inputText);
        }

        public static string UnescapeXml(this string s)
        {
            return s.Replace("<![CDATA[", string.Empty).Replace("]]>", string.Empty);
        }
    }
}