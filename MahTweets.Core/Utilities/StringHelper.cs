using System.Text.RegularExpressions;
using System;

namespace MahTweets.Core.Utilities
{
    /// <summary>
    /// String related static methods
    /// </summary>
    public static class StringHelper
    {
        private static Regex rxURLPattern = new Regex(@"((https?|ftp)://)([\w#?=:&%\.\-~@\!\$|^\*\/\|\[\]]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static Regex rxURLPattern2 = new Regex(@"((http|ftp|https):\/\/)([\w\-_]+\.[\w\-_]+[\w\-\.,@?^=%&amp;\(\):/~\+#]*[\w\-\@?^=%&amp;/~\+\(\)#])?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static MatchCollection GetHyperlinks(string matchText)
        {
            return rxURLPattern2.Matches(matchText);
        }
        public static string StripHTML(this string text)
        {
            string filtered = text;
            filtered = Regex.Replace(text, @"<(.|\n)*?>", string.Empty); // Strip HTML Tags out;
            filtered = filtered.Replace("&apos;", "'"); // Stupid MSFT Bug. &apos; is missing from their entities table. 
            // TODO: fix this
            //filtered = HttpUtility.HtmlDecode(filtered); 
            return filtered;
        }

        public static Boolean ContainsHyperlink(string matchText)
        {
            return rxURLPattern2.IsMatch(matchText);
        }

        private static Regex rxTwit = new Regex("@([A-Za-z0-9_]*)", RegexOptions.Compiled & RegexOptions.Singleline);
        public static String GetTwit(this string text)
        {
            return rxTwit.Match(text).Groups[1].Value; ;
        }

        private static Regex rxSourceText = new Regex(@">(.*?)<", RegexOptions.Compiled & RegexOptions.Multiline);
        private static Regex rxSourceURL = new Regex("href=\"(.*?)\"", RegexOptions.Compiled & RegexOptions.Multiline);

        public static string GetSourceText(string Source)
        {
            if (string.IsNullOrEmpty(Source))
            {
                return String.Empty;
            }
            else if (rxSourceText.IsMatch(Source))
            {
                return rxSourceText.Match(Source).Groups[1].Value;
            }
            else
            {
                return Source;
            }
        }
        public static string GetSourceURL(string Source)
        {
            if (string.IsNullOrEmpty(Source))
            {
                return String.Empty;
            }
            else if (rxSourceURL.IsMatch(Source))
            {
                return rxSourceURL.Match(Source).Groups[1].Value;
            }
            else
            {
                return "http://twitter.com";
            }
        }
        public static bool IsShortUrl(this String source)
        {
            //HACK: Make this not suck so much.
            if (source.Contains("http://bit.ly/") || 
                source.Contains("http://cli.gs/") ||  
                source.Contains("http://is.gd/") || 
                source.Contains("http://j.mp/") ||  
                source.Contains("http://kl.am/") ||  
                source.Contains("http://su.pr/") ||  
                source.Contains("http://tinyurl.com/") ||   
                source.Contains("http://307.to/") ||  
                source.Contains("http://adjix.com/") || 
                source.Contains("http://b23.ru/") || 
                source.Contains("http://bacn.me/") || 
                source.Contains("http://bloat.me/") || 
                source.Contains("http://budurl.com/") ||
                source.Contains("http://clipurl.us/") || 
                source.Contains("http://cort.as/") ||
                source.Contains("http://dwarfurl.com/") || 
                source.Contains("http://ff.im/") || 
                source.Contains("http://fff.to/") || 
                source.Contains("http://href.in/") ||
                source.Contains("http://idek.net/") || 
                source.Contains("http://korta.nu/") || 
                source.Contains("http://lin.cr/") || 
                source.Contains("http://ln-s.net/") || 
                source.Contains("http://loopt.us/") || 
                source.Contains("http://lost.in/") || 
                source.Contains("http://memurl.com/") || 
                source.Contains("http://merky.de/") || 
                source.Contains("http://migre.me/") ||
                source.Contains("http://moourl.com/") || 
                source.Contains("http://nanourl.se/") || 
                source.Contains("http://ow.ly/") || 
                source.Contains("http://peaurl.com/") || 
                source.Contains("http://ping.fm/") || 
                source.Contains("http://piurl.com/") || 
                source.Contains("http://plurl.me/") || 
                source.Contains("http://pnt.me/") || 
                source.Contains("http://poprl.com/") || 
                source.Contains("http://post.ly/") || 
                source.Contains("http://rde.me/") || 
                source.Contains("http://reallytinyurl.com/") || 
                source.Contains("http://redir.ec/") ||
                source.Contains("http://retwt.me/") || 
                source.Contains("http://rubyurl.com/") || 
                source.Contains("http://short.ie/") || 
                source.Contains("http://short.to/") || 
                source.Contains("http://smallr.com/") || 
                source.Contains("http://sn.im/") ||
                source.Contains("http://sn.vc/") || 
                source.Contains("http://snipr.com/") || 
                source.Contains("http://snipurl.com/") ||
                source.Contains("http://snurl.com/") ||
                source.Contains("http://tiny.cc/") || 
                source.Contains("http://tinysong.com/") || 
                source.Contains("http://togoto.us/") || 
                source.Contains("http://tr.im/") || 
                source.Contains("http://tra.kz/") || 
                source.Contains("http://trg.li/") || 
                source.Contains("http://twurl.cc/") ||
                source.Contains("http://twurl.nl/") || 
                source.Contains("http://u.mavrev.com/") || 
                source.Contains("http://u.nu/") || 
                source.Contains("http://ur1.ca/") || 
                source.Contains("http://url.az/") || 
                source.Contains("http://url.ie/") || 
                source.Contains("http://urlx.ie/") || 
                source.Contains("http://w34.us/") || 
                source.Contains("http://xrl.us/") || 
                source.Contains("http://yep.it/") || 
                source.Contains("http://zi.ma/") || 
                source.Contains("http://zurl.ws/") || 
                source.Contains("http://chilp.it/") ||
                source.Contains("http://notlong.com/") || 
                source.Contains("http://qlnk.net/") || 
                source.Contains("http://trim.li"))
            {
                return true;
            }
            else return false;
        }

    }
}


