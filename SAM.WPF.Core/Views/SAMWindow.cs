using System.Windows;
using MahApps.Metro.Controls;

namespace SAM.WPF.Core.Views;

public class SAMWindow : MetroWindow
{
    static SAMWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SAMWindow), new FrameworkPropertyMetadata(typeof(SAMWindow)));
    }
}