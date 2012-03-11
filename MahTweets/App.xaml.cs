using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MahTweets.Configuration;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Global;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.ViewModels;
using Microsoft.VisualBasic.ApplicationServices;
using StartupEventArgs = Microsoft.VisualBasic.ApplicationServices.StartupEventArgs;

namespace MahTweets
{
    /// <summary>
    /// Wrapper WinForms Class to get access to WindowsFormsApplicationBase for single instance stuff.
    /// </summary>
    public class WindowsFormsApp : WindowsFormsApplicationBase
    {
        private static Thread _splashThread;
        private static ManualResetEvent _resetSplashCreated;
        private static Window _splashWindow;
        private App _wpfApp;

        public WindowsFormsApp()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            _resetSplashCreated = new ManualResetEvent(false);
            _splashThread = new Thread(ShowSplash);
            _splashThread.SetApartmentState(ApartmentState.STA);
            _splashThread.IsBackground = true;
            _splashThread.Name = "Splash Screen";
            _splashThread.Start();

            _resetSplashCreated.WaitOne();

            _wpfApp = new App((IgnoreStartupURI) _splashWindow);
            _wpfApp.InitializeComponent();
            _wpfApp.Run();

            return false;
        }

        private static void ShowSplash()
        {
            _splashWindow = new IgnoreStartupURI();
            _splashWindow.Show();
            _resetSplashCreated.Set();
            Dispatcher.Run();
        }

    }

    public partial class App
    {
        private MahTweetsBootstrapper _bootstrapper;
        private NotifyIcon _notifyIcon;

        private IShell _shell;

        public App(IgnoreStartupURI splashWindow)
        {
            //Proxy stuff.
            WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

            LoadCompleted += (s, e) => splashWindow.LoadComplete();
        }

        public AppViewModel ViewModel { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
            var wrapper = new WindowsFormsApp();
            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null
                &&
                AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null)
            {
                wrapper.Run(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData);
            }
            else
            {
                wrapper.Run(Environment.GetCommandLineArgs());
            }
        }

        private void ApplicationStartup(object sender, System.Windows.StartupEventArgs e)
        {
            BlackBoxRecorder.Init();
            GlobalDispatcher.Dispatcher = Dispatcher;

            try
            {
                LoadTrayIcon();
            }
            catch (Exception ex)
            {
                // oh dear, things are really really bad. Hosed. Gone Pear shaped.
                Console.WriteLine(ex.Message);
            }

            _bootstrapper = new MahTweetsBootstrapper();
            _bootstrapper.Bootstrap();

            StartApplication();
        }

        private void StartApplication()
        {
            _shell = CompositionManager.Get<IShell>();

            ViewModel = CompositionManager.Get<AppViewModel>();

            var accounts = CompositionManager.Get<IAccountSettingsProvider>();

            var columns = CompositionManager.Get<IColumnsSettingsProvider>();

            var applicationSettings = CompositionManager.Get<IApplicationSettingsProvider>();

            var scriptingManager = CompositionManager.Get<IScriptingManager>();

            scriptingManager.Start();

            try
            {
                if (applicationSettings.StyleFontSize > 0)
                    ChangeSize("DefaultFontSize", applicationSettings.StyleFontSize);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }

            if (accounts.MicroblogCredentials.Any())
            {
                ViewModel.LoadMicroblogs();
                ViewModel.LoadStreams();
                ViewModel.LoadSavedSearches();
                ViewModel.LoadUrlshorteners();
                ViewModel.LoadStatusHandlers();
            }
            else
            {
                ViewModel.ShowSetupWindow();
                ViewModel.LoadDefaultStream();
            }
            _shell.Start();
            OnLoadCompleted(null);
        }

        /// <summary>
        /// Save the settings on exit - should probably be done when the settings dialog closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _bootstrapper.Shutdown();

            if (ViewModel != null)
                ViewModel.Cleanup();

            if (_notifyIcon != null)
                _notifyIcon.Visible = false;
        }

        private void LoadTrayIcon()
        {
            //Setup the tray icon
            _notifyIcon = new NotifyIcon
                              {
                                  BalloonTipText = @"MahTweets has been minimised. Click the tray icon to show.",
                                  BalloonTipTitle = @"MahTweets",
                                  Text = @"MahTweets",
                                  Icon = new Icon("mahtweetsicon.ico"),
                                  Visible = true
                              };

            _notifyIcon.Click += NotifyIconClick;

            var cm = new ContextMenu();
            _notifyIcon.ContextMenu = cm;

            cm.MenuItems.Add("Reset Position", ResetClick);
            cm.MenuItems.Add("About", AboutClick);
            cm.MenuItems.Add("Exit", ExitClick);
        }

        private void ResetClick(object sender, EventArgs e)
        {
            _shell.SetWidth(400.0);
            _shell.SetHeight(600.0);

            _shell.SetTop(0.0);
            _shell.SetLeft(0.0);
        }

        private void AboutClick(object sender, EventArgs e)
        {
            _shell.DisplayWindow();
            ViewModel.Model.ShowAbout();
        }

        private static void ExitClick(object sender, EventArgs e)
        {
            Current.Shutdown();
        }

        private static void ChangeSize(String name, double size)
        {
            Current.Resources.Remove(name);
            Current.Resources.Add(name, size);
        }

        private void NotifyIconClick(object sender, EventArgs e)
        {
            var ev = e as MouseEventArgs;
            if (ev == null) return;

            if (_shell == null || ev.Button != MouseButtons.Left)
            {
                if (_shell != null && _shell.Visible)
                    _shell.HideWindow();
                else
                {
                    if (_shell != null) _shell.DisplayWindow();
                }
            }
        }
    }
}