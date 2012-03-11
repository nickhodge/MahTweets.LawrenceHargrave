using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Configuration
{
    /// <summary>
    /// Persists a Window's Size, Location and WindowState to UserScopeSettings 
    /// </summary>
    public class WindowSettings
    {
        // ReSharper disable InconsistentNaming
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        /// <summary>
        /// Register the "Save" attached property and the "OnSaveInvalidated" callback 
        /// </summary>
        public static readonly DependencyProperty SaveProperty
            = DependencyProperty.RegisterAttached("Save", typeof (bool), typeof (WindowSettings),
                                                  new FrameworkPropertyMetadata(OnSaveInvalidated));

        // ReSharper restore InconsistentNaming

        private Window _window;
        private WindowApplicationSettings _windowApplicationSettings;

        public WindowSettings(Window window)
        {
            _window = window;
        }

        [Browsable(false)]
        internal WindowApplicationSettings Settings
        {
            get
            {
                if (_windowApplicationSettings == null)
                {
                    _windowApplicationSettings = CreateWindowApplicationSettingsInstance();
                }
                return _windowApplicationSettings;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        public static void SetSave(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(SaveProperty, enabled);
        }

        /// <summary>
        /// Called when Save is changed on an object.
        /// </summary>
        private static void OnSaveInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = dependencyObject as Window;
            if (window == null || !((bool) e.NewValue)) return;
            var settings = new WindowSettings(window);
            settings.Attach();
        }

        /// <summary>
        /// Load the Window Size Location and State from the settings object
        /// </summary>
        protected virtual void LoadWindowState()
        {
            Settings.Reload();

            if (Settings.Placement == null) return;
            try
            {
                // Load window placement details for previous application session from application settings
                // if window was closed on a monitor that is now disconnected from the computer,
                // SetWindowPlacement will place the window onto a visible monitor.
                WINDOWPLACEMENT wp = Settings.Placement.Value;

                wp.length = Marshal.SizeOf(typeof (WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = (wp.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : wp.showCmd);
                IntPtr hwnd = new WindowInteropHelper(_window).Handle;
                SetWindowPlacement(hwnd, ref wp);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(string.Format("Failed to load window state:\r\n{0}", ex));
#endif
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        /// <summary>
        /// Save the Window Size, Location and State to the settings object
        /// </summary>
        protected virtual void SaveWindowState()
        {
            WINDOWPLACEMENT wp;
            IntPtr hwnd = new WindowInteropHelper(_window).Handle;
            GetWindowPlacement(hwnd, out wp);
            Settings.Placement = wp;
            Settings.Save();
        }

        private void Attach()
        {
            if (_window == null) return;
            _window.Closing += WindowClosing;
            _window.SourceInitialized += WindowSourceInitialized;
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            LoadWindowState();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            SaveWindowState();
            _window.Closing -= WindowClosing;
            _window.SourceInitialized -= WindowSourceInitialized;
            _window = null;
        }

        internal virtual WindowApplicationSettings CreateWindowApplicationSettingsInstance()
        {
            return new WindowApplicationSettings(this);
        }

        #region Nested type: WindowApplicationSettings

        internal class WindowApplicationSettings : ApplicationSettingsBase
        {
            public WindowApplicationSettings(WindowSettings windowSettings)
                : base(windowSettings._window.GetType().FullName)
            {
            }

            [UserScopedSetting]
            public WINDOWPLACEMENT? Placement
            {
                get
                {
                    if (this["Placement"] != null)
                    {
                        return ((WINDOWPLACEMENT) this["Placement"]);
                    }
                    return null;
                }
                set { this["Placement"] = value; }
            }
        }

        #endregion
    }
}