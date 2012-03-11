using System;
using System.Globalization;
using System.Windows;

namespace MahTweets.TwitterPlugin.Views
{
    public partial class PinVerifierView
    {
        public PinVerifierView()
        {
            InitializeComponent();
            txtPIN.Focus();
        }

        public string PIN { get; set; }

        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            if (isNumeric(txtPIN.Text, NumberStyles.Integer))
            {
                PIN = txtPIN.Text.Trim();
                ModalResult = true;
            }
            else
            {
                MessageBox.Show("It appears there are some invalid characters in the PIN!");
            }
        }

        public bool isNumeric(string val, NumberStyles NumberStyle)
        {
            Double result;
            return Double.TryParse(val, NumberStyle, CultureInfo.CurrentCulture, out result);
        }
    }
}