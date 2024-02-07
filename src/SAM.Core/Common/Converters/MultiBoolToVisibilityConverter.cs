using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SAM.Core.Converters
{
	[DefaultValue(All)]
	public enum MultiBoolEvaluationMode
	{
		All = 0,
		Any,
		None
	}

	public class MultiBoolToVisibilityConverter : IMultiValueConverter
	{
		public MultiBoolEvaluationMode Mode
		{
			get; set;
		}
		public bool Inverse
		{
			get; set;
		}
		public bool HiddenInsteadOfCollapsed
		{
			get; set;
		}

		public object Convert (object [] values, Type targetType, object parameter, CultureInfo culture)
		{
			var boolValues = values.Where(v => v is bool).Cast<bool>();

			var result = Mode switch
			{
				MultiBoolEvaluationMode.All => boolValues.All(b => b),
				MultiBoolEvaluationMode.Any => boolValues.Any(b => b),
				MultiBoolEvaluationMode.None => !boolValues.All(b => b),
				_ => throw new ArgumentOutOfRangeException(nameof(Mode))
			};

			if (Inverse)
			{
				result = !result;
			}

			return result
				? Visibility.Visible
				: HiddenInsteadOfCollapsed
					? Visibility.Hidden
					: Visibility.Collapsed;
		}

		public object [] ConvertBack (object value, Type [] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class MultiBoolToVisibilityConverterExtension : MarkupExtension
	{
		public MultiBoolEvaluationMode Mode
		{
			get; set;
		}
		public bool Inverse
		{
			get; set;
		}
		public bool HiddenInsteadOfCollapsed
		{
			get; set;
		}
		public IValueConverter ItemConverter
		{
			get; set;
		}

		public MultiBoolToVisibilityConverterExtension ()
		{

		}

		public MultiBoolToVisibilityConverterExtension (IValueConverter itemConverter)
		{
			ItemConverter = itemConverter;
		}

		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			return new MultiBoolToVisibilityConverter
			{
				Mode = Mode,
				Inverse = Inverse,
				HiddenInsteadOfCollapsed = HiddenInsteadOfCollapsed
			};
		}
	}
}
