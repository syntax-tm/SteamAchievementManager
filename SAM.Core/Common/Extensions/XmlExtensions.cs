using System;
using System.Diagnostics;
using System.Xml;

namespace SAM.Core.Extensions
{
    public static class XmlExtensions
    {
        public static decimal GetValueAsDecimal(this XmlNode node, string path)
        {
            // if they didn't start with a slash, prepend two so the xpath can be
            // relative from any node
            if (!path.StartsWith("/"))
            {
                path = $"//{path}";
            }

            var selectedNode = node.SelectSingleNode(path);

            Debug.Assert(selectedNode != null, nameof(selectedNode) + " != null");

            return decimal.Parse(selectedNode.InnerText);
        }

        // TODO: add support for string values ("true" and "false")
        public static bool GetValueAsBool(this XmlNode node, string path)
        {
            // if they didn't start with a slash, prepend two so the xpath can be
            // relative from any node
            if (!path.StartsWith("/"))
            {
                path = $"//{path}";
            }

            var selectedNode = node.SelectSingleNode(path);

            Debug.Assert(selectedNode != null, nameof(selectedNode) + " != null");

            var nodeValue = int.Parse(selectedNode.InnerText);

            return Convert.ToBoolean(nodeValue);
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
