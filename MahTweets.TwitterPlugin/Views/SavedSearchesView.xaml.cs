using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Twitter.Models;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.TwitterPlugin.Logic;

namespace MahTweets.TwitterPlugin.Views
{
    /// <summary>
    /// Interaction logic for SavedSearchesView.xaml
    /// </summary>
    public partial class SavedSearchesView
    {
        public SavedSearchesView(Twitter _twitter)
        {
            InitializeComponent();

            this._twitter = _twitter;
        }

        private Twitter _twitter { get; set; }

        public void SetSearches(List<SavedSearch> searcheses)
        {
            lstSearches.ItemsSource = searcheses;
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var s = ((TextBlock) sender).DataContext as SavedSearch;
            if (s != null) _twitter.Search(s.Query);

            var _eventAggregator = CompositionManager.Get<IEventAggregator>();
            _eventAggregator.GetEvent<CloseDialog>().Publish(this);
        }
    }
}