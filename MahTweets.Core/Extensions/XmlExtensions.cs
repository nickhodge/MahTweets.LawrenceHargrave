using System.Xml;

namespace MahTweets.Core.Extensions
{
    /// <summary>
    /// Helper Class for Manipulating XML
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Adds a new Child Node to the specified Node using the default Namespace.
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static XmlNode AddChild(this XmlNode ParentNode, string Name)
        {
            var xmlDocument = ParentNode as XmlDocument;
            if (xmlDocument != null)
            {
                XmlNode node = (xmlDocument).CreateNode(XmlNodeType.Element, Name, ParentNode.NamespaceURI);
                ParentNode.AppendChild(node);
                return node;
            }
            if (ParentNode.OwnerDocument != null)
            {
                XmlNode node = ParentNode.OwnerDocument.CreateNode(XmlNodeType.Element, Name,
                                                                   ParentNode.OwnerDocument.NamespaceURI);
                ParentNode.AppendChild(node);
                return node;
            }
            return null;
        }

        /// <summary>
        /// Adds a new Attribute to the specified Node using the default namespace for the document.
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static XmlAttribute AddAttribute(this XmlNode ParentNode, string Name)
        {
            if (ParentNode.OwnerDocument != null)
            {
                XmlAttribute attribute = ParentNode.OwnerDocument.CreateAttribute(Name,
                                                                                  ParentNode.OwnerDocument.NamespaceURI);
                if (ParentNode.Attributes != null) ParentNode.Attributes.Append(attribute);
                return attribute;
            }
            return null;
        }

        /// <summary>
        /// Returns True if the Node itself is null, or the Node.InnerText (after trimming) is null or empty
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this XmlNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.InnerText.Trim()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the InnerText of the Node. If the node InnerText is Null or Empty, it returns an empty string.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string InnerTextOrDefault(this XmlNode node)
        {
            return node.InnerTextOrDefault(string.Empty);
        }

        /// <summary>
        /// Returns the InnerText of the Node. If the node InnerText is Null or Empty, it returns the string in Default; 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="Default">String to return if Node Inntertext </param>
        /// <returns></returns>
        public static string InnerTextOrDefault(this XmlNode node, string Default)
        {
            if (node.IsNullOrEmpty())
            {
                return Default;
            }
            return node.InnerText;
        }
    }
}