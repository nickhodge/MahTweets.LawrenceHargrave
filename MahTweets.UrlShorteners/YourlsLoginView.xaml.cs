using System.Windows;
using MahTweets.Core;

namespace MahTweets.UrlShorteners
{
    /// <summary>
    /// Interaction logic for YourlsLoginView.xaml
    /// </summary>
    public partial class YourlsLoginView
    {
        private readonly Credential _credentials;


        public YourlsLoginView(Credential credentials)
        {
            InitializeComponent();

            if (credentials == null)
            {
                _credentials = new Credential
                                   {
                                       Protocol = "yourls"
                                   };
            }
            else
                _credentials = credentials;

            txtLogin.Text = _credentials.Username;
            txtPassword.Text = _credentials.Password;
            txtEndpoint.Text = _credentials.AccountName;
        }

        public Credential Credentials
        {
            get { return _credentials; }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _credentials.Username = txtLogin.Text;
            _credentials.Password = txtPassword.Text;
            _credentials.AccountName = txtEndpoint.Text;
            ModalResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ModalResult = false;
        }
    }
}