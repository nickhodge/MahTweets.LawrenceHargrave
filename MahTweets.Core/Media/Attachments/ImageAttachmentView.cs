namespace MahTweets.Core.Media.Attachments
{
    /// <summary>
    /// Interaction logic for ImageAttachment.xaml
    /// </summary>
    public partial class ImageAttachmentView
    {
        public ImageAttachmentView(ImageAttachment Source)
        {
            InitializeComponent();

            DataContext = Source;

            if (Source.MaxHeight <= 1) return;
            MaxHeight = Source.MaxHeight;
            MaxWidth = Source.MaxWidth;
        }
    }
}