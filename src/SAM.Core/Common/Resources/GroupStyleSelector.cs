﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SAM.Core.Resources
{
	public class GroupStyleSelector : StyleSelector
	{
		public Style NoGroupHeaderStyle
		{
			get; set;
		}
		public Style DefaultGroupStyle
		{
			get; set;
		}

		public override Style SelectStyle (object item, DependencyObject container)
		{
			var group = item as CollectionViewGroup;
			return group?.Name == null ? NoGroupHeaderStyle : DefaultGroupStyle;
		}
	}
}
