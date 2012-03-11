using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using MahTweets.Core;
using MahTweets.Core.Interfaces;
using MahTweets2.Library;
//using String = MahTweets2.Library.Helpers.String;

namespace MahTweets2.MediaProviders
{
    public class ImageProvider : IMediaProvider
    {
        public String Name { get { return "Image"; } }
        public bool Match(string Url)
        {
            return (Url.ToLower().EndsWith(".jpg") || Url.ToLower().EndsWith(".png"));
        }
        public MediaObject Transform(string Url)
        {
            return new MediaObject() { Url = Url, MediaType = MediaType.Image, Provider = this };
        }
    }
    public class TwitpicProvider : IMediaProvider
    {
        public String Name { get { return "Image"; } }
        public bool Match(string Url)
        {
            return Url.Contains("http://twitpic.com/");
        }

        public MediaObject Transform(string Url)
        {
            //Fetch the Twitpic page
            WebClient wc = new WebClient();
            StreamReader sr = new StreamReader(wc.OpenRead(Url));
            String twitpic = sr.ReadToEnd();

            //Find the image uri via regex
            Match match = System.Text.RegularExpressions.Regex.Match(twitpic, "<img id=\"photo-display\" class=\"photo-large\" src=\"http:\\/\\/([a-zA-Z0-9\\._\\'\\/\\-\\?\\=\\#\\&\\%]*)\"");
            if (match.Success)
                return new MediaObject() { Url = "http://" + match.Groups[1].Value, MediaType = MediaType.Image, Provider = this };
            else
                return null;
        }
    }

    public class BlipProvider : IMediaProvider
    {
        public String Name { get { return "Blip"; } }
        public bool Match(string Url)
        {
            return Url.StartsWith("http://blip.fm/~");
        }

        public MediaObject Transform(string Url)
        {
            //Fetch the blip page
            WebClient wc = new WebClient();
            StreamReader sr = new StreamReader(wc.OpenRead(Url));
            String blipPage = sr.ReadToEnd();

            //Find Youtube, Imeem or Direct link to the song
            Match matchYoutube = Regex.Match(blipPage, "url: \"([A-Za-z0-9\\-]*)\", type: \"youtubeVideo\"");
            Match matchImeem = Regex.Match(blipPage, "url: \"([A-Za-z0-9\\-_]*)\", type: \"imeemSong\"");
            Match matchDirectUrl = Regex.Match(blipPage, "url: \"([\\w#?=:&%\\.\\-~@\\!\\$|^\\*\\/\\|\\[\\]\\(\\)]*)?\", type: \"songUrl\"");

            if (matchYoutube.Success)
            {
                String newUrl = "http://www.youtube.com/v/" + matchYoutube.Groups[1].Value;
                return new MediaObject() { Url = newUrl, MediaType = MediaType.Flash, Provider = this };
            }
            else if (matchImeem.Success)
            {
                return new MediaObject() { Url = String.Format("http://resources-p2.imeem.com/resources/versioned/192/flash/audio_player3.swf?isEmbed=1&autoStart=true&ak=dgH-z&r=offsite&gatewayUrl=http%3a%2f%2fwww.imeem.com%2famf%2f&pm=st&mids={0}", matchImeem.Groups[1].Value), MediaType = MediaType.Flash, Provider = this };
            }
            else if (matchDirectUrl.Success)
            {
                return new MediaObject() { Url = matchDirectUrl.Groups[1].Value, MediaType = MediaType.Audio, Provider = this };
            }
            return null;
        }
    }
    public class YoutubeProvider : IMediaProvider
    {
        public String Name { get { return "Youtube"; } }
        public bool Match(string Url)
        {
            if (Url.Contains("http://www.youtube.com/watch?v="))
                return true;
            return false;
        }

        public MediaObject Transform(string Url)
        {
            var youtube = Regex.Match(Url, "http://www\\.youtube\\.com/watch\\?v=([\\w\\-\\d]+)");
            if (youtube.Success)
            {
                return new MediaObject() { MediaType = MediaType.Flash, Provider = this, Url = "http://www.youtube.com/v/" + youtube.Groups[1].Value };
            }
            return null;

        }
    }
    public class FlickrProvider : IMediaProvider
    {
        public String Name { get { return "Flickr"; } }
        public bool Match(string Url)
        {
            if (Url.Contains("http://www.flickr.com/photos/") && !Url.EndsWith("/sets/"))
                return true;
            return false;
        }

        public MediaObject Transform(string Url)
        {
            WebClient wc = new WebClient();
            StreamReader sr = new StreamReader(wc.OpenRead(Url));
            String flickr = sr.ReadToEnd();

            Match matchVideo = System.Text.RegularExpressions.Regex.Match(flickr, "rel=\"video_src\" href=\"http:\\/\\/([a-zA-Z0-9\\._\\'\\/\\-\\?\\=\\#\\&\\%]*)\"");
            Match matchPicture = System.Text.RegularExpressions.Regex.Match(flickr, "rel=\"image_src\" href=\"http:\\/\\/([a-zA-Z0-9\\._\\'\\/\\-\\?\\=\\#\\&\\%]*)\"");

            if (matchVideo.Success)
            {
                return null;
                /*__dispatcher.Invoke(
                    (EmptyDelegate)delegate()
                    {
                        //Insert the image into the textblock
                        InlineImageControl iic = new InlineImageControl();
                        iic.Source = new Uri("http://" + matchPicture.Groups[1].Value, UriKind.RelativeOrAbsolute);
                        iic.OriginalPage = __uri;

                        TextPointer tp = __contentstart;
                        new InlineUIContainer(iic, tp);

                        ((InlineLink)sender).Tag = true;
                    });
                return;*/
            }
            else if (matchPicture.Success)
            {
                return new MediaObject() { MediaType = MediaType.Image, Provider = this, Url = "http://" + matchPicture.Groups[1].Value };
            }
            return null;
        }
    }
}
