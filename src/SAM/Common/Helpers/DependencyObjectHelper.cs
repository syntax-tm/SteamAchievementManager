using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace SAM;

public static class DependencyObjectHelper
{
    public static Collection<T> GetDirectVisualChildren<T>(DependencyObject current) where T : DependencyObject
    {
        if (current == null)
            return null;

        var children = new Collection<T>();

        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(current); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(current, i);
            if (child is not T tChild) continue;
            children.Add(tChild);
        }

        return children;
    }

    public static Collection<T> GetVisualChildren<T>(DependencyObject current) where T : DependencyObject
    {
        if (current == null)
            return null;

        var children = new Collection<T>();
        
        GetVisualChildren(current, children);

        return children;
    }

    private static void GetVisualChildren<T>(DependencyObject current, ICollection<T> children) where T : DependencyObject
    {
        if (current == null) return;

        if (current.GetType() == typeof(T))
        {
            children.Add((T) current);
        }

        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(current); i++)
        {
            GetVisualChildren(System.Windows.Media.VisualTreeHelper.GetChild(current, i), children);
        }
    }
}
