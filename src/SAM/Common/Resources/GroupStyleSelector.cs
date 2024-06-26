﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SAM.Resources
{
    public class GroupStyleSelector : StyleSelector
    {
        public Style NoGroupHeaderStyle { get; set; }
        public Style DefaultGroupStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var group = item as CollectionViewGroup;
            if (group?.Name == null /* or any other condition */)
            {
                return NoGroupHeaderStyle;
            }
            return DefaultGroupStyle;
        }
    }
}
