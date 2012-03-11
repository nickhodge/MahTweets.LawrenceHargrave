using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace MahTweets.Views
{
    public partial class AboutView
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void HyperlinkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(((Hyperlink) sender).Tag.ToString());
            }
            catch
            {
            }
        }
    }
}