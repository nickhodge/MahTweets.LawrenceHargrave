using System;
using System.Diagnostics;
using System.Windows.Controls;
using MahTweets.Core.Interfaces;

namespace MahTweets.Core.Media.Attachments
{
    /// <summary>
    /// Interaction logic for LinkAttachment.xaml
    /// </summary>
    public partial class LinkAttachmentView : UserControl
    {
        public LinkAttachmentView(LinkAttachment attachment)
        {
            InitializeComponent();

            ilLink.Text = attachment.Text;
            ilLink.Url = new Uri(attachment.Url);
            ilLink.MouseDown += (s, e) => { Process.Start(attachment.Url); };
        }
    }

    public class LinkAttachment : IStatusUpdateAttachment
    {
        public LinkAttachment(String Text, String Url)
        {
            this.Text = Text;
            this.Url = Url;
        }

        public String Text { get; set; }
        public String Url { get; set; }

        #region IStatusUpdateAttachment Members

        public UserControl NewView()
        {
            return new LinkAttachmentView(this);
        }

        public string MappedToUrl()
        {
            return Url;
        }

        #endregion
    }
}