using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using MahTweets.Core.Collections;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.ViewModels;
using MahTweets.Services;

namespace MahTweets.ViewModels
{
    public class SearchViewModel : ContainerViewModel, ISearchViewModel
    {
        private Timer _timer;

        public SearchViewModel()
        {
            SearchProviders = new ObservableCollection<ISearchProvider>();
            Results = new StatusUpdateService();
            Title = "Search";
        }

        public SearchViewModel(IEnumerable<ISearchProvider> searchProviders) : this()
        {
            AvailableSearchProviders = new ThreadSafeObservableCollection<ISearchProvider>(searchProviders);
        }

        public virtual IStatusUpdateService Results { get; set; }

        public ThreadSafeObservableCollection<ISearchProvider> AvailableSearchProviders { get; set; }

        #region ISearchViewModel Members

        public ObservableCollection<ISearchProvider> SearchProviders { get; set; }

        public string SearchTerm
        {
            get
            {
                if (!SearchProviders.Any())
                    return string.Empty;

                IEnumerable<string> terms = SearchProviders.First().SearchTerms;

                // TODO: i think we need to look at SearchProviders and how they can integrate 
                // with userstreams - there's something odd here about how each provider can have different search values
                // which is making things somewhat complex

                return terms.First();
            }
        }

        #endregion

        private void OutgoingUpdates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // we have the first item
                IsLoading = false;
                Results.OutgoingUpdates.CollectionChanged -= OutgoingUpdates_CollectionChanged;
            }
        }

        public void Search()
        {
            IsLoading = true;

            Results.OutgoingUpdates.CollectionChanged += OutgoingUpdates_CollectionChanged;

            Title = SearchTerm;

            Task.Run(() => SearchProviders.ForEach(s => s.Start(Results)));
            RefreshProviders();

            _timer = new Timer {AutoReset = true, Interval = 1000*60*2};
            _timer.Elapsed += RefreshProviders;
            _timer.Start();
        }

        private void RefreshProviders(object sender, ElapsedEventArgs e)
        {
            RefreshProviders();
        }

        private void RefreshProviders()
        {
            Task.Run(() =>
                           {
                               foreach (ISearchProvider s in SearchProviders.Where(s => !s.IsAutomatic))
                               {
                                   s.Refresh();
                               }
                           });
        }

        public override void Close()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= RefreshProviders;
            }

            if (SearchProviders != null)
                foreach (ISearchProvider s in SearchProviders)
                    s.Stop();

            var model = (Parent as MainViewModel);
            if (model == null) return;
            model.RemoveContainer(this);
        }

        public void Clear()
        {
            Results.Reset();
        }
    }
}