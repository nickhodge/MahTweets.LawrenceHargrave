using System;
using System.Windows.Media.Imaging;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;

namespace MahTweets.UI.Controls.Mapping
{
    public partial class CloseableMap
    {
        private readonly IContact _c;

        public CloseableMap(double lat, double lon, IContact c = null)
        {
            _c = c;
            InitializeComponent();
            DataContext = this;
            var applicationSettingsProvider = CompositionManager.Get<IApplicationSettingsProvider>();

            map.Source = applicationSettingsProvider.MapEngine == 0
                             ? new BitmapImage(
                                   new Uri(
                                       string.Format(
                                           "http://dev.virtualearth.net/REST/V1/Imagery/Map/Road/{0},{1}/16?mapSize=245,200&pushpin={0},{1};1&key=Aml5DDclMcqafTmR6BjVt9utuMtLAgoTzIsAfFbBjTOQ2CoJfDo5rd1BFdmluW3d",
                                           lat, lon), UriKind.RelativeOrAbsolute))
                             : new BitmapImage(
                                   new Uri(
                                       string.Format(
                                           "http://maps.google.com/maps/api/staticmap?center={0},{1}&markers=color:red|color:red|{0},{1}&zoom=12&size=245x200&sensor=false",
                                           lat, lon), UriKind.RelativeOrAbsolute));
        }

        public IContact C
        {
            get { return _c; }
        }

        public DelegateCommand CloseCommand { get; set; }
    }
}