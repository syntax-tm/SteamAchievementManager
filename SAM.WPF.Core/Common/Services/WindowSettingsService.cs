using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;

namespace SAM.WPF.Core.Services
{
    /// <summary>
    /// Persists a Window's Size, Location and WindowState to UserScopeSettings
    /// </summary>
    public class WindowSettings
    {
        /// <summary>
        /// Register the "Save" attached property and the "OnSaveInvalidated" callback
        /// </summary>
        public static readonly DependencyProperty SaveProperty =
            DependencyProperty.RegisterAttached("Save", typeof(bool), typeof(WindowSettings), new FrameworkPropertyMetadata(OnSaveInvalidated));

        private readonly Window mWindow;

        private WindowApplicationSettings mWindowApplicationSettings;

        [Browsable(false)]
        public WindowApplicationSettings Settings
        {
            get
            {
                if (mWindowApplicationSettings == null) mWindowApplicationSettings = CreateWindowApplicationSettingsInstance();
                return mWindowApplicationSettings;
            }
        }

        public WindowSettings(Window pWindow)
        {
            mWindow = pWindow;
        }

        public static void SetSave(DependencyObject pDependencyObject, bool pEnabled)
        {
            pDependencyObject.SetValue(SaveProperty, pEnabled);
        }

        protected virtual WindowApplicationSettings CreateWindowApplicationSettingsInstance()
        {
            return new WindowApplicationSettings(this);
        }

        /// <summary>
        /// Load the Window Size Location and State from the settings object
        /// </summary>
        protected virtual void LoadWindowState()
        {
            Settings.Reload();
            if (Settings.Location != Rect.Empty)
            {
                mWindow.Left = Settings.Location.Left;
                mWindow.Top = Settings.Location.Top;
                mWindow.Width = Settings.Location.Width;
                mWindow.Height = Settings.Location.Height;
            }

            if (Settings.WindowState != WindowState.Maximized) mWindow.WindowState = Settings.WindowState;
        }

        /// <summary>
        /// Save the Window Size, Location and State to the settings object
        /// </summary>
        protected virtual void SaveWindowState()
        {
            Settings.WindowState = mWindow.WindowState;
            Settings.Location = mWindow.RestoreBounds;
            Settings.Save();
        }

        /// <summary>
        /// Called when Save is changed on an object.
        /// </summary>
        private static void OnSaveInvalidated(DependencyObject pDependencyObject, DependencyPropertyChangedEventArgs pDependencyPropertyChangedEventArgs)
        {
            var window = pDependencyObject as Window;
            if (window != null)
                if ((bool) pDependencyPropertyChangedEventArgs.NewValue)
                {
                    var settings = new WindowSettings(window);
                    settings.Attach();
                }
        }

        private void Attach()
        {
            if (mWindow != null)
            {
                mWindow.Closing += WindowClosing;
                mWindow.Initialized += WindowInitialized;
                mWindow.Loaded += WindowLoaded;
            }
        }

        private void WindowClosing(object pSender, CancelEventArgs pCancelEventArgs)
        {
            SaveWindowState();
        }

        private void WindowInitialized(object pSender, EventArgs pEventArgs)
        {
            LoadWindowState();
        }

        private void WindowLoaded(object pSender, RoutedEventArgs pRoutedEventArgs)
        {
            if (Settings.WindowState == WindowState.Maximized) mWindow.WindowState = Settings.WindowState;
        }

        public class WindowApplicationSettings : ApplicationSettingsBase
        {
            [UserScopedSetting]
            public Rect Location
            {
                get
                {
                    if (this["Location"] != null) return (Rect) this["Location"];
                    return Rect.Empty;
                }
                set => this["Location"] = value;
            }

            [UserScopedSetting]
            public WindowState WindowState
            {
                get
                {
                    if (this["WindowState"] != null) return (WindowState) this["WindowState"];
                    return WindowState.Normal;
                }
                set => this["WindowState"] = value;
            }

            public WindowApplicationSettings(WindowSettings pWindowSettings)
            {
            }
        }
    }
}
