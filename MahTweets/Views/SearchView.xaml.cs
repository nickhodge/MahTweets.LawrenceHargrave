using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.ViewModels;

namespace MahTweets.Views
{
    public partial class SearchView
    {
        public SearchView()
        {
            InitializeComponent();
            Loaded += SearchViewLoaded;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
            }
            else
            {
                Loaded += StreamViewLoaded;
            }
        }

        public new SearchViewModel ViewModel
        {
            get { return DataContext as SearchViewModel; }
        }

        private static void StreamViewLoaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<MoveToEndStream>().Publish(null);
        }

        private void SearchViewLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SearchProviders == null || ViewModel.SearchProviders.Count == 0)
                VisualStateManager.GoToState(this, "Edit", true);
            else
            {
                ViewModel.Search();
                VisualStateManager.GoToState(this, "View", true);
                grdEdit.Visibility = Visibility.Collapsed;
                svUpdates.Visibility = Visibility.Visible;
            }
        }

        private void BtnSearchClick(object sender, RoutedEventArgs e)
        {
            if (lstSearchProviders.SelectedItems != null && lstSearchProviders.SelectedItems.Count > 0)
            {
                foreach (object item in lstSearchProviders.SelectedItems)
                {
                    var provider = item as ISearchProvider;
                    if (provider == null)
                        continue;

                    provider.AddSearchTerm(txtSearchTerm.Text);
                    ViewModel.SearchProviders.Add(provider);
                }

                ViewModel.Search();
                VisualStateManager.GoToState(this, "View", false);
                grdEdit.Visibility = Visibility.Collapsed;
                svUpdates.Visibility = Visibility.Visible;
            }
        }

        private void txtSearchTerm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (lstSearchProviders.SelectedItems == null || lstSearchProviders.SelectedItems.Count == 0)
                    lstSearchProviders.SelectedItem = lstSearchProviders.Items[0];

                BtnSearchClick(null, null);
            }
        }
    }
}