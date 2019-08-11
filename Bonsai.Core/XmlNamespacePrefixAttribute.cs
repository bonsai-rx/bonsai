using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    /// <summary>
    /// Specifies a recommended prefix to associate with a XML namespace identifier
    /// when serializing a workflow file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class XmlNamespacePrefixAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlNamespacePrefixAttribute"/> class
        /// with the specified namespace and prefix.
        /// </summary>
        /// <param name="xmlNamespace">The XML namespace identifier.</param>
        /// <param name="prefix">The recommended prefix for the namespace.</param>
        public XmlNamespacePrefixAttribute(string xmlNamespace, string prefix)
        {
            XmlNamespace = xmlNamespace;
            Prefix = prefix;
        }

        /// <summary>
        /// Gets or sets the XML namespace identifier.
        /// </summary>
        public string XmlNamespace { get; set; }

        /// <summary>
        /// Gets or sets the recommended prefix for the namespace.
        /// </summary>
        public string Prefix { get; set; }
    }
}
