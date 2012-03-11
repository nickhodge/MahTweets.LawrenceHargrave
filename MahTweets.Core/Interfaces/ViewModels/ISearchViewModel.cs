using System.Collections.ObjectModel;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.ViewModels
{
    public interface ISearchViewModel : IContainerViewModel
    {
        string SearchTerm { get; }
        ObservableCollection<ISearchProvider> SearchProviders { get; set; }
    }
}