using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.ViewModels;

namespace MahTweets.Views.Controls
{
    public class ContainerView : ItemsControl
    {
        private Grid _containerViewGrid;

        static ContainerView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ContainerView),
                                                     new FrameworkPropertyMetadata(typeof (ContainerView)));
        }

        public ContainerViewModel ViewModel
        {
            get { return DataContext as ContainerViewModel; }
        }

        public ConfirmClose ConfirmClose
        {
            get { return Template.FindName("confirmClose", this) as ConfirmClose; }
        }

        private Grid ContainerViewGrid
        {
            get { return _containerViewGrid ?? (_containerViewGrid = Template.FindName("ContainerViewGrid", this) as Grid); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ConfirmClose.Result += ConfirmCloseResult;
        }

        private void ConfirmCloseResult(object sender, ConfirmEventArgs e)
        {
            if (e.Result)
            {
                var sb = ContainerViewGrid.Resources["RemoveStream"] as Storyboard;
                if (sb != null)
                {
                    sb.Completed += (s, ie) =>
                                        {
                                            if (ViewModel != null)
                                            {
                                                ViewModel.Close();
                                            }
                                        };
                    sb.Begin();
                }
                else
                    ViewModel.Close();
            }
            else
            {
                var sb = ContainerViewGrid.Resources["HideConfirmClose"] as Storyboard;
                if (sb != null) sb.Begin();
            }
        }

        public void ConfirmCloseStream(object sender, RoutedEventArgs e)
        {
            var sb = ContainerViewGrid.Resources["ShowConfirmClose"] as Storyboard;
            if (sb != null) sb.Begin();
        }

        public void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var f = (Thumb) sender;
            f.Background = (Brush) FindResource("HeadingBackgroundColour");
                // TODO correct background colour choice on drag of thumb to be the appropriate colour for the type of element
            f.Foreground = (Brush) FindResource("HeadingBackgroundColour");
            var g = (FrameworkElement) f.Parent;
            var h = (FrameworkElement) g.Parent;
            ViewModel.Width = h.Width;
        }

        public void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var f = (Thumb) sender;
            f.Background = (Brush) FindResource("LightGreyColour");
            f.Foreground = (Brush) FindResource("LightGreyColour");
        }

        public void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var f = (FrameworkElement) sender;
            var g = (FrameworkElement) f.Parent;
            var h = (FrameworkElement) g.Parent;
            double xadjust = h.Width + e.HorizontalChange;
            if (xadjust < 0) return;
            h.Width = xadjust;
            Canvas.SetLeft(f, Canvas.GetLeft(f) + e.HorizontalChange);
        }

        public void ColumnDrag_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var dobj = new DataObject();
            var ansc1 = (FrameworkElement) FindAncestor<Grid>((FrameworkElement) sender);
                // put the parent's grid into the DataObject
            if (ansc1 == null) return;
            dobj.SetData(ansc1);
            DragDrop.DoDragDrop(this, dobj, DragDropEffects.Move);
        }

        public void ColumnDrag_Drop(object sender, DragEventArgs e)
        {
            var stpnlholder = FindAncestor<StackPanel>((FrameworkElement) sender);
            if (stpnlholder == null) return;
            var d1 = (Grid) e.Data.GetData(typeof (Grid));
            if (d1 == null) return;
            object draggedelement = d1.DataContext;
            if (draggedelement == null) return;
            var dt = e.Source as UIElement;
            if (dt == null) return;
            var d2 = FindGrandparent<Grid>(dt);
            if (d2 == null) return;

            object droppedontoelement = d2.DataContext;
            if (draggedelement == droppedontoelement)
                return;

            if (!(from UIElement element in stpnlholder.Children
                  select ((ContentPresenter) element).Content
                  into cc
                  where cc != null
                  select cc).Contains(droppedontoelement)) return;
            var hThumb = (Thumb) d2.FindName("columnHandle");
            if (hThumb != null)
            {
                hThumb.Foreground = (Brush) FindResource("BaseColour");
                hThumb.Opacity = (double) FindResource("BaseColourOpacityThinColumns");
            }
            var model = CompositionManager.Get<IMainViewModel>();
            int droptargetIndex = -1, i = 0;
            foreach (ContainerViewModel element in model.StreamContainers)
            {
                if (element.Equals(droppedontoelement))
                {
                    droptargetIndex = i;
                    break;
                }
                i++;
            }
            if (droptargetIndex == -1) return;
            model.StreamContainers.Remove((ContainerViewModel) draggedelement);
            model.StreamContainers.Insert(droptargetIndex, (ContainerViewModel) draggedelement);
        }

        public void ColumnDrag_DragEnter(object sender, DragEventArgs e)
        {
            var stpnlholder = FindAncestor<StackPanel>((FrameworkElement) sender);
            if (stpnlholder == null) return;
            var d1 = (Grid) e.Data.GetData(typeof (Grid));
            if (d1 == null) return;
            object draggedelement = d1.DataContext;
            if (draggedelement == null) return;
            var dt = e.Source as UIElement;
            if (dt == null) return;
            var d2 = FindGrandparent<Grid>(dt);
            if (d2 == null) return;
            object droppedontoelement = d2.DataContext;
            if (draggedelement == droppedontoelement)
            {
                Mouse.SetCursor(Cursors.Hand);
                return;
            }
            if (!(from UIElement element in stpnlholder.Children
                  select ((ContentPresenter) element).Content
                  into cc where cc != null select cc).Contains(droppedontoelement)) return;
            var hThumb = (Thumb) d2.FindName("columnHandle");
            if (hThumb == null) return;
            hThumb.Foreground = (Brush) FindResource("BaseColour");
            hThumb.Opacity = (double) FindResource("BaseColourOpacityThinColumnsHighlight");
        }

        public void ColumnDrag_DragLeave(object sender, DragEventArgs e)
        {
            var stpnlholder = FindAncestor<StackPanel>((FrameworkElement) sender);
            if (stpnlholder == null) return;
            var d1 = (Grid) e.Data.GetData(typeof (Grid));
            if (d1 == null) return;
            object draggedelement = d1.DataContext;
            if (draggedelement == null) return;
            var dt = e.Source as UIElement;
            if (dt == null) return;
            var d2 = FindGrandparent<Grid>(dt);
            if (d2 == null) return;

            object droppedontoelement = d2.DataContext;
            if (draggedelement == droppedontoelement)
            {
                Mouse.SetCursor(Cursors.Hand);
                return;
            }
            if (!(from UIElement element in stpnlholder.Children
                  select ((ContentPresenter) element).Content
                  into cc
                  where cc != null
                  select cc).Contains(droppedontoelement)) return;
            var hThumb = (Thumb) d2.FindName("columnHandle");
            if (hThumb == null) return;
            hThumb.Foreground = (Brush) FindResource("BaseColour");
            hThumb.Opacity = (double) FindResource("BaseColourOpacityThinColumns");
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T) current;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        private static T FindGrandparent<T>(DependencyObject current) where T : DependencyObject
        {
            current = VisualTreeHelper.GetParent(current); // go up one straight away
            current = VisualTreeHelper.GetParent(current); // go up one straight away
            do
            {
                if (current is T)
                {
                    return (T) current;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }
    }
}