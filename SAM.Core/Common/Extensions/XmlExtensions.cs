using System;
using System.Globalization;
using System.Xml;

namespace SAM.Core.Extensions
{
    public static class XmlExtensions
    {
        private static readonly CultureInfo _enUsCulture = CultureInfo.GetCultureInfo("en-US");
        
        public static decimal GetValueAsDecimal(this XmlNode node, string path)
        {
            var nodeText = GetValue(node, path);

            if (!decimal.TryParse(nodeText, NumberStyles.Any, CultureInfo.CurrentCulture, out var result) &&
                !decimal.TryParse(nodeText, NumberStyles.Any, _enUsCulture, out result) &&
                !decimal.TryParse(nodeText, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = default;
            }

            return result;
        }

        // TODO: add support for string values ("true" and "false")
        public static bool GetValueAsBool(this XmlNode node, string path)
        {
            var nodeText = GetValue(node, path);
            
            if (!int.TryParse(nodeText, NumberStyles.Any, CultureInfo.CurrentCulture, out var result) &&
                !int.TryParse(nodeText, NumberStyles.Any, _enUsCulture, out result) &&
                !int.TryParse(nodeText, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = default;
            }

            return Convert.ToBoolean(result);
        }

        public static string GetValue(this XmlNode node, string path)
        {
            // if they didn't start with a slash, prepend two so the xpath can be
            // relative from any node
            if (!path.StartsWith("/"))
            {
                path = $"//{path}";
            }

            var selectedNode = node.SelectSingleNode(path);

            return selectedNode?.InnerText;
        }
    }
}
