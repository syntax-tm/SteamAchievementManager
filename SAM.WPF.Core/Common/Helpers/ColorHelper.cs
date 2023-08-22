using System;
using System.Drawing;
using System.Text.RegularExpressions;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    public static class ColorHelper
    {
        private const string HTML_COLOR_REGEX = @"^\#?(?:[a-fA-F0-9]{3,8})$";

        public static Color ToColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

            if (Enum.TryParse<KnownColor>(value, true, out var knownColor))
            {
                return Color.FromKnownColor(knownColor);
            }

            if (Regex.IsMatch(value, HTML_COLOR_REGEX))
            {
                return ColorTranslator.FromHtml(value);
            }

            throw new ArgumentException($"Unable to determine the {nameof(Color)} of '{value}'.", nameof(value));
        }

        public static System.Windows.Media.Color ToMediaColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

            var drawingColor = ToColor(value);
            return drawingColor.ToMediaColor();
        }
        
        public static Brush ToBrush(string value)
        {
            var color = ToColor(value);
            return new SolidBrush(color);
        }
    }
}
