using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.ViewModels;

namespace MahTweets.Views
{
    public partial class ProfileView
    {
        public ProfileView()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
            }
            else
            {
                Loaded += ProfileViewLoaded;
            }
        }

        public new ProfileViewModel ViewModel
        {
            get { return DataContext as ProfileViewModel; }
        }

        private static void ProfileViewLoaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<MoveToEndStream>().Publish(null);
        }

        private void ProfileControlLayoutUpdated(object sender, EventArgs e)
        {
            double topSectionHeight = titleFill.ActualHeight;
            double midSectionHeight = bioContainer.ActualHeight;
            double footerSectionHeight = Footer.ActualHeight;

            double totalHeight = ProfileControl.ActualHeight;

            if (double.IsNaN(topSectionHeight)) return;
            if (double.IsNaN(midSectionHeight)) return;
            if (double.IsNaN(footerSectionHeight)) return;
            if (double.IsNaN(totalHeight)) return;

            double availableHeight = totalHeight - footerSectionHeight - midSectionHeight - topSectionHeight;

            svUpdates.Height = availableHeight;
        }


        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb == null) return;
            if (tb.Tag == null) return;

            var url = tb.Tag.ToString();
            if (string.IsNullOrWhiteSpace(url)) return;

            Task.Run(() => Process.Start(url));
        }
    }
}