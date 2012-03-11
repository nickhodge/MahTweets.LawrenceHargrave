using System;
using System.Windows;
using System.Windows.Controls;
using MahTweets.Core.Interfaces;

namespace MahTweets.Core.Media.Attachments
{
    /// <summary>
    /// Interaction logic for ImageMinimisedAttachment.xaml
    /// </summary>
    public partial class ImageMinimisedAttachmentView : UserControl
    {
        public ImageMinimisedAttachmentView(ImageMinimisedAttachment Source)
        {
            InitializeComponent();
            DataContext = Source;
        }

        private void showhide(object sender, RoutedEventArgs e)
        {
            if (imageContainer.Height != 0)
            {
                imageContainer.Height = 0;
                imageContainer.Width = 0;
                imgIconHelper.Text = "I";
            }
            else
            {
                if (imageContainer.Source == null) return;
                imageContainer.Width = 300;
                imageContainer.Height = Double.NaN;
                imgIconHelper.Text = ":";
            }
        }
    }

    public class ImageMinimisedAttachment : IStatusUpdateAttachment
    {
        public ImageMinimisedAttachment(String Source)
        {
            this.Source = Source;
            Url = Source;
        }

        public String Source { get; set; }
        public String Url { get; set; }
        public double MaxWidth { get; set; }
        public double MaxHeight { get; set; }

        #region IStatusUpdateAttachment Members

        public UserControl NewView()
        {
            return new ImageMinimisedAttachmentView(this);
        }

        public string MappedToUrl()
        {
            return Url;
        }

        #endregion
    }
}