using System;
using System.Windows.Controls;
using MahTweets.Core.Interfaces;

namespace MahTweets.Core.Media.Attachments
{
    public class ImageAttachment : IStatusUpdateAttachment
    {
        public ImageAttachment(String Source)
        {
            this.Source = Source;
            Url = Source;
        }

        public ImageAttachment(String Source, String Url, double MaxWidth = Double.NaN, double MaxHeight = Double.NaN)
        {
            this.Source = Source;
            this.Url = Url;
            this.MaxHeight = MaxHeight;
            this.MaxWidth = MaxWidth;
        }

        public String Source { get; set; }
        public String Url { get; set; }
        public double MaxWidth { get; set; }
        public double MaxHeight { get; set; }

        #region IStatusUpdateAttachment Members

        public UserControl NewView()
        {
            return new ImageAttachmentView(this);
        }

        public string MappedToUrl()
        {
            return Url;
        }

        #endregion
    }
}