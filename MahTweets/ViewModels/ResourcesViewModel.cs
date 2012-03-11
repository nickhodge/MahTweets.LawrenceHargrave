using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.ViewModels;

namespace MahTweets.ViewModels
{
    public class ResourcesViewModel : IResourcesViewModel
    {
        public ResourcesViewModel(IEnumerable<ResourceDictionary> views,
                                  IEnumerable<IStyle> themes)
        {
            Views = views;
            Themes = new ObservableCollection<IStyle>(themes);
        }


        public ObservableCollection<IStyle> Themes { get; private set; }

        #region IResourcesViewModel Members

        public IEnumerable<ResourceDictionary> Views { get; private set; }

        #endregion
    }
}