using System.Runtime.CompilerServices;

namespace SAM.WPF.Core.Extensions
{
    public static class ColorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color drawingColor)
        {
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Windows.Media.Brush ToBrush(this System.Drawing.Color color)
        {
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
