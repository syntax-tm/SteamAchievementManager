using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity;
using log4net;
using Newtonsoft.Json;
using SAM.Core.Storage;

namespace SAM.Core.Behaviors
{
    public class WindowAutoSaveBehavior : Behavior<Window>
    {
        private bool _initialized;
        private readonly WindowSettings _config;

        public WindowAutoSaveBehavior()
        {
            _config = new ();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            
            LoadSettings();
            
            AssociatedObject.StateChanged += WindowStateChanged;
            AssociatedObject.Closing += WindowClosing;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            
            if (AssociatedObject == null) return;
            
            AssociatedObject.StateChanged -= WindowStateChanged;
            AssociatedObject.Closing -= WindowClosing;

            SaveSettings();
        }

        private void LoadSettings()
        {
            if (_config == null) return;
            if (AssociatedObject == null) return;
            
            AssociatedObject.WindowState = _config.WindowState;
            AssociatedObject.WindowStartupLocation = _config.StartupLocation;
            AssociatedObject.Width = _config.Width;
            AssociatedObject.Height = _config.Height;
            AssociatedObject.Left = _config.X;
            AssociatedObject.Top = _config.Y;

            _initialized = true;
        }

        private void SaveSettings()
        {
            if (_config == null || !_initialized) return;

            var state = AssociatedObject.WindowState;

            _config.WindowState = state;
            _config.StartupLocation = AssociatedObject.WindowStartupLocation;
            
            if (state == WindowState.Normal)
            {
                _config.Width = AssociatedObject.Width;
                _config.Height = AssociatedObject.Height;
                _config.X = AssociatedObject.Left;
                _config.Y = AssociatedObject.Top;
            }

            _config.Save();
        }
        
        private void WindowStateChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }
        
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }
    }

    public class WindowSettings
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WindowSettings));

        private static WindowSettings _default;

        private readonly string _key;
        private readonly string _fileName;
        private readonly object syncLock = new();
        
        public WindowState WindowState { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public WindowStartupLocation StartupLocation { get; set; } = WindowStartupLocation.Manual;
        
        public WindowSettings()
        {
            var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;
            var fullName = nameof(WindowSettings);

            var settingsKey = $"{assemblyName}_{fullName}";

            _key = settingsKey;
            _fileName = $"{_key}.json";

            Load();
        }
        
        public static WindowSettings Default
        {
            get
            {
                if (_default is not null) return _default;

                var defaultResolution = SystemParameters.WorkArea;

                _default = new()
                {
                    Width = Math.Min(1280, defaultResolution.Width),
                    Height = Math.Min(720, defaultResolution.Height),
                    WindowState = WindowState.Normal,
                    StartupLocation = WindowStartupLocation.CenterScreen
                };

                return _default;
            }
        }

        public void Load()
        {
            try
            {
                var exists = CacheManager.StorageManager.FileExists(_fileName);
                if (!exists)
                {
                    Width = Default.Width;
                    Height = Default.Height;
                    WindowState = Default.WindowState;
                    StartupLocation = Default.StartupLocation;

                    return;
                }

                var configText = CacheManager.StorageManager.GetTextFile(_fileName);
                JsonConvert.PopulateObject(configText, this);
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to load the {nameof(WindowSettings)} for '{_key}'. {e.Message}";
                log.Error(message, e);
            }
        }

        public void Save()
        {
            try
            {
                lock (syncLock)
                {
                    var configText = JsonConvert.SerializeObject(this, Formatting.None);

                    CacheManager.StorageManager.SaveText(_fileName, configText);

                    log.Debug($"Saved {nameof(WindowSettings)} to '{_fileName}'.");
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to save the {nameof(WindowSettings)} for '{_key}'. {e.Message}";
                log.Error(message, e);
            }
        }
    }
}
