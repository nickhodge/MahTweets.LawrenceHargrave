using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MahTweets.Core;

namespace MahTweets.UrlShorteners
{
    /// <summary>
    /// Interaction logic for BitlyLoginView.xaml
    /// </summary>
    public partial class BitlyLoginView
    {
        private readonly Credential _credentials;

        public BitlyLoginView(Credential credentials)
        {
            InitializeComponent();
            if (credentials == null)
            {
                _credentials = new Credential
                                   {
                                       Protocol = "bitly"
                                   };
            }
            else
                _credentials = credentials;

            txtLogin.Text = _credentials.Username;
            txtAPIKey.Text = _credentials.Password;
        }

        public Credential Credentials
        {
            get { return _credentials; }
        }


        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://bit.ly/a/account");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _credentials.Username = txtLogin.Text;
            _credentials.Password = txtAPIKey.Text;
            ModalResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ModalResult = false;
        }
    }
}