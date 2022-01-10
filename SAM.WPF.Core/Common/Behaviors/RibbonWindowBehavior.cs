using System;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity;
using Fluent;
using MahApps.Metro.Controls;

namespace SAM.WPF.Core.Behaviors
{
    public class RibbonWindowBehavior : Behavior<MetroWindow>
    {
#region TitelBar
        
        public static readonly DependencyPropertyKey TitleBarPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TitleBar), typeof(RibbonTitleBar), typeof(RibbonWindowBehavior), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty TitleBarProperty = TitleBarPropertyKey.DependencyProperty;
        
        public RibbonTitleBar TitleBar
        {
            get => (RibbonTitleBar) GetValue(TitleBarProperty);
            private set => SetValue(TitleBarPropertyKey, value);
        }

#endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += WindowOnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Loaded -= WindowOnLoaded;
        }

        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            TitleBar = AssociatedObject.FindChild<RibbonTitleBar>(@"PART_RibbonTitleBar");

            if (TitleBar is null)
            {
                throw new ArgumentNullException(nameof(TitleBar));
            }

            TitleBar.InvalidateArrange();
            TitleBar.UpdateLayout();

            AssociatedObject.ShowSystemMenuOnRightClick = false;
        }
    }
}
