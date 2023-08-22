using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using DevExpress.Mvvm.UI.Interactivity;

namespace SAM.WPF.Core.Behaviors
{
    public class AutoResizeBehavior : Behavior<UniformGrid>
    {
        public static readonly DependencyProperty MaxItemWidthProperty =
            DependencyProperty.Register("MaxItemWidth", typeof(double), typeof(AutoResizeBehavior),
            new PropertyMetadata());

        public double MaxItemWidth {
            get { return (double) GetValue(MaxItemWidthProperty); }
            set { SetValue(MaxItemWidthProperty, value); }
        }
        
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.SizeChanged += OnSizeChanged;
            Update();
        }
        
        protected override void OnDetaching() {
            AssociatedObject.SizeChanged -= OnSizeChanged;
            base.OnDetaching();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;

            Update();
        }
        
        void Update()
        {
            var width = AssociatedObject.ActualWidth;
            
            if (width < MaxItemWidth)
            {
                AssociatedObject.Columns = 1;
                return;
            }
            
            var columns = (int) Math.Floor(width / MaxItemWidth);

            AssociatedObject.Columns = columns;

            AssociatedObject.InvalidateVisual();
        }
    }
}
