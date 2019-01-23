using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an externalized property of a workflow element.
    /// </summary>
    [XmlType("ExternalizedMappingItem", Namespace = Constants.XmlNamespace)]
    public sealed class ExternalizedMapping
    {
        /// <summary>
        /// Gets or sets the member name of the externalized property.
        /// </summary>
        [XmlAttribute]
        [Category("Member")]
        [TypeConverter(typeof(ExternalizedMappingNameConverter))]
        [Description("The member name of the externalized property.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an optional display name of the externalized property.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("")]
        [Category("Design")]
        [Description("The optional display name that will appear on the pages of a nested workflow.")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets an optional description for the externalized property.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("")]
        [Category("Design")]
        [Description("The optional description for the externalized property.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an optional category for the externalized property.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("")]
        [Category("Design")]
        [Description("The optional category used to group the externalized property.")]
        public string Category { get; set; }

        internal string ExternalizedName
        {
            get
            {
                var displayName = DisplayName;
                return string.IsNullOrEmpty(displayName) ? Name : displayName;
            }
        }
    }
}
