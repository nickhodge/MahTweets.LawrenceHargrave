using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.ViewModels;
using MahTweets.ViewModels.Setup;

namespace MahTweets.ViewModels
{
    public class SetupViewModel : BaseViewModel
    {
        public SetupViewModel(
            AccountsViewModel accountsViewModel,
            GeneralSettingsViewModel generalSettingsViewModel,
            GlobalIgnoresViewModel globalIgnoresViewModel,
            ExtrasViewModel extrasViewModel,
            IPluginRepository pluginRepository,
            IEnumerable<IUrlShortener> urlshorteners)
        {
            PluginRepository = pluginRepository;

            UrlShortenersCollection = new ObservableCollection<IUrlShortener>(urlshorteners);

            AccountsViewModel = accountsViewModel;
            GeneralSettingsViewModel = generalSettingsViewModel;
            GlobalIgnoresViewModel = globalIgnoresViewModel;
            ExtrasViewModel = extrasViewModel;
        }

        public ObservableCollection<IUrlShortener> UrlShortenersCollection { get; set; }

        public IPluginRepository PluginRepository { get; set; }

        public Grid OverlayContent { get; set; }

        public virtual ExtrasViewModel ExtrasViewModel { get; set; }

        public virtual AccountsViewModel AccountsViewModel { get; set; }

        public virtual GeneralSettingsViewModel GeneralSettingsViewModel { get; set; }

        public virtual GlobalIgnoresViewModel GlobalIgnoresViewModel { get; set; }
    }
}