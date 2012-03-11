using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;

namespace MahTweets.Core.VersionCheck
{
    /// <summary>
    /// Interaction logic for UpdateNotification.xaml
    /// </summary>
    public partial class UpdateNotification : UserControl
    {
        public UpdateNotification(string message, string version, string updateUrl,string infourl)
        {
            InitializeComponent();

            MessageText = message;
            VersionText = version;
            UpdateUrl = updateUrl;
            InfoUrl = infourl;

            DataContext = this;

            _eventAggregator = CompositionManager.Get<IEventAggregator>();
        }

        public string MessageText { get; set; }
        public string VersionText { get; set; }
        public string UpdateUrl { get; set; }
        public string InfoUrl { get; set; }
        private IEventAggregator _eventAggregator { get; set; }

        private void update_Click(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<HideNotification>().Publish(new HideNotificationPayload(this));
            Process.Start(UpdateUrl);
        }

        private void getinfo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(InfoUrl);
        }

    }
}
