using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevExpress.Mvvm.UI.Interactivity;

namespace SAM.Behaviors;

public class DataGridColumnHeaderRowBehavior : Behavior<ContentControl>
{
    public static readonly DependencyProperty DataGridProperty =
        DependencyProperty.Register(nameof(DataGrid), typeof(DataGrid), typeof(DataGridColumnHeaderRowBehavior));
    
    public static readonly DependencyProperty ColumnHeaderRowProperty =
        DependencyProperty.Register(nameof(ColumnHeaderRow), typeof(DataGridColumnHeadersPresenter), typeof(DataGridColumnHeaderRowBehavior),
                                            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public DataGrid DataGrid
    {
        get => (DataGrid) GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    public DataGridColumnHeadersPresenter ColumnHeaderRow
    {
        get => (DataGridColumnHeadersPresenter) GetValue(ColumnHeaderRowProperty);
        set => SetValue(ColumnHeaderRowProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        Update();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
    }

    private void Update()
    {
        var columnHeaderRow = FindChild<DataGridColumnHeadersPresenter>(DataGrid); // @"PART_ColumnHeadersPresenter"

        if (columnHeaderRow == null) return;

        // TODO: remove from parent
        var p = columnHeaderRow.Parent as Grid;
        p?.Children.Remove(columnHeaderRow);

        AssociatedObject.Content = columnHeaderRow;
    }

    private T FindChild<T>(DependencyObject parent, string childName = null)
        where T : DependencyObject
    {
        // confirm parent and childName are valid. 
        if (parent == null)
        {
            return null;
        }

        T foundChild = null;

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            if (child is not T childType)
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null)
                {
                    break;
                }
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                // If the child's name is set for search
                if (childType is not FrameworkElement frameworkElement || frameworkElement.Name != childName)
                {
                    continue;
                }

                // if the child's name is of the request name
                foundChild = childType;
                break;
            }
            else
            {
                // child element found.
                foundChild = childType;
                break;
            }
        }

        return foundChild;
    }
}
