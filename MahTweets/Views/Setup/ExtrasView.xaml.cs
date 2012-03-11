using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.ViewModels.Setup;

namespace MahTweets.Views.Setup
{
    public partial class ExtrasView
    {
        public ExtrasView()
        {
            InitializeComponent();
            Loaded += ExtrasViewLoaded;
        }

        private void ExtrasViewLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (ExtrasViewModel) DataContext;
            var cvsAvailable = Resources["cvsAvailableExtras"] as CollectionViewSource;
            var c = new CompositeCollection();
            c.Add(new CollectionContainer {Collection = vm.StatusHandlers});
            c.Add(new CollectionContainer {Collection = vm.UrlShorteners});
            cvsAvailable.Source = c;
        }

        private void UrlShortenerSettingsClick(object sender, RoutedEventArgs e)
        {
            //(IUrlShortener)sender
        }
    }

    public class ExtrasViewDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (item is IStatusHandler)
                return element.FindResource("IStatusHandler") as DataTemplate;

            if (item is IUrlShortener)
                return element.FindResource("IUrlShortener") as DataTemplate;

            return null;
        }
    }
}