using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Filters;
using MahTweets.Core.Global;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.Scripting;
using MahTweets.Core.VersionCheck;
using MahTweets.Helpers;
using MahTweets.UI.Controls;
using MahTweets.ViewModels;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace MahTweets
{
    public partial class MainWindow : MetroWindow, IShell
    {
        public static readonly RoutedUICommand ShowSetup = new RoutedUICommand("Initiated show setup", "ShowSetup",
                                                                               typeof (MainWindow));

        public static readonly RoutedUICommand ShowScriptConsoleCommand = new RoutedUICommand("Show Scripting Console",
                                                                                              "ShowScriptConsoleCommand",
                                                                                              typeof (MainWindow));

        public readonly TaskbarManager TaskbarManager;
        private readonly IAccountSettingsProvider _accountSettings;
        private readonly IApplicationSettingsProvider _applicationSettingsProvider;
        private readonly IColumnsSettingsProvider _columnsSettings;
        private readonly IPluginRepository _pluginRepository;
        private readonly IEventAggregator _eventAggregator;

        public MainWindow(AppViewModel viewModel,
                          IPluginRepository pluginRepository,
                          IAccountSettingsProvider accountSettingsProvider,
                          IColumnsSettingsProvider columnsSettingsProvider,
                          IApplicationSettingsProvider applicationSettingsProvider,
                          IEventAggregator eventAggregator)
        {
            InitializeComponent();

            _pluginRepository = pluginRepository;
            _accountSettings = accountSettingsProvider;
            _columnsSettings = columnsSettingsProvider;
            _applicationSettingsProvider = applicationSettingsProvider;
            _eventAggregator = eventAggregator;

            var mainViewModel = viewModel.Model;
            var resourceViewModel = viewModel.Resources;

            txtUpdate.SetHeight((Double) FindResource("DefaultCompositionBoxHeight"));
            var c = (CompositionTextBox) txtUpdate.FindName("txtUpdateBox");
            if (c != null)
                c.IsInlineReply = false;

            Model = mainViewModel;
            ResourcesViewModel = resourceViewModel;
            mainViewModel.View = this;

            DataContext = mainViewModel;

            GlobalWindows.MainWindow = this;

            try
            {
                if (TaskbarManager.IsPlatformSupported)
                    TaskbarManager = TaskbarManager.Instance;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }


            if (resourceViewModel.Views == null) return;

            //Import the XAML
            foreach (var r in resourceViewModel.Views)
            {
                Resources.MergedDictionaries.Add(r);
            }

            var ibShowSetup = new InputBinding(ShowSetup, new KeyGesture(Key.S, ModifierKeys.Control));
            var ibShowScriptingConsole = new InputBinding(ShowScriptConsoleCommand,
                                                          new KeyGesture(Key.S, ModifierKeys.Alt));
            InputBindings.Add(ibShowSetup);
            InputBindings.Add(ibShowScriptingConsole);

            var cbShowSetup = new CommandBinding(ShowSetup);
            cbShowSetup.Executed += CbShowSetupExecuted;
            CommandBindings.Add(cbShowSetup);

            var scriptConsole = new CommandBinding(ShowScriptConsoleCommand);
            scriptConsole.Executed += ScriptConsoleVisible;
            CommandBindings.Add(scriptConsole);

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                MahTweets.Configuration.WindowSettings.SetSave(this, true);
            }
        }

        public ResourcesViewModel ResourcesViewModel { get; set; }
        public IMainViewModel Model { get; set; }

        #region IShell Members

        public void Start()
        {
            #region MahTweets scripting for app startup

            var scriptingmanager = CompositionManager.Get<IScriptingManager>();
            var scriptinglibrary = CompositionManager.Get<IScriptingLibrarian>();
            var scriptingconfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (scriptingmanager.IsScriptEngineActive)
            {
                if (scriptinglibrary.CountScriptEntryPoints(scriptingconfiguration.MahTweetsStartupEntryPoint) > 0)
                {
                    foreach (
                        var sf in
                            scriptinglibrary.ScriptEntryPoints(scriptingconfiguration.MahTweetsStartupEntryPoint))
                    {
                        try
                        {
                            scriptingmanager.ExecuteScriptNoReturn(sf.Key,
                                                                    scriptingconfiguration.MahTweetsStartupEntryPoint,
                                                                    Model, null);
                        }
                        catch (Exception ex)
                        {
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                        }
                    }
                }
            }

            #endregion

            if (_pluginRepository.Microblogs.Count == 1)
            {
                lstUpdateTargets.SelectedIndex = 0;
            }
            else
                // erg, we have multiple accounts, now at startup we should find & select the default account for this user in this checkboxy thing
            {
                if (_applicationSettingsProvider.SelectedAccounts.Count > 0)
                {
                    foreach (var selected in _applicationSettingsProvider.SelectedAccounts)
                    {
                        var selected1 = selected;
                        var selectedMicroblog = _pluginRepository.Microblogs.FirstOrDefault(m => m.Id == selected1);
                        if (selectedMicroblog == null) continue;
                        lstUpdateTargets.SelectedItems.Add(selectedMicroblog);
                    }
                }
            }

            if (Model.FilterGroups.Count == 0)
            {
                var f = StreamModel.CreateDefault();
                Model.CreateStream(f, null);
                Model.FilterGroups.Add(f);
            }

            // init the Version check thingy
            var vc = CompositionManager.Get<ICurrentVersionCheck>();
            var assembly = Assembly.GetExecutingAssembly().GetName();
            _eventAggregator.GetEvent<CheckMahTweetsVersion>().Publish(new CheckMahTweetsVersionMessage(assembly.Version.Major, assembly.Version.Minor, assembly.Version.Build, assembly.Version.Revision.ToString()));


            AddToTaskbarThumbnail(GlobalWindows.MainWindow, GlobalWindows.MainWindow, "MahTweets Main Window", new Vector(0, 0));
            Show();
        }

        public void SetWidth(double width)
        {
            Width = width;
        }

        public void SetHeight(double height)
        {
            Height = height;
        }

        public void SetTop(double top)
        {
            Top = top;
        }

        public void SetLeft(double left)
        {
            Left = left;
        }

        public void DisplayWindow()
        {
            Activate();
            Show();
            WindowState = WindowState.Normal;
        }

        public bool Visible
        {
            get { return Visibility == Visibility.Visible; }
        }

        public void HideWindow()
        {
            Hide();
            WindowState = WindowState.Minimized;
        }

        #endregion

        #region windows 7 thumbnail magicals

        public Timer TabbedThumbnailTimer;

        public void AddToTaskbarThumbnail(Window winder, UIElement fifthelement, string timthetoolmantip,
                                          Vector? kimpeekoffset)
        {
            if (TaskbarManager.IsPlatformSupported)
            {
                var tt = new TabbedThumbnail(winder, fifthelement, new Vector(0, 0))
                             {
                                 Title = "MahTweets",
                                 Tooltip = timthetoolmantip,
                                 DisplayFrameAroundBitmap = false,
                                 PeekOffset = kimpeekoffset
                             };


                tt.TabbedThumbnailActivated += TabbedThumbnailActivated;

                TaskbarManager.TabbedThumbnail.AddThumbnailPreview(tt);

                if (TabbedThumbnailTimer == null)
                {
                    TabbedThumbnailTimer = new Timer();
                    TabbedThumbnailTimer.Elapsed += ThumbnailTimerElapsed;
                    TabbedThumbnailTimer.Interval = 1000;
                    TabbedThumbnailTimer.Start();
                }
            }
        }


        private void TabbedThumbnailActivated(object sender, TabbedThumbnailEventArgs e)
        {
            Activate();
        }


        public void ThumbnailTimerElapsed(object sender, ElapsedEventArgs e)
        {
            //TODO async this bit
            // TaskbarManager.TabbedThumbnail.InvalidateThumbnails();
        }

        #endregion

        private void CbShowSetupExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Model.ShowSetup();
        }

        private void CvsTargetsFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = !((IMicroblog) e.Item).ReadOnly;
        }

        public void CreateNewStream(StreamModel f)
        {
            Model.CreateStream(f, null);
        }

        private void WindowDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (droppedFilePaths == null)
                    return;

                string file = droppedFilePaths[0];

                if (file.ToLower().EndsWith(".jpg") ||
                    file.ToLower().EndsWith(".jpeg") ||
                    file.ToLower().EndsWith(".gif") ||
                    file.ToLower().EndsWith(".png"))
                {
                    //Upload(file);
                }
            }
        }

        private void CheckedChecked(object sender, RoutedEventArgs e)
        {
            var chkbox = sender as CheckBox;
            if (chkbox == null) return;

            var blog = chkbox.Tag as IMicroblog;
            if (blog == null) return;

            Model.SelectMicroblog(blog);
        }

        private void CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            var chkbox = sender as CheckBox;
            if (chkbox == null) return;

            var blog = chkbox.Tag as IMicroblog;
            if (blog == null) return;

            Model.UnselectMicroblog(blog);
        }


        //private void btnCloseDialog_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    var sb = FindResource("hideDialog") as Storyboard;
        //    sb.Begin();
        //}

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            //TODO: Move this onto the model. perhaps a command on closing?

            #region MahTweets scripting for app shutdown

            var scriptingmanager = CompositionManager.Get<IScriptingManager>();
            var scriptinglibrary = CompositionManager.Get<IScriptingLibrarian>();
            var scriptingconfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (scriptingmanager.IsScriptEngineActive)
            {
                if (scriptinglibrary.CountScriptEntryPoints(scriptingconfiguration.MahTweetsShutdownEntryPoint) > 0)
                {
                    foreach (
                        ScriptFile sf in
                            scriptinglibrary.ScriptEntryPoints(scriptingconfiguration.MahTweetsShutdownEntryPoint))
                    {
                        try
                        {
                            scriptingmanager.ExecuteScriptNoReturn(sf.Key,
                                                                    scriptingconfiguration.MahTweetsShutdownEntryPoint,
                                                                    Model, null);
                        }
                        catch (Exception ex)
                        {
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                        }
                    }
                }
            }

            #endregion

            SettingsHelper.SaveSettings(_accountSettings, _pluginRepository);
            SettingsHelper.SaveSettings(Model, _accountSettings, _columnsSettings, _applicationSettingsProvider);

            Application.Current.Shutdown();
        }

        private void ListBoxMouseUp(object sender, MouseButtonEventArgs e)
        {
            lstUpdateTargets.Visibility = lstUpdateTargets.Visibility == Visibility.Hidden
                                              ? Visibility.Visible
                                              : Visibility.Hidden;
        }

        private void AvatarChangeDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                    if (droppedFilePaths == null)
                        return;

                    var fileName = droppedFilePaths[0];

                    if (fileName.ToLower().EndsWith(".jpg") ||
                        fileName.ToLower().EndsWith(".jpeg") ||
                        fileName.ToLower().EndsWith(".gif") ||
                        fileName.ToLower().EndsWith(".png"))
                    {
                        var mb = (IMicroblog) ((FrameworkElement) sender).Tag;
                        mb.UpdateAvatar(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void AvatarChangeDragOver(object sender, DragEventArgs e)
        {
            var mb = ((FrameworkElement) sender).Tag;
            if (mb is IMicroblog)
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ScriptConsoleVisible(object sender, ExecutedRoutedEventArgs e)
        {
            var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();
            scriptingUiHelper.ScriptConsoleView = new ScriptConsole(this);
            scriptingUiHelper.ScriptConsoleView.Show();
        }

        #region Nested type: WindowMode

        private enum WindowMode
        {
            Maximise,
            Normal
        }

        #endregion
    }
}