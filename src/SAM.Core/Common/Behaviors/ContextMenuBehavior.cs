using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Mvvm.UI.Interactivity;

namespace SAM.Core.Behaviors;

public class ContextMenuBehaviour : Behavior<Control>
{
    private const int DEFAULT_DELAY_IN_MS = 500;

    public static readonly DependencyProperty ShowOnMouseOverProperty =
        DependencyProperty.Register(nameof(ShowOnMouseOver), typeof(bool), typeof(ContextMenuBehaviour));
    
    public static readonly DependencyProperty ShowOnMouseOverDelayProperty =
        DependencyProperty.Register(nameof(ShowOnMouseOverDelay), typeof(int), typeof(ContextMenuBehaviour), new (DEFAULT_DELAY_IN_MS));

    public static readonly DependencyProperty ShowOnLeftMouseDownProperty =
        DependencyProperty.Register(nameof(ShowOnLeftMouseDown), typeof(bool), typeof(ContextMenuBehaviour));
    
    private DispatcherTimer _timer;

    public bool ShowOnMouseOver
    {
        get => (bool) GetValue(ShowOnMouseOverProperty);
        set => SetValue(ShowOnMouseOverProperty, value);
    }
    
    public int ShowOnMouseOverDelay
    {
        get => (int) GetValue(ShowOnMouseOverDelayProperty);
        set => SetValue(ShowOnMouseOverDelayProperty, value);
    }

    public bool ShowOnLeftMouseDown
    {
        get => (bool) GetValue(ShowOnLeftMouseDownProperty);
        set => SetValue(ShowOnLeftMouseDownProperty, value);
    }
        
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += AssociatedObjectOnMouseEnter;
        AssociatedObject.MouseLeave += AssociatedObjectOnMouseLeave;
        AssociatedObject.MouseLeftButtonDown += AssociatedObjectOnMouseLeftButtonDown;
    }
    
    protected override void OnDetaching()
    {
        AssociatedObject.MouseEnter -= AssociatedObjectOnMouseEnter;
        AssociatedObject.MouseLeave -= AssociatedObjectOnMouseLeave;
        AssociatedObject.MouseLeftButtonDown -= AssociatedObjectOnMouseLeftButtonDown;
        base.OnDetaching();
    }

    private void AssociatedObjectOnMouseLeave(object sender, MouseEventArgs e)
    {
        StopTimer();
        
        e.Handled = true;
    }

    private void AssociatedObjectOnMouseEnter(object sender, MouseEventArgs e)
    {
        StartTimer();

        e.Handled = true;
    }

    private void AssociatedObjectOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ShowContextMenu();

        e.Handled = true;
    }

    private void StartTimer()
    {
        _timer = new (DispatcherPriority.Background);
        _timer.Interval = TimeSpan.FromMilliseconds(ShowOnMouseOverDelay);
        _timer.Tick += TimerOnTick;
        _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= TimerOnTick;
        }

        _timer = null;
    }
    
    private void TimerOnTick(object sender, EventArgs e)
    {
        StopTimer();

        ShowContextMenu();
    }

    private void ShowContextMenu()
    {
        var menu = AssociatedObject?.ContextMenu;

        if (menu == null) return;

        menu.PlacementTarget = AssociatedObject;
        menu.IsOpen = true;
    }
}
