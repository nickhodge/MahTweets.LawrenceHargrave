using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.UI.Controls
{
    public partial class MultiParentChoiceDialog
    {
        private readonly Action<IMicroblog> _onSelected;

        public MultiParentChoiceDialog(IEnumerable<IMicroblog> accounts, Action<IMicroblog> onSelected, String verb,
                                       String target)
        {
            InitializeComponent();
            Verb = verb;
            Target = target;
            if (accounts != null) lstParents.ItemsSource = accounts;
            _onSelected = onSelected;
            if (accounts != null) Type = accounts.First().Name;
            DataContext = this;
        }

        public String Verb { get; set; }
        public String Target { get; set; }
        public String Type { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selected = ((Button) sender).DataContext as IMicroblog;
            if (selected == null) return;
            _onSelected(selected);
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<CloseDialog>().Publish(this);
        }
    }
}