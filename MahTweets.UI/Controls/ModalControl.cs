using System;
using System.ComponentModel;
using System.Windows.Controls;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;

namespace MahTweets.UI.Controls
{
    public class ModalControl : UserControl
    {
        private bool? _modalResult;

        public bool? ModalResult
        {
            get { return _modalResult; }
            set
            {
                _modalResult = value;

                if (Closing != null)
                {
                    var args = new CancelEventArgs();
                    Closing(this, args);

                    if (args.Cancel) return;
                }

                if (Closed != null)
                    Closed(this, EventArgs.Empty);

                var eventAggregator = CompositionManager.Get<IEventAggregator>();
                eventAggregator.GetEvent<CloseDialog>().Publish(this);
            }
        }

        public event EventHandler Closed;
        public event CancelEventHandler Closing;
    }
}