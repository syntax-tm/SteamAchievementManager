using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace SAM.Controls;

public class MenuHeaderItem : MenuItem
{
    static MenuHeaderItem()
    {
        IsEnabledProperty.OverrideMetadata(typeof(MenuHeaderItem), new FrameworkPropertyMetadata(false));
        BorderBrushProperty.OverrideMetadata(typeof(MenuHeaderItem), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent)));
        BorderThicknessProperty.OverrideMetadata(typeof(MenuHeaderItem), new FrameworkPropertyMetadata(new Thickness(0)));
    }
}
