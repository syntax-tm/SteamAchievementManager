using System;
using System.Windows.Controls;

namespace SAM.WPF.Views.Library
{
    public partial class LibraryItemsControlView
    {
        public LibraryItemsControlView()
        {
            InitializeComponent();
        }

        // TODO: this needs to be either in a custom control or a dependency property
        // the scrollviewer's vertical offset when scrolling is too small
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = (ScrollViewer) sender;

            if (e.VerticalChange == 0) return;
            if (Math.Abs(e.VerticalChange) > 70) return;

            var newOffset = (e.VerticalChange * 2) + e.VerticalOffset;
            
            e.Handled = true;

            sv.ScrollToVerticalOffset(newOffset);
        }
    }
}
