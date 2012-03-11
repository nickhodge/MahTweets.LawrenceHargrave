using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MahTweets.Core.Collections;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.ViewModels
{
    public interface IMainViewModel : IContainerViewModel
    {
        IContactService ContactService { get; set; }

        ObservableCollection<IContainerViewModel> StreamContainers { get; set; }
        ObservableCollection<StreamModel> FilterGroups { get; set; }
        ThreadSafeObservableCollection<IMicroblog> SelectedMicroblogs { get; set; }
        ObservableCollection<ISearchViewModel> CurrentSearches { get; set; }

        void ShowSetup();
        void CreateNewSearch(SavedSearch savedSearch, IList<ISearchProvider> searchProviders);
        void CreateBlankStream();
        void CreateStream(StreamModel f);
        void CreateStream(StreamModel f, String title);
        void CreateStream(StreamModel f, String title, Double columnwidth);
        void ShowAbout();
        void SelectMicroblog(IMicroblog blog);
        void UnselectMicroblog(IMicroblog blog);
        void SelectDefaultMicroblogs();
    }
}