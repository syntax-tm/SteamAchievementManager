using System;
using System.ComponentModel.DataAnnotations;

namespace SAM.WPF.Core.Extensions
{
    public static class EnumExtensions
    {

        public static string GetDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            if (fi == null)
            {
                return string.Empty;
            }

            var attributes = (DisplayAttribute[]) fi.GetCustomAttributes(typeof(DisplayAttribute), false);
            return attributes.Length > 0
                       ? attributes[0].Description
                       : value.ToString();
        }

    }
}
