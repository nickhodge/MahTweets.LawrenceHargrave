using System;
using System.Windows;
using MahTweets.UI.Controls;

namespace MahTweets.TwitterPlugin.UI
{
    /// <summary>
    /// Interaction logic for PINVerifierWindow.xaml
    /// </summary>
    public partial class PINVerifierWindow : GlassWindow
    {
        public String PIN { get; set;  }
        public PINVerifierWindow()
        {
            InitializeComponent();
            txtPIN.Focus();
        }

        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            if (isNumeric(txtPIN.Text, System.Globalization.NumberStyles.Integer))
            {
                PIN = txtPIN.Text.Trim();
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("It appears there are some invalid characters in the PIN!");
            }
        }

        public bool isNumeric(string val, System.Globalization.NumberStyles NumberStyle)
        {
            Double result;
            return Double.TryParse(val, NumberStyle,
                                   System.Globalization.CultureInfo.CurrentCulture, out result);
        }
    }
}


