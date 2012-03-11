using System.ComponentModel;
using System.Windows;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.ViewModels;

namespace MahTweets.Views
{
    public partial class ConversationView
    {
        public ConversationView()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
            }
            else
            {
                Loaded += ConversationViewLoaded;
            }
        }

        public new ConversationViewModel ViewModel
        {
            get { return DataContext as ConversationViewModel; }
        }

        private static void ConversationViewLoaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<MoveToEndStream>().Publish(null);
        }
    }
}