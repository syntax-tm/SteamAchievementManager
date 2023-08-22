using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace SAM.WPF.Core
{
    public static class PackIconHelper
    {

        public static ImageSource GetImageSource(PackIconMaterialKind kind, Brush foreground = null)
        {
            foreground ??= new SolidColorBrush(Colors.White);

            var packIcon = new PackIconMaterial { Kind = kind };

            return GetImageSource(packIcon, foreground);
        }

        public static ImageSource GetImageSource(PackIconControlBase packIcon, Brush foreground = null)
        {
            foreground ??= new SolidColorBrush(Colors.White);

            var geo = Geometry.Parse(packIcon.Data);
            var gd = new GeometryDrawing
            {
                Geometry = geo,
                Brush = foreground
            };

            var drawingGroup = new DrawingGroup
            {
                Children = { gd },
                Transform = new ScaleTransform(1, 1)
            };
            
            var img = new DrawingImage { Drawing = drawingGroup };
            img.Freeze();

            return img;
        }

    }
}
