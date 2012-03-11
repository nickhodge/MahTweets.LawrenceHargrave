using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.ViewModels;

namespace MahTweets.UI.Controls
{
    public partial class StreamsContainer
    {
        public StreamsContainer()
        {
            InitializeComponent();
            Loaded += StreamsContainer_Loaded;
        }

        private IMainViewModel Model
        {
            get { return DataContext as IMainViewModel; }
        }

        private void StreamsContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<MoveToEndStream>().Subscribe(MoveToEndStream);
        }

        private void MainWindowContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var _scriptingUIHelper = CompositionManager.Get<IScriptingUIHelper>();
            var _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (fe == null)
                return;

            var newcontextualmenu = new ContextMenu();
            if (_scriptingUIHelper != null)
            {
                List<object> menu = _scriptingUIHelper.CreateContextMenusFromScripts(false,
                                                                                     _scriptingConfiguration.
                                                                                         MainWindowContextMenuEntryPoint,
                                                                                     MenuItemClick);

                if (menu != null)
                {
                    foreach (object i in menu)
                        newcontextualmenu.Items.Add(i);
                }
            }

            fe.ContextMenu = newcontextualmenu;
        }

        private void MenuItemClick(object sender, RoutedEventArgs e)
        {
            var mif = e.Source as MenuItem;
            var _scriptingUIHelper = CompositionManager.Get<IScriptingUIHelper>();
            var _scriptingEngine = CompositionManager.Get<IScriptingEngine>();
            var _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (mif == null || _scriptingUIHelper == null || _scriptingEngine == null)
                return;

            string guidintoscripts = mif.Name; // get the name of the chosen contextual menu, used as key into scripts

            if (guidintoscripts != null)
            {
                if (!_scriptingUIHelper.IsEditScriptEvent(null, guidintoscripts))
                {
                    // TODO: test this works
                    _scriptingEngine.ExecuteScriptGUID(guidintoscripts,
                                                       _scriptingConfiguration.MainWindowContextMenuEntryPoint, Model);
                }
            }
        }

        private void MoveToEndStream(NullEvent obj)
        {
            //Fx bug, this is the workaround. See: http://connect.microsoft.com/VisualStudio/feedback/details/324064/wpf-listbox-scrollintoview-last-item-after-itemsource-change
            ScrollViewer.UpdateLayout();
            if (ScrollViewer.ExtentWidth > ScrollViewer.ViewportWidth)
                ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.ExtentWidth - 1);
        }
    }
}