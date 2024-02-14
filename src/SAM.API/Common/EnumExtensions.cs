using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SAM.API;

public static class EnumExtensions
{
	internal static string GetDisplayName (this Enum value)
	{
		var fi = value.GetType().GetField(value.ToString());
		if (fi is null)
		{
			return string.Empty;
		}

		var displayName = fi.GetAttribute<DisplayNameAttribute>()?.DisplayName;
		if (!string.IsNullOrEmpty(displayName))
		{
			return displayName;
		}

		var display = fi.GetAttribute<DisplayAttribute>();
		if (display != null)
		{
			if (!string.IsNullOrEmpty(display.Name))
			{
				return display.Name;
			}

			if (!string.IsNullOrEmpty(display.ShortName))
			{
				return display.ShortName;
			}

			if (!string.IsNullOrEmpty(display.Description))
			{
				return display.Description;
			}
		}

		var description = fi.GetAttribute<DescriptionAttribute>()?.Description;
		return !string.IsNullOrEmpty(description) ? description : value.ToString();
	}

	internal static string GetDescription (this Enum value)
	{
		var fi = value.GetType().GetField(value.ToString());
		if (fi is null)
		{
			return string.Empty;
		}

		var description = fi.GetAttribute<DescriptionAttribute>()?.Description;
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}

		var display = fi.GetAttribute<DisplayAttribute>();
		if (display != null)
		{
			if (!string.IsNullOrEmpty(display.Description))
			{
				return display.Description;
			}

			if (!string.IsNullOrEmpty(display.Name))
			{
				return display.Name;
			}

			if (!string.IsNullOrEmpty(display.ShortName))
			{
				return display.ShortName;
			}
		}

		var displayName = fi.GetAttribute<DisplayNameAttribute>()?.DisplayName;
		return !string.IsNullOrEmpty(displayName) ? displayName : value.ToString();
	}

	private static T GetAttribute<T> (this ICustomAttributeProvider fi)
		where T : Attribute
	{
		var attributes = fi.GetAttributes<T>();
		return attributes.FirstOrDefault();
	}

	private static T [] GetAttributes<T> (this ICustomAttributeProvider fi)
		where T : Attribute
	{
		var attributes = fi.GetCustomAttributes(typeof(T), false) as T [];
		return attributes;
	}
}
