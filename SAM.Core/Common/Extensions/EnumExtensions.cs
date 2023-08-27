using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAM.Core.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        if (fi is null)
        {
            return string.Empty;
        }

        var descAttributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (descAttributes.Length > 0)
        {
            return descAttributes[0].Description;
        }

        var displayAttributes = (DisplayAttribute[]) fi.GetCustomAttributes(typeof(DisplayAttribute), false);
        if (displayAttributes.Length > 0)
        {
            var displayAttr = displayAttributes[0];

            if (!string.IsNullOrWhiteSpace(displayAttr.Description)) return displayAttr.Description;
            if (!string.IsNullOrWhiteSpace(displayAttr.Name)) return displayAttr.Name;
            if (!string.IsNullOrWhiteSpace(displayAttr.ShortName)) return displayAttr.ShortName;

            return string.Empty;
        }

        return value.ToString();
    }
}
