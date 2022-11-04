using System;
using System.ComponentModel.DataAnnotations;

namespace SAM.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            if (fi is null)
            {
                return string.Empty;
            }

            var attributes = (DisplayAttribute[]) fi.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attributes.Length > 0)
            {
                var displayAttr = attributes[0];

                if (!string.IsNullOrWhiteSpace(displayAttr.Description)) return displayAttr.Description;
                if (!string.IsNullOrWhiteSpace(displayAttr.Name)) return displayAttr.Name;
                if (!string.IsNullOrWhiteSpace(displayAttr.ShortName)) return displayAttr.ShortName;
            }

            return value.ToString();
        }
    }
}
