using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;

namespace MahTweets.Views.Controls
{
    public partial class ModalPanel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Stack<UIElement> _stack;

        public ModalPanel()
        {
            InitializeComponent();

            _stack = new Stack<UIElement>();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _eventAggregator = CompositionManager.Get<IEventAggregator>();
            _eventAggregator.GetEvent<ShowDialog>().Subscribe(HandleShowDialog);
            _eventAggregator.GetEvent<CloseDialog>().Subscribe(HandleCloseDialog);
        }

        private void HandleShowDialog(UIElement obj)
        {
            _stack.Push(obj);

            if (ModalContent.Content == null)
            {
                Panel.SetZIndex(this, 4);
                ModalContent.Content = obj;
            }
            else
                ModalContent.Content = obj;

            btnCloseDialog.Visibility = Visibility.Visible;
        }

        private void HandleCloseDialog(UIElement obj)
        {
            if (_stack.Peek() != obj)
                return;

            _stack.Pop();
            ModalContent.Content = null;

            if (_stack.Count > 0)
                ModalContent.Content = _stack.Peek();
            else
            {
                Panel.SetZIndex(this, 0);
                btnCloseDialog.Visibility = Visibility.Collapsed;
            }
        }

        private void btnCloseDialog_Click(object sender, RoutedEventArgs e)
        {
            var uiElement = ModalContent.Content as UIElement;
            if (uiElement == null)
            {
                btnCloseDialog.Visibility = Visibility.Collapsed;
                return;
            }

            HandleCloseDialog(uiElement);
        }

        private void rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            btnCloseDialog_Click(null, null);
        }
    }
}