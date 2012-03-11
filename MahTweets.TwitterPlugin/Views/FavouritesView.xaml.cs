using MahTweets.Core.Views;
using MahTweets.TwitterPlugin.Logic;

namespace MahTweets.TwitterPlugin.Views
{
    public partial class FavouritesView : IView<FavouritesViewModel>
    {
        public FavouritesView()
        {
            InitializeComponent();
        }

        #region IView<FavouritesViewModel> Members

        public FavouritesViewModel ViewModel
        {
            get { return DataContext as FavouritesViewModel; }
        }

        #endregion

        public void Start()
        {
        }
    }
}