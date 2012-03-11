using System.Windows;

namespace MahTweets
{
    public partial class IgnoreStartupURI
    {
        public IgnoreStartupURI()
        {
            InitializeComponent();

            if (Visibility == Visibility.Visible)
            {
                //Hide();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                //  Hide();
            }
        }

        public void LoadComplete()
        {
            Dispatcher.InvokeShutdown();
        }
    }
}