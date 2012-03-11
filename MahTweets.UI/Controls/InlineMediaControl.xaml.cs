using System;
using System.Windows;
using System.Windows.Documents;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.UI.Controls
{
    public partial class InlineMediaControl
    {
        public InlineMediaControl()
        {
            InitializeComponent();
        }

        public InlineMediaControl(Uri url) : this()
        {
            CachedImage.Url = url;
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var iuc = Parent as InlineUIContainer;
                if (iuc == null) return;

                var paragraph = iuc.Parent as Paragraph;
                if (paragraph == null) return;

                paragraph.Inlines.Remove(iuc);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }
    }
}