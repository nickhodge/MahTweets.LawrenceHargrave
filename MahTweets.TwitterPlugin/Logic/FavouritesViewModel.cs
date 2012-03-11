using MahTweets.Core.Collections;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.ViewModels;

namespace MahTweets.TwitterPlugin.Logic
{
    public class FavouritesViewModel : ContainerViewModel
    {
        public FavouritesViewModel()
        {
            Updates = new ThreadSafeObservableCollection<IStatusUpdate>();
        }

        public ThreadSafeObservableCollection<IStatusUpdate> Updates { get; set; }
    }
}