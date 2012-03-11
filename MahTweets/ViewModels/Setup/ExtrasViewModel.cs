using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MahTweets.Core.Commands;
using MahTweets.Core.Events;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.ViewModels;

namespace MahTweets.ViewModels.Setup
{
    public class ExtrasViewModel : BaseViewModel
    {
        public ExtrasViewModel(
            IPluginRepository pluginRepository,
            IEnumerable<IStatusHandler> statusHandlers,
            IEnumerable<IUrlShortener> urlShorteners,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            StatusHandlers = new ObservableCollection<IStatusHandler>(statusHandlers.OrderBy(m => m.Name));
            UrlShorteners = new ObservableCollection<IUrlShortener>(urlShorteners.Where(u => u.HasSettings));
            ShowSettingsCommand = new DelegateCommand<IPlugin>(ShowSettings);
        }

        public ObservableCollection<IStatusHandler> StatusHandlers { get; set; }
        public ObservableCollection<IUrlShortener> UrlShorteners { get; set; }

        public ICommand ShowSettingsCommand { get; set; }

        private void ShowSettings(IPlugin plugin)
        {
            if (plugin.HasSettings)
                plugin.ShowSettings();
        }
    }
}