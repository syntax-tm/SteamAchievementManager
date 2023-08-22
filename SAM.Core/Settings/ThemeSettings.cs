using System.Windows.Media;
using Newtonsoft.Json;

namespace SAM.Core.Settings
{
    [JsonObject]
    public class ThemeSettings
    {

        private const string ACCENT_COLOR = "#F73541";

        public SystemAppTheme SystemAppTheme { get; set; }
        public string AccentColor { get; set; }
        public Color Accent => ColorHelper.ToMediaColor(ACCENT_COLOR);

        public static ThemeSettings Default =>
            new()
            {
                SystemAppTheme = SystemAppTheme.Dark,
                AccentColor = ACCENT_COLOR
            };
    }
}
