using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using MahTweets2.Library.Gui.Controls;

namespace MahTweets2.UrlShorteners
{
    /// <summary>
    /// Interaction logic for BitlySetupWindow.xaml
    /// </summary>
    public partial class BitlySetupWindow : GlassWindow
    {
        public String APIKey { get; set; }
        public String Login { get; set; }
        public BitlySetupWindow()
        {
            InitializeComponent();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            APIKey = txtAPIKey.Text;
            Login = txtLogin.Text;
            this.DialogResult = true;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	System.Diagnostics.Process.Start("http://bit.ly/account/your_api_key/");
        }
    }
}
