using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using ControlzEx.Theming;
using Newtonsoft.Json;
using SAM.WPF.Core.Themes;

namespace SAM.WPF.Core.Settings
{
    [JsonObject]
    public class ThemeSettings
    {

        public SystemAppTheme SystemAppTheme { get; set; }
        public string AccentColor { get; set; }
        public Color Accent => ColorHelper.ToMediaColor("#F73541");

        public static ThemeSettings Default
        {
            get
            {
                return new ThemeSettings
                {
                    SystemAppTheme = SystemAppTheme.Dark,
                    AccentColor = "#F73541"
                };
            }
        }

    }
}
