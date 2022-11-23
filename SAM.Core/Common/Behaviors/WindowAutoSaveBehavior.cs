﻿using System;
using System.ComponentModel;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Interactivity;
using log4net;
using Newtonsoft.Json;

namespace SAM.Core.Behaviors
{
    public class WindowAutoSaveBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty ConfigProperty = DependencyProperty.Register(nameof(Config), typeof(WindowSettings), typeof(WindowAutoSaveBehavior), new (OnConfigPropertyChanged));

        public WindowSettings Config
        {
            get => (WindowSettings) GetValue(ConfigProperty);
            set => SetValue(ConfigProperty, value);
        }

        private static void OnConfigPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = d as WindowAutoSaveBehavior;
            if (e.NewValue is not null)
            {
                b?.LoadSettings();
            }
        }

        private bool _initialized;

        protected override void OnAttached()
        {
            base.OnAttached();
            
            AssociatedObject.SizeChanged += WindowSizedChanged;
            AssociatedObject.StateChanged += WindowStateChanged;
            AssociatedObject.Closing += WindowClosing;
            
            LoadSettings();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            
            if (AssociatedObject == null) return;

            AssociatedObject.SizeChanged -= WindowSizedChanged;
            AssociatedObject.StateChanged -= WindowStateChanged;
            AssociatedObject.Closing -= WindowClosing;
            
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (Config == null) return;
            if (AssociatedObject == null) return;
            
            AssociatedObject.WindowState = Config.WindowState;
            AssociatedObject.WindowStartupLocation = Config.StartupLocation;
            AssociatedObject.Width = Config.Width;
            AssociatedObject.Height = Config.Height;

            _initialized = true;
        }

        private void SaveSettings()
        {
            if (Config == null || !_initialized) return;

            var state = AssociatedObject.WindowState;

            Config.WindowState = state;
            Config.StartupLocation = AssociatedObject.WindowStartupLocation;
            
            if (state == WindowState.Normal)
            {
                Config.Width = AssociatedObject.Width;
                Config.Height = AssociatedObject.Height;
            }

            Config.Save();
        }
        
        private void WindowSizedChanged(object sender, SizeChangedEventArgs e)
        {
            SaveSettings();
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

    public class WindowSettings : BindableBase
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WindowSettings));

        private static WindowSettings _default;

        private readonly string _key;
        private readonly string _fileName;
        private readonly object syncLock = new();
        
        [JsonProperty]
        public WindowState WindowState
        {
            get => GetProperty(() => WindowState);
            set => SetProperty(() => WindowState, value);
        }
        
        [JsonProperty]
        public double Width
        {
            get => GetProperty(() => Width);
            set => SetProperty(() => Width, value);
        }
        
        [JsonProperty]
        public double Height
        {
            get => GetProperty(() => Height);
            set => SetProperty(() => Height, value);
        }
        
        [JsonProperty]
        public WindowStartupLocation StartupLocation
        {
            get => GetProperty(() => StartupLocation);
            set => SetProperty(() => StartupLocation, value);
        }

        [JsonConstructor]
        protected WindowSettings()
        {

        }

        public WindowSettings(string key)
        {
            _key = key;
            _fileName = $"{_key}.json";

            Load();
        }

        public static WindowSettings Default
        {
            get
            {
                if (_default is not null) return _default;

                _default = new()
                {
                    Width = 1280,
                    Height = 720,
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
                var exists = IsolatedStorageManager.FileExists(_fileName);
                if (!exists)
                {
                    var defaults = Default;
                    
                    Width = defaults.Width;
                    Height = defaults.Height;
                    WindowState = defaults.WindowState;
                    StartupLocation = defaults.StartupLocation;

                    return;
                }

                var configText = IsolatedStorageManager.GetTextFile(_fileName);
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

                    IsolatedStorageManager.SaveText(_fileName, configText);
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
