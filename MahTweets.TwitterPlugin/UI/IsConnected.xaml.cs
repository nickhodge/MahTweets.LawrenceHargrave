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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahTweets2.Library;
using System.Threading;

namespace MahTweets2.TwitterPlugin
{
    /// <summary>
    /// Interaction logic for IsConnected.xaml
    /// </summary>
    public partial class IsConnected : UserControl
    {
        public IsConnected()
        {
            InitializeComponent();
        }

        private delegate void EmptyDelegate();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((EmptyDelegate)delegate() { (this.DataContext as Twitter).Refresh(true); }, null);
        }
    }
}
