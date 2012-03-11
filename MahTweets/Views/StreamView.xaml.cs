using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.Scripting;
using MahTweets.ViewModels;

namespace MahTweets.Views
{
    public partial class StreamView
    {
        private CollectionViewSource _CVS;

        public StreamView()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
            }
            else
            {
                Loaded += StreamViewLoaded;
            }
        }

        public StreamView(StreamViewModel model)
            : this()
        {
            DataContext = model;
            model.View = this;
        }

        public new StreamViewModel ViewModel
        {
            get { return DataContext as StreamViewModel; }
        }

        public CollectionViewSource CVS
        {
            get { return _CVS ?? (_CVS = (CollectionViewSource) (FindResource("UpdatesCollectionView"))); }
        }

        private static void StreamViewLoaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<MoveToEndStream>().Publish(null);
        }

        private void ChkStreamUnchecked(object sender, RoutedEventArgs e)
        {
            var chkBox = ((CheckBox) sender);
            if (chkBox.IsChecked == false)
            {
                var updateType = (UpdateType) chkBox.DataContext;
                var tag = chkBox.Tag as IMicroblog;
                ViewModel.StreamConfiguration.RemoveStream(tag, updateType);
            }
        }

        private void ChkStreamChecked(object sender, RoutedEventArgs e)
        {
            var chkBox = ((CheckBox) sender);
            if (chkBox.IsChecked == true)
            {
                var updateType = (UpdateType) chkBox.DataContext;
                var tag = chkBox.Tag as IMicroblog;
                ViewModel.StreamConfiguration.AddStream(tag, updateType);
            }
        }

        private void CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var chkBox = ((CheckBox) sender);
            var btnColour = chkBox.FindName("btnColour") as Button;
            if (chkBox.IsChecked != true) return;
            Color defaultColor = Colors.White;
            var contact = chkBox.DataContext as Contact;

            if (btnColour != null && btnColour.Tag != null)
                defaultColor = (Color) btnColour.Tag;

            ViewModel.StreamConfiguration.AddContactFilter(contact, defaultColor);
        }

        private void CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            var chkBox = sender as CheckBox;
            if (chkBox == null) return;

            if (chkBox.IsChecked == false)
            {
                var contact = chkBox.Tag as Contact;
                ViewModel.StreamConfiguration.RemoveContactFilter(contact);
            }
        }

        private void CheckBoxIndeterminate(object sender, RoutedEventArgs e)
        {
            var chkBox = sender as CheckBox;
            if (chkBox == null) return;

            if (chkBox.IsChecked == null)
            {
                var contact = chkBox.DataContext as Contact;
                ViewModel.StreamConfiguration.AddContactExcludeFilter(contact);
            }
        }

        private void ScriptFilterUnchecked(object sender, RoutedEventArgs e)
        {
            var chkBox = sender as CheckBox;
            if (chkBox == null) return;

            if (chkBox.IsChecked == false)
            {
                var sf = chkBox.Tag as ScriptFilter;
                ViewModel.RemoveScriptFilter(sf);
            }
        }

        private void ScriptFilterChecked(object sender, RoutedEventArgs e)
        {
            var chkBox = sender as CheckBox;
            if (chkBox == null) return;

            if (chkBox.IsChecked == true)
            {
                var sf = chkBox.DataContext as ScriptFilter;
                ViewModel.AddScriptFilter(sf);
            }
        }

        private void ScriptFilterMouseUp(object sender, MouseButtonEventArgs e)
        {
            var txtBox = sender as TextBlock;
            if (txtBox == null) return;
            var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();

            var sf = txtBox.Tag as ScriptFilter;
            if (sf != null && scriptingUiHelper != null)
            {
                scriptingUiHelper.IsEditScriptEvent(sf.ScriptKey, null);
            }
        }

        private void BtnFilterColourClick(object sender, RoutedEventArgs e)
        {
            //var cd = new WPFColorPickerLib.ColorDialog();
            //var btnColour = sender as Button;
            //if (btnColour == null) return;
            //if (btnColour.Tag == null) return;

            //if (GlobalWindows.MainWindow != null)
            //    cd.Owner = GlobalWindows.MainWindow;

            //if (cd.ShowDialog() == true)
            //{
            //    var color = cd.SelectedColor;
            //    btnColour.Background = new SolidColorBrush(color);

            //    var chk = btnColour.FindName("Data") as CheckBox;
            //    if (chk == null) return;

            //    if (chk.IsChecked == true)
            //    {
            //        var sf = btnColour.Tag as ScriptFilter;
            //        ViewModel.UpdateScriptFilterColor(sf, color);
            //    }

            //    //Set the background to the colour so people can see what they've selected.
            //    var wp = chk.FindName("wp") as WrapPanel;
            //    var gb = new LinearGradientBrush();
            //    gb.GradientStops.Add(new GradientStop(Colors.White, 0));
            //    gb.GradientStops.Add(new GradientStop(color, 1));

            //    if (wp != null) wp.Background = gb;
            //}
        }

        private void ChkStreamIndeterminate(object sender, RoutedEventArgs e)
        {
            var chkbox = sender as CheckBox;
            if (chkbox == null) return;

            if (chkbox.IsChecked == null)
            {
                var updatetype = chkbox.DataContext as UpdateType;
                var tag = chkbox.Tag as IMicroblog;
                chkbox.ToolTip = "Completely ignore";
                if (updatetype == null || tag == null)
                {
                    return;
                }
                if (ViewModel != null)
                    ViewModel.StreamConfiguration.IgnoreStream(tag, updatetype);
            }
        }

        private void BtnColourClick(object sender, RoutedEventArgs e)
        {
            var btnColour = (Button) sender;
            var _eventAggregator = CompositionManager.Get<IEventAggregator>();
            //var filterColourPicker = new FilterColourPicker((c) =>
            //{
            //    btnColour.Tag = c;
            //    btnColour.Background = new SolidColorBrush(c);

            //    var chk = btnColour.FindName("Data") as CheckBox;
            //    if (chk == null) return;

            //    if (chk.IsChecked == true)
            //    {
            //        //var contactName = ((Contact)chk.DataContext).Name.ToUpper();
            //        var contact = chk.DataContext as Contact;
            //        if (contact != null)
            //            ViewModel.UpdateContactColor(contact, c);
            //    }

            //    //Set the background to the colour so people can see what they've selected.
            //    var wp = chk.FindName("wp") as WrapPanel;
            //    var gb = new LinearGradientBrush();
            //    gb.GradientStops.Add(new GradientStop(Colors.White, 0));
            //    gb.GradientStops.Add(new GradientStop(c, 1));

            //    if (wp != null) wp.Background = gb;
            //});
            //_eventAggregator.Show(filterColourPicker);

            //var cd = new WPFColorPickerLib.ColorDialog();
            //var btnColour = sender as Button;
            //if (btnColour == null) return;

            //if (GlobalWindows.MainWindow != null)
            //    cd.Owner = GlobalWindows.MainWindow;

            //if (cd.ShowDialog() == true)
            //{
            //    var color = cd.SelectedColor;
            //    btnColour.Tag = color;
            //    btnColour.Background = new SolidColorBrush(color);

            //    var chk = btnColour.FindName("Data") as CheckBox;
            //    if (chk == null) return;

            //    if (chk.IsChecked == true)
            //    {
            //        //var contactName = ((Contact)chk.DataContext).Name.ToUpper();
            //        var contact = chk.DataContext as Contact;
            //        if (contact != null)
            //            ViewModel.UpdateContactColor(contact, color);
            //    }

            //    //Set the background to the colour so people can see what they've selected.
            //    var wp = chk.FindName("wp") as WrapPanel;
            //    var gb = new LinearGradientBrush();
            //    gb.GradientStops.Add(new GradientStop(Colors.White, 0));
            //    gb.GradientStops.Add(new GradientStop(color, 1));

            //    if (wp != null) wp.Background = gb;
            //}
        }

        private void BtnColourClick1(object sender, RoutedEventArgs e)
        {
            var btnColour = (Button) sender;
            var _eventAggregator = CompositionManager.Get<IEventAggregator>();
            //var filterColourPicker = new FilterColourPicker((c) =>
            //                                                    {
            //                                                        btnColour.Tag = c;
            //                                                        btnColour.Background = new SolidColorBrush(c);
            //                                                        var chk = btnColour.FindName("Data") as CheckBox;
            //                                                        if (chk == null)
            //                                                            return;

            //                                                        if (chk.IsChecked == true)
            //                                                        {
            //                                                            var updatetype = ((UpdateType)chk.DataContext);
            //                                                            var imicroblog = ((IMicroblog)chk.Tag);
            //                                                            ViewModel.UpdateMicroblogColor(imicroblog, updatetype, c);
            //                                                        }

            //                                                        var wp = chk.FindName("wp") as WrapPanel;

            //                                                        if (wp == null)
            //                                                            return;

            //                                                        var gb = new LinearGradientBrush();
            //                                                        gb.GradientStops.Add(new GradientStop(Colors.White, 0));
            //                                                        gb.GradientStops.Add(new GradientStop(c, 1));
            //                                                        wp.Background = gb;
            //                                                    });
            //_eventAggregator.Show(filterColourPicker);

            //var cd = new WPFColorPickerLib.ColorDialog();
            //var btnColour = (Button)sender;
            //if (GlobalWindows.MainWindow != null)
            //    cd.Owner = GlobalWindows.MainWindow;
            //if (cd.ShowDialog() == true)
            //{
            //    Color color = cd.SelectedColor;
            //    btnColour.Tag = color;
            //    btnColour.Background = new SolidColorBrush(color);

            //    var chk = btnColour.FindName("Data") as CheckBox;
            //    if (chk == null)
            //        return;

            //    if (chk.IsChecked == true)
            //    {
            //        var updatetype = ((UpdateType)chk.DataContext);
            //        var imicroblog = ((IMicroblog)chk.Tag);
            //        ViewModel.UpdateMicroblogColor(imicroblog, updatetype, color);
            //    }

            //    //Set the background to the colour so people can see what they've selected.
            //    var wp = chk.FindName("wp") as WrapPanel;

            //    if (wp == null)
            //        return;

            //    var gb = new LinearGradientBrush();
            //    gb.GradientStops.Add(new GradientStop(Colors.White, 0));
            //    gb.GradientStops.Add(new GradientStop(color, 1));
            //    wp.Background = gb;
            //}
        }

        private void UpdatesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (svUpdates.SelectedItem == null) return;

            var selected = ((IStatusUpdate) svUpdates.SelectedItem);
            selected.IsRead = true;
        }

        private void CollectionViewSourceFilter(object sender, FilterEventArgs e)
        {
            if (ViewModel != null)
            {
                bool accept = ViewModel.Filter(e.Item as IStatusUpdate);
                e.Accepted = accept;
            }
        }

        private void ClearUnreadItems(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.ClearBeforeDate(DateTime.Now);
        }

        private void UpdatesKeyDown(object sender, KeyEventArgs e)
        {
            var application = Application.Current as App;
            if (application == null)
                return;

            if (e.Key == Key.Right)
            {
                IEnumerable<IContainerViewModel> x =
                    application.ViewModel.Model.StreamContainers.Where(i => i.Position == (ViewModel.Position + 1));
                if (x.Any())
                {
                    var streamView = x.First().View as StreamView;
                    if (streamView == null) return;
                    ListBox sv = (streamView).svUpdates;
                    sv.Focus();
                }
            }
            else if (e.Key == Key.Left)
            {
                IEnumerable<IContainerViewModel> x =
                    application.ViewModel.Model.StreamContainers.Where(i => i.Position == (ViewModel.Position - 1));
                if (x.Any())
                {
                    var streamView = x.First().View as StreamView;
                    if (streamView == null) return;
                    ListBox sv = (streamView).svUpdates;
                    sv.Focus();
                }
            }
        }

        private void svUpdates_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.Pause();
        }

        private void svUpdates_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.Resume();
        }

        private void btnChangeOrder_Click(object sender, RoutedEventArgs e)
        {
            SortItems(ViewModel.StreamConfiguration.Filters.Direction == ListSortDirection.Ascending
                          ? ListSortDirection.Descending
                          : ListSortDirection.Ascending);
        }

        private void SortItems(ListSortDirection newDirection)
        {
            CVS.SortDescriptions.Clear();
            CVS.SortDescriptions.Add(new SortDescription("Time", newDirection));
            CVS.SortDescriptions.Add(new SortDescription("ID", newDirection));
            ViewModel.StreamConfiguration.Filters.Direction = newDirection;
        }

        #region MahTweets Scripting UI Addition

        private void ColumnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();
            var scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();
            if (fe == null || scriptingUiHelper == null)
                return;

            var newcontextualmenu = new ContextMenu();

            var nmi = new MenuItem {Header = "Clear"};
            nmi.Click += ClearUnreadItems;
            newcontextualmenu.Items.Add(nmi);

            List<object> menu = scriptingUiHelper.CreateContextMenusFromScripts(newcontextualmenu.HasItems,
                                                                                scriptingConfiguration.
                                                                                    ColumnContextMenuEntryPoint,
                                                                                MenuItemClick);
            if (menu != null)
            {
                foreach (object i in menu)
                    newcontextualmenu.Items.Add(i);
            }

            fe.ContextMenu = newcontextualmenu;
        }

        private void MenuItemClick(object sender, RoutedEventArgs e)
        {
            var mif = e.Source as MenuItem;
            var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();
            var scriptingEngine = CompositionManager.Get<IScriptingEngine>();
            var scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (mif == null || scriptingUiHelper == null || scriptingEngine == null)
                return;

            string guidintoscripts = mif.Name; // get the name of the chosen contextual menu, used as key into scripts

            if (guidintoscripts != null)
            {
                if (!scriptingUiHelper.IsEditScriptEvent(null, guidintoscripts))
                {
                    scriptingEngine.ExecuteScriptGUID(guidintoscripts,
                                                      scriptingConfiguration.ColumnContextMenuEntryPoint, ViewModel);
                }
            }
        }

        #endregion
    }
}